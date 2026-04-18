using Application.UseCases.Events.Queries;
using Application.UseCases.Events.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
   
        [ApiController]
        [Route("api/v1/events")]
        public class EventsController : ControllerBase
        {
            private readonly GetEventsHandler _getEventsHandler;
            private readonly GetSectorsByEventHandler _getSectorsHandler;
            private readonly GetSeatsBySectorHandler _getSeatsHandler;

            public EventsController(
                GetEventsHandler getEventsHandler,
                GetSectorsByEventHandler getSectorsHandler,
                GetSeatsBySectorHandler getSeatsHandler)
            {
                _getEventsHandler = getEventsHandler;
                _getSectorsHandler = getSectorsHandler;
                _getSeatsHandler = getSeatsHandler;
            }

            [HttpGet]
            public async Task<IActionResult> GetEvents()
            {
                var events = await _getEventsHandler.Handle(new GetEventsQuery());
                return Ok(events);
            }

            [HttpGet("{eventId}/sectors")]
            public async Task<IActionResult> GetSectors(int eventId)
            {
                var sectors = await _getSectorsHandler.Handle(new GetSectorsByEventQuery { EventId = eventId });
                return Ok(sectors);
            }

            [HttpGet("{eventId}/sectors/{sectorId}/seats")]
            public async Task<IActionResult> GetSeats(int eventId, int sectorId)
            {
                var seats = await _getSeatsHandler.Handle(new GetSeatsBySectorQuery { SectorId = sectorId });
                return Ok(seats);
            }
        }
}
