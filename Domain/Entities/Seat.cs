using Domain.Constants;

namespace Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public int SectorId { get; set; }
        public string RowIdentifier { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string Status { get; set; } = SeatStatuses.Available;
        public int Version { get; set; }

        public Sector? Sector { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public bool IsAvailable()
        {
            return string.Equals(Status, SeatStatuses.Available, StringComparison.OrdinalIgnoreCase);
        }

        public void Reserve()
        {
            if (!IsAvailable())
            {
                throw new InvalidOperationException("Seat is not available for reservation.");
            }

            Status = SeatStatuses.Reserved;
        }
    }
}
