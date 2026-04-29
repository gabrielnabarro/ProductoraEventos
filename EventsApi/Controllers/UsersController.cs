using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Reservations.Queries.GetUserReservations;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IGetUserReservationsQueryHandler _getUserReservationsQueryHandler;

    public UsersController(IGetUserReservationsQueryHandler getUserReservationsQueryHandler)
    {
        _getUserReservationsQueryHandler = getUserReservationsQueryHandler;
    }

    [HttpGet("{userId:int}/reservations")]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<UserReservationResponseDto>>> GetReservations(int userId, [FromQuery] int? eventId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var response = await _getUserReservationsQueryHandler.Handle(
            new GetUserReservationsQuery
            {
                UserId = userId,
                EventId = eventId,
                Status = status
            },
            cancellationToken);

        return Ok(response);
    }
}
