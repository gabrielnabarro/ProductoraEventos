using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public int SectorId { get; set; }
        public string RowIdentifier { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string Status { get; set; } = "Available"; // Available, Reserved, Sold
        public int Version { get; set; } // Para control de concurrencia

        public Sector? Sector { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}