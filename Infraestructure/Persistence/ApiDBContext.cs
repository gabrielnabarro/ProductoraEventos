using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infraestructure.Persistence
{
    public class ApiDBContext: DbContext
    {
        public ApiDBContext(DbContextOptions<ApiDBContext> options) : base(options)
        {
        }

        public DbSet<Event> Event { get; set; }
        public DbSet<Sector> Sector { get; set; }
        public DbSet<Seat> Seat { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de Optimistic Locking
            modelBuilder.Entity<Seat>()
                .Property(s => s.Version)
                .IsConcurrencyToken(); // EF Core verificará este campo antes de hacer un UPDATE

            modelBuilder.Entity<Sector>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sector>()
                .HasOne(s => s.Event)
                .WithMany(e => e.Sectors)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Sector)
                .WithMany(sec => sec.Seats)
                .HasForeignKey(s => s.SectorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Seat)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
