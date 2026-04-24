using Domain.Constants;

namespace Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Guid SeatId { get; set; }
        public string Status { get; set; } = ReservationStatuses.Reserved;
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public User? User { get; set; }
        public Seat? Seat { get; set; }
    }
}
