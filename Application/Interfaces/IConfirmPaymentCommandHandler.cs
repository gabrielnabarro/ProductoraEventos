using Application.DTOs;
using Application.UseCases.Payments.Commands.ConfirmPayment;

namespace Application.Interfaces
{
    public interface IConfirmPaymentCommandHandler
    {
        Task<PaymentResponseDto> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken = default);
    }
}
