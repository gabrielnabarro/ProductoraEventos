using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Payments.Commands.ConfirmPayment
{
    public sealed class ConfirmPaymentCommand
    {
        public Guid ReservationId { get; init; }
        public int UserId { get; init; }
    }
}
