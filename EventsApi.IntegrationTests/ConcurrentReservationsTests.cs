using Domain.Constants;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EventsApi.IntegrationTests;

public sealed class ConcurrentReservationsTests : IAsyncLifetime
{
    private const int ConcurrentRequestCount = 200;

    private readonly string _databaseName = $"ProductoraEventosTests_{Guid.NewGuid():N}";

    private TestApiFactory _testApiFactory = null!;
    private HttpClient _apiClient = null!;
    private Guid _targetSeatId;
    private int[] _testUserIds = [];

    private string ConnectionString => $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    public async Task InitializeAsync()
    {
        _testApiFactory = new TestApiFactory(ConnectionString);
        _apiClient = _testApiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        await using var scope = _testApiFactory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seat = await context.SEAT
            .Where(currentSeat => currentSeat.Status == SeatStatuses.Available)
            .OrderBy(currentSeat => currentSeat.RowIdentifier)
            .ThenBy(currentSeat => currentSeat.SeatNumber)
            .FirstAsync();

        _targetSeatId = seat.Id;
        _testUserIds = await CreateUsersAsync(context, ConcurrentRequestCount);
    }

    public async Task DisposeAsync()
    {
        if (_testApiFactory is not null)
        {
            await using var scope = _testApiFactory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureDeletedAsync();
        }

        _apiClient?.Dispose();
        _testApiFactory?.Dispose();
    }

    [Fact]
    public async Task CreateReservation_AllowsOnlyOneConcurrentWinnerForSameSeat()
    {
        var simultaneousStartSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var concurrentRequests = _testUserIds
            .Select(userId => SendReservationAsync(userId, simultaneousStartSignal.Task))
            .ToArray();

        simultaneousStartSignal.SetResult(true);

        var responses = await Task.WhenAll(concurrentRequests);

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(ConcurrentRequestCount - 1, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));

        await using var scope = _testApiFactory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seat = await context.SEAT.SingleAsync(currentSeat => currentSeat.Id == _targetSeatId);
        var activeReservations = await context.RESERVATION
            .Where(reservation => reservation.SeatId == _targetSeatId && reservation.Status == ReservationStatuses.Pending)
            .ToListAsync();
        var auditLogs = await context.AUDIT_LOG
            .Where(auditLog =>
                auditLog.EntityId == _targetSeatId.ToString() &&
                (auditLog.Action == AuditLogActions.ReserveAttempt ||
                 auditLog.Action == AuditLogActions.ReserveSuccess ||
                 auditLog.Action == AuditLogActions.ReserveRejected))
            .ToListAsync();

        Assert.Equal(SeatStatuses.Reserved, seat.Status);
        Assert.Equal(2, seat.Version);
        Assert.Single(activeReservations);
        Assert.Equal(ConcurrentRequestCount, auditLogs.Count(auditLog => auditLog.Action == AuditLogActions.ReserveAttempt));
        Assert.Single(auditLogs.Where(auditLog => auditLog.Action == AuditLogActions.ReserveSuccess));
        Assert.Equal(ConcurrentRequestCount - 1, auditLogs.Count(auditLog => auditLog.Action == AuditLogActions.ReserveRejected));
    }

    private Task<HttpResponseMessage> SendReservationAsync(int userId, Task startSignal)
    {
        return SendAsync();

        async Task<HttpResponseMessage> SendAsync()
        {
            await startSignal;

            return await _apiClient.PostAsJsonAsync("/api/v1/reservations", new
            {
                seatId = _targetSeatId,
                userId
            });
        }
    }

    private static async Task<int[]> CreateUsersAsync(AppDbContext context, int count)
    {
        var users = Enumerable.Range(1, count)
            .Select(index => new User
            {
                Name = $"Concurrency User {index}",
                Email = $"concurrency-{Guid.NewGuid():N}-{index}@productoraeventos.local",
                PasswordHash = "integration-test"
            })
            .ToArray();

        await context.USER.AddRangeAsync(users);
        await context.SaveChangesAsync();

        return users.Select(user => user.Id).ToArray();
    }
}
