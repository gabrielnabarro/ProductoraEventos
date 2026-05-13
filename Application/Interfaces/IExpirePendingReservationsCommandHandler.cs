using Application.UseCases.Reservations.Commands.ExpirePendingReservations;

namespace Application.Interfaces;

public interface IExpirePendingReservationsCommandHandler
{
    Task<ExpirePendingReservationsResult> Handle(ExpirePendingReservationsCommand command, CancellationToken cancellationToken = default);
}
