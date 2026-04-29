using Application.Interfaces;
using Application.DTOs;
using Application.UseCases.Reservations.Commands.CreateReservation;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ICreateReservationCommandHandler _createReservationCommandHandler;

    public ReservationsController(ICreateReservationCommandHandler createReservationCommandHandler)
    {
        _createReservationCommandHandler = createReservationCommandHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> CreateReservation([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var response = await _createReservationCommandHandler.Handle(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
