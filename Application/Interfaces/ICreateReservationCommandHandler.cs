using Application.DTOs;
using Application.UseCases.Reservations.Commands.CreateReservation;

namespace Application.Interfaces;

public interface ICreateReservationCommandHandler
{
    Task<ReservationResponseDto> Handle(CreateReservationCommand command, CancellationToken cancellationToken = default);
}
