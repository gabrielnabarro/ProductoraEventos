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
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty; // Ej: "RESERVE_ATTEMPT" 
        public string EntityType { get; set; } = string.Empty; // Ej: "Reservation"
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty; // JSON
        public DateTime CreatedAt { get; set; }
    }
}
