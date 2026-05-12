using Application.DTOs;
using Application.UseCases.Payments.Commands.ConfirmPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IConfirmPaymentCommandHandler
    {
        Task<PaymentResponseDto> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken = default);
    }
}
