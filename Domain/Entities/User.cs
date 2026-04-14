using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Un usuario puede realizar múltiples reservas
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Un usuario puede estar asociado a múltiples registros de auditoría
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}