using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public sealed class PaymentResponseDto
    {
        public Guid ReservationId { get; init; }
        public Guid SeatId { get; init; }
        public int UserId { get; init; }
        public string SeatStatus { get; init; } = string.Empty;
        public string ReservationStatus { get; init; } = string.Empty;
        public DateTime ProcessedAt { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
