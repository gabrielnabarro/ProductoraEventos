using Domain.Constants;
using Domain.Entities;

namespace Infrastructure.Persistence;

public static class AppDbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        var hasEvents = context.EVENT.Any();
        var hasSectors = context.SECTOR.Any();
        var hasSeats = context.SEAT.Any();
        var hasUsers = context.USER.Any();

        if (hasEvents && hasSectors && hasSeats && hasUsers)
        {
            return;
        }

        if (hasEvents || hasSectors || hasSeats || hasUsers)
        {
            throw new InvalidOperationException("La base de datos contiene datos parciales de precarga. Limpiarla antes de inicializar nuevamente.");
        }

        using var transaction = context.Database.BeginTransaction();

        try
        {
            var eventEntity = new Event
            {
                Name = "ACDC en Argentina",
                EventDate = DateTime.UtcNow.AddMonths(2),
                Venue = "Estadio River Plate",
                Status = EventStatuses.Active,
                Sectors =
                [
                    new Sector
                    {
                        Name = "VIP",
                        Price = 150000.00m,
                        Capacity = 50,
                        Seats = Enumerable.Range(1, 50)
                            .Select(seatNumber => new Seat
                            {
                                Id = Guid.NewGuid(),
                                RowIdentifier = "V",
                                SeatNumber = seatNumber,
                                Status = seatNumber == 1 ? SeatStatuses.Sold : SeatStatuses.Available,
                                Version = 1
                            })
                            .ToList()
                    },
                    new Sector
                    {
                        Name = "General",
                        Price = 90000.00m,
                        Capacity = 50,
                        Seats = Enumerable.Range(1, 50)
                            .Select(seatNumber => new Seat
                            {
                                Id = Guid.NewGuid(),
                                RowIdentifier = "G",
                                SeatNumber = seatNumber,
                                Status = SeatStatuses.Available,
                                Version = 1
                            })
                            .ToList()
                    }
                ]
            };

            var demoUser = new User
            {
                Name = "Usuario Demo",
                Email = "demo@productoraeventos.local",
                PasswordHash = "seed-demo-user"
            };

            context.EVENT.Add(eventEntity);
            context.USER.Add(demoUser);
            context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
