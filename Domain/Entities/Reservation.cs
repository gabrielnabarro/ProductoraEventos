using Domain.Constants;

namespace Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Guid SeatId { get; set; }
        public string Status { get; set; } = ReservationStatuses.Pending;
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public User? User { get; set; }
        public Seat? Seat { get; set; }

        public bool IsForSeat(Guid seatId)
        {
            return SeatId == seatId;
        }

        public bool CanExpire(DateTime now)
        {
            return string.Equals(Status, ReservationStatuses.Pending, StringComparison.OrdinalIgnoreCase)
                && ExpiresAt <= now;
        }

        public void Expire(DateTime timestamp)
        {
            Status = ReservationStatuses.Expired;
            ExpiresAt = timestamp;
        }

        public static Reservation CreatePending(int userId, Guid seatId, DateTime reservedAt, DateTime expiresAt)
        {
            return new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeatId = seatId,
                Status = ReservationStatuses.Pending,
                ReservedAt = reservedAt,
                ExpiresAt = expiresAt
            };
        }

        public void Pay()
        {
            if (!string.Equals(Status, ReservationStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Solo una reserva pendiente puede marcarse como pagada.");
            }

            Status = ReservationStatuses.Paid;
        }

        public bool IsExpired(DateTime now) => now > ExpiresAt;

    }
}
