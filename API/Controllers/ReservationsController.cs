using Application.UseCases.Reservations.Commands;
using Application.UseCases.Reservations.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly CreateReservationHandler _createReservationHandler;

        public ReservationsController(CreateReservationHandler createReservationHandler)
        {
            _createReservationHandler = createReservationHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
        {
            var result = await _createReservationHandler.Handle(command);

            if (!result) return BadRequest("El asiento no está disponible o no existe.");

            return Ok(new { message = "Reserva exitosa. Tiene 5 minutos para completar el pago." });
        }
    }
}
