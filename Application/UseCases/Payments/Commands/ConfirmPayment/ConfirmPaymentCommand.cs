
namespace Application.UseCases.Payments.Commands.ConfirmPayment
{
    public sealed class ConfirmPaymentCommand
    {
        public Guid ReservationId { get; init; }
        public int UserId { get; init; }
    }
}
