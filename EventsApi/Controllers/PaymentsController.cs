using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Payments.Commands.ConfirmPayment;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("api/v1/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IConfirmPaymentCommandHandler _confirmPaymentCommandHandler;

    public PaymentsController(IConfirmPaymentCommandHandler confirmPaymentCommandHandler)
    {
        _confirmPaymentCommandHandler = confirmPaymentCommandHandler;
    }


    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponseDto>> ConfirmPayment(
        [FromBody] ConfirmPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _confirmPaymentCommandHandler.Handle(command, cancellationToken);
        return Ok(response);
    }
}