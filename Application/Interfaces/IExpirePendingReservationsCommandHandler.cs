using Application.UseCases.Reservations.Commands.ExpirePendingReservations;

namespace Application.Interfaces;

public interface IExpirePendingReservationsCommandHandler
{
    Task<int> Handle(ExpirePendingReservationsCommand command, CancellationToken cancellationToken = default);
}
