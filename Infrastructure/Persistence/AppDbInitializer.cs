using Domain.Constants;
using Domain.Entities;

namespace Infrastructure.Persistence;

public static class AppDbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        var hasEvents = context.Event.Any();
        var hasSectors = context.Sector.Any();
        var hasSeats = context.Seat.Any();
        var hasUsers = context.User.Any();

        if (hasEvents && hasSectors && hasSeats && hasUsers)
        {
            return;
        }

        if (hasEvents || hasSectors || hasSeats || hasUsers)
        {
            throw new InvalidOperationException("Database contains partial seed data. Clean the database before initializing again.");
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

            context.Event.Add(eventEntity);
            context.User.Add(demoUser);
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
