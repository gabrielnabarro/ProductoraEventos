using Domain.Constants;
using Domain.Entities;
using Application.Interfaces;

namespace Infrastructure.Persistence;

public static class AppDbInitializer
{
    private const string DemoUserEmail = "demo@productoraeventos.local";
    private const string DemoUserTwoEmail = "demo2@productoraeventos.local";
    private const int DefaultSeatsPerRow = 10;

    public static void Initialize(AppDbContext context, IPasswordHasher passwordHasher)
    {
        var hasEvents = context.EVENT.Any();
        var hasSectors = context.SECTOR.Any();
        var hasSeats = context.SEAT.Any();
        var hasUsers = context.USER.Any();

        if (hasEvents && hasSectors && hasSeats && hasUsers)
        {
            EnsureDemoUsers(context, passwordHasher);
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
                        Seats = BuildSeats(50, seatIndex => seatIndex == 1 ? SeatStatuses.Sold : SeatStatuses.Available)
                    },
                    new Sector
                    {
                        Name = "General",
                        Price = 90000.00m,
                        Capacity = 50,
                        Seats = BuildSeats(50)
                    }
                ]
            };

            context.EVENT.Add(eventEntity);
            context.USER.AddRange(BuildDemoUsers(passwordHasher));
            context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void EnsureDemoUsers(AppDbContext context, IPasswordHasher passwordHasher)
    {
        var hasChanges = false;

        foreach (var missingUser in BuildDemoUsers(passwordHasher)
                     .Where(seedUser => context.USER.All(existingUser => existingUser.Email != seedUser.Email)))
        {
            context.USER.Add(missingUser);
            hasChanges = true;
        }

        if (hasChanges)
        {
            context.SaveChanges();
        }
    }

    private static User[] BuildDemoUsers(IPasswordHasher passwordHasher)
    {
        return
        [
            new User
            {
                Name = "Usuario Demo",
                Email = DemoUserEmail,
                PasswordHash = passwordHasher.Hash("demo1234")
            },
            new User
            {
                Name = "Usuario Demo 2",
                Email = DemoUserTwoEmail,
                PasswordHash = passwordHasher.Hash("demo1234")
            }
        ];
    }

    private static List<Seat> BuildSeats(int totalSeats, Func<int, string>? statusResolver = null)
    {
        return Enumerable.Range(1, totalSeats)
            .Select(seatIndex => new Seat
            {
                Id = Guid.NewGuid(),
                RowIdentifier = BuildRowIdentifier(seatIndex),
                SeatNumber = ((seatIndex - 1) % DefaultSeatsPerRow) + 1,
                Status = statusResolver?.Invoke(seatIndex) ?? SeatStatuses.Available,
                Version = 1
            })
            .ToList();
    }

    private static string BuildRowIdentifier(int seatIndex)
    {
        var rowIndex = (seatIndex - 1) / DefaultSeatsPerRow;

        if (rowIndex >= 26)
        {
            throw new InvalidOperationException("La precarga no admite mas de 26 filas por sector.");
        }

        return ((char)('A' + rowIndex)).ToString();
    }
}
