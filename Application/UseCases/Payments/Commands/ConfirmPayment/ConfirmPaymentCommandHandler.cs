using Application.DTOs;
using Application.Common;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Payments.Commands.ConfirmPayment
{
    public sealed class ConfirmPaymentCommandHandler : IConfirmPaymentCommandHandler
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmPaymentCommandHandler(
            IReservationRepository reservationRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _reservationRepository = reservationRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentResponseDto> Handle(
            ConfirmPaymentCommand command,
            CancellationToken cancellationToken = default)
        {
            ValidateCommand(command);

            var attemptTimestamp = DateTime.UtcNow;

            // Registro de intento de pago ANTES de la transacción (siempre persiste)
            await _auditLogRepository.AddAsync(
                BuildAuditLog(command.UserId, command.ReservationId,
                              AuditLogActions.PaymentAttempt,
                              "Se recibio un intento de pago para la reserva.",
                              attemptTimestamp),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Obtener la reserva con butaca cargada (con tracking para que EF detecte los cambios)
            var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);

            if (reservation is null)
            {
                await LogFailureAsync(command.UserId, command.ReservationId,
                                      "Reserva no encontrada.", cancellationToken);
                throw new NotFoundException("Reserva no encontrada.");
            }

            if (reservation.UserId != command.UserId)
            {
                await LogFailureAsync(command.UserId, command.ReservationId,
                                      "La reserva no pertenece al usuario indicado.", cancellationToken);
                throw new UnauthorizedException("No tienes permiso para pagar esta reserva.");
            }

            if (!string.Equals(reservation.Status, Domain.Constants.ReservationStatuses.Pending,
                                StringComparison.OrdinalIgnoreCase))
            {
                await LogFailureAsync(command.UserId, command.ReservationId,
                                      $"La reserva ya se encuentra en estado '{reservation.Status}'.", cancellationToken);
                throw new ConflictException($"La reserva ya fue procesada (estado: {reservation.Status}).");
            }

            if (reservation.IsExpired(DateTime.UtcNow))
            {
                await LogFailureAsync(command.UserId, command.ReservationId,
                                      "La reserva ha expirado.", cancellationToken);
                throw new ConflictException("La reserva ha expirado y ya no puede procesarse.");
            }

            if (reservation.Seat is null)
            {
                await LogFailureAsync(command.UserId, command.ReservationId,
                                      "No se pudo cargar la butaca asociada a la reserva.", cancellationToken);
                throw new ValidationException("La reserva no tiene una butaca asociada valida.");
            }


            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var processedAt = DateTime.UtcNow;

                // Marcar butaca como Vendida
                reservation.Seat.Sell();

                // Marcar reserva como Pagada
                reservation.Pay();

                //Registrar auditoría de pago exitoso (dentro de la misma transacción)
                await _auditLogRepository.AddAsync(
                    BuildAuditLog(command.UserId, command.ReservationId,
                                  AuditLogActions.PaymentSuccess,
                                  $"Pago confirmado. Butaca {reservation.SeatId} marcada como Vendida.",
                                  processedAt),
                    cancellationToken);


                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new PaymentResponseDto
                {
                    ReservationId = reservation.Id,
                    SeatId = reservation.SeatId,
                    UserId = reservation.UserId,
                    SeatStatus = reservation.Seat.Status,
                    ReservationStatus = reservation.Status,
                    ProcessedAt = UtcDateTime.Normalize(processedAt),
                    Message = "Pago procesado correctamente. La butaca ha sido vendida."
                };
            }
            catch
            {
                // Rollback completo: ninguno de los tres cambios queda persistido
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                await LogFailureAsync(command.UserId, command.ReservationId,
                                      "El pago fallo y se ejecuto un rollback de la operacion.", cancellationToken);
                throw;
            }
        }



        private async Task LogFailureAsync(
            int userId, Guid reservationId, string details, CancellationToken cancellationToken)
        {
            await _auditLogRepository.AddAsync(
                BuildAuditLog(userId, reservationId, AuditLogActions.PaymentFailed, details, DateTime.UtcNow),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static AuditLog BuildAuditLog(
            int userId, Guid reservationId, string action, string details, DateTime timestamp)
        {
            return new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = AuditLogEntityTypes.Reservation,
                EntityId = reservationId.ToString(),
                Details = details,
                CreatedAt = timestamp
            };
        }

        private static void ValidateCommand(ConfirmPaymentCommand command)
        {
            if (command.ReservationId == Guid.Empty)
                throw new ValidationException("El identificador de la reserva es obligatorio.");

            if (command.UserId <= 0)
                throw new ValidationException("El identificador del usuario debe ser mayor a cero.");
        }
    }
}
