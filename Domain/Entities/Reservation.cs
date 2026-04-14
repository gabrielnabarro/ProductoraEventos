using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; } 
        public int UserId { get; set; }
        public Guid SeatId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Expired
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        // Propiedades de navegación
        public User? User { get; set; }
        public Seat? Seat { get; set; }
    }
}
