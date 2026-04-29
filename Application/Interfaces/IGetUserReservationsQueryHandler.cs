using Application.DTOs;
using Application.UseCases.Reservations.Queries.GetUserReservations;

namespace Application.Interfaces;

public interface IGetUserReservationsQueryHandler
{
    Task<IReadOnlyCollection<UserReservationResponseDto>> Handle(GetUserReservationsQuery query, CancellationToken cancellationToken = default);
}
