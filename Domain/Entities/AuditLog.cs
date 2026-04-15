using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } 
        public int? UserId { get; set; } // Puede ser nulo si es un proceso de sistema
        public string Action { get; set; } = string.Empty; // Ej: RESERVE_ATTEMPT, RESERVE_SUCCESS, EXPIRED
        public string EntityType { get; set; } = string.Empty; // Ej: Reservation, Seat
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty; // JSON con metadatos
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}