using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> EVENT => Set<Event>();
    public DbSet<Sector> SECTOR => Set<Sector>();
    public DbSet<Seat> SEAT => Set<Seat>();
    public DbSet<User> USER => Set<User>();
    public DbSet<Reservation> RESERVATION => Set<Reservation>();
    public DbSet<AuditLog> AUDIT_LOG => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("EVENT");
            entity.HasKey(eventEntity => eventEntity.Id);

            entity.Property(eventEntity => eventEntity.Id).HasColumnName("Id");
            entity.Property(eventEntity => eventEntity.Name).HasColumnName("Name").HasMaxLength(150);
            entity.Property(eventEntity => eventEntity.EventDate).HasColumnName("EventDate");
            entity.Property(eventEntity => eventEntity.Venue).HasColumnName("Venue").HasMaxLength(150);
            entity.Property(eventEntity => eventEntity.Status).HasColumnName("Status").HasMaxLength(20);

            entity.HasMany(eventEntity => eventEntity.Sectors)
                .WithOne(sector => sector.Event)
                .HasForeignKey(sector => sector.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("SECTOR");
            entity.HasKey(sector => sector.Id);

            entity.Property(sector => sector.Id).HasColumnName("Id");
            entity.Property(sector => sector.EventId).HasColumnName("EventId");
            entity.Property(sector => sector.Name).HasColumnName("Name").HasMaxLength(100);
            entity.Property(sector => sector.Price).HasColumnName("Price").HasPrecision(18, 2);
            entity.Property(sector => sector.Capacity).HasColumnName("Capacity");

            entity.HasMany(sector => sector.Seats)
                .WithOne(seat => seat.Sector)
                .HasForeignKey(seat => seat.SectorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Seat>()
            .ToTable("SEAT");

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(seat => seat.Id);

            entity.Property(seat => seat.Id).HasColumnName("Id");
            entity.Property(seat => seat.SectorId).HasColumnName("SectorId");
            entity.Property(seat => seat.RowIdentifier).HasColumnName("RowIdentifier").HasMaxLength(10);
            entity.Property(seat => seat.SeatNumber).HasColumnName("SeatNumber");
            entity.Property(seat => seat.Status).HasColumnName("Status").HasMaxLength(20);
            entity.Property(seat => seat.Version).HasColumnName("Version").IsConcurrencyToken();

            entity.HasMany(seat => seat.Reservations)
                .WithOne(reservation => reservation.Seat)
                .HasForeignKey(reservation => reservation.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USER");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();

            entity.Property(user => user.Id).HasColumnName("Id");
            entity.Property(user => user.Name).HasColumnName("Name").HasMaxLength(100);
            entity.Property(user => user.Email).HasColumnName("Email").HasMaxLength(150);
            entity.Property(user => user.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255);

            entity.HasMany(user => user.Reservations)
                .WithOne(reservation => reservation.User)
                .HasForeignKey(reservation => reservation.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(user => user.AuditLogs)
                .WithOne(auditLog => auditLog.User)
                .HasForeignKey(auditLog => auditLog.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("RESERVATION");
            entity.HasKey(reservation => reservation.Id);

            entity.Property(reservation => reservation.Id).HasColumnName("Id");
            entity.Property(reservation => reservation.UserId).HasColumnName("UserId");
            entity.Property(reservation => reservation.SeatId).HasColumnName("SeatId");
            entity.Property(reservation => reservation.Status).HasColumnName("Status").HasMaxLength(20);
            entity.Property(reservation => reservation.ReservedAt).HasColumnName("ReservedAt");
            entity.Property(reservation => reservation.ExpiresAt).HasColumnName("ExpiresAt");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AUDIT_LOG");
            entity.HasKey(auditLog => auditLog.Id);

            entity.Property(auditLog => auditLog.Id).HasColumnName("Id");
            entity.Property(auditLog => auditLog.UserId).HasColumnName("UserId").IsRequired(false);
            entity.Property(auditLog => auditLog.Action).HasColumnName("Action").HasMaxLength(50);
            entity.Property(auditLog => auditLog.EntityType).HasColumnName("EntityType").HasMaxLength(50);
            entity.Property(auditLog => auditLog.EntityId).HasColumnName("EntityId").HasMaxLength(100);
            entity.Property(auditLog => auditLog.Details).HasColumnName("Details");
            entity.Property(auditLog => auditLog.CreatedAt).HasColumnName("CreatedAt");
        });
    }
}
