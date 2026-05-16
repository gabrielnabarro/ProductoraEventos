using Domain.Constants;
using System.ComponentModel.DataAnnotations;

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
                throw new InvalidOperationException("La butaca no esta disponible para reservar.");
            }

            Status = SeatStatuses.Reserved;
        }

        public void Release()
        {
            if (string.Equals(Status, SeatStatuses.Sold, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La butaca vendida no se puede liberar.");
            }

            Status = SeatStatuses.Available;
        }

        public void Sell()
        {
            if (!string.Equals(Status, SeatStatuses.Reserved, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Solo una butaca reservada puede marcarse como vendida.");
            }

            Status = SeatStatuses.Sold;
        }
    }
}
