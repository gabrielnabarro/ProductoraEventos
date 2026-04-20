using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Events.Queries.GetAllEvents;
using Application.UseCases.Events.Queries.GetEventById;
using Application.UseCases.Events.Queries.GetEventSeatMap;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventsController : ControllerBase
{
    private readonly IGetAllEventsQueryHandler _getAllEventsQueryHandler;
    private readonly IGetEventByIdQueryHandler _getEventByIdQueryHandler;
    private readonly IGetEventSeatMapQueryHandler _getEventSeatMapQueryHandler;

    public EventsController(
        IGetAllEventsQueryHandler getAllEventsQueryHandler,
        IGetEventByIdQueryHandler getEventByIdQueryHandler,
        IGetEventSeatMapQueryHandler getEventSeatMapQueryHandler)
    {
        _getAllEventsQueryHandler = getAllEventsQueryHandler;
        _getEventByIdQueryHandler = getEventByIdQueryHandler;
        _getEventSeatMapQueryHandler = getEventSeatMapQueryHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<EventResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<EventResponseDto>>> GetEvents([FromQuery] GetAllEventsQuery query, CancellationToken cancellationToken)
    {
        var response = await _getAllEventsQueryHandler.Handle(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{eventId:int}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponseDto>> GetEventById(int eventId, CancellationToken cancellationToken)
    {
        var response = await _getEventByIdQueryHandler.Handle(new GetEventByIdQuery { EventId = eventId }, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{eventId:int}/seats")]
    [ProducesResponseType(typeof(EventSeatMapResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventSeatMapResponseDto>> GetEventSeatMap(int eventId, CancellationToken cancellationToken)
    {
        var response = await _getEventSeatMapQueryHandler.Handle(new GetEventSeatMapQuery { EventId = eventId }, cancellationToken);
        return Ok(response);
    }
}
