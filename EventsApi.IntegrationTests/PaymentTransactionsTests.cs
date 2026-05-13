using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EventsApi.IntegrationTests;

public sealed class PaymentTransactionsTests : IAsyncLifetime
{
    private readonly string _databaseName = $"ProductoraEventosTests_{Guid.NewGuid():N}";

    private TestApiFactory _testApiFactory = null!;
    private HttpClient _apiClient = null!;
    private Guid _targetSeatId;
    private int _testUserId;

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
        var user = await context.USER
            .OrderBy(currentUser => currentUser.Id)
            .FirstAsync();

        _targetSeatId = seat.Id;
        _testUserId = user.Id;
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
    public async Task ConfirmPayment_MarksSeatSoldReservationPaidAndWritesSuccessAudit()
    {
        var reservationResponse = await _apiClient.PostAsJsonAsync("/api/v1/reservations", new
        {
            seatId = _targetSeatId,
            userId = _testUserId
        });

        Assert.Equal(HttpStatusCode.Created, reservationResponse.StatusCode);

        var reservationBody = await reservationResponse.Content.ReadFromJsonAsync<JsonElement>();
        var reservationId = reservationBody.GetProperty("reservationId").GetGuid();

        var paymentResponse = await _apiClient.PostAsJsonAsync("/api/v1/payments", new
        {
            reservationId,
            userId = _testUserId
        });

        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);

        await using var scope = _testApiFactory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seat = await context.SEAT
            .AsNoTracking()
            .SingleAsync(currentSeat => currentSeat.Id == _targetSeatId);
        var reservation = await context.RESERVATION
            .AsNoTracking()
            .SingleAsync(currentReservation => currentReservation.Id == reservationId);
        var paymentAuditLogs = await context.AUDIT_LOG
            .AsNoTracking()
            .Where(auditLog =>
                auditLog.EntityId == reservationId.ToString() &&
                (auditLog.Action == AuditLogActions.PaymentAttempt ||
                 auditLog.Action == AuditLogActions.PaymentSuccess ||
                 auditLog.Action == AuditLogActions.PaymentFailed))
            .ToListAsync();

        Assert.Equal(SeatStatuses.Sold, seat.Status);
        Assert.Equal(ReservationStatuses.Paid, reservation.Status);
        Assert.Single(paymentAuditLogs.Where(auditLog => auditLog.Action == AuditLogActions.PaymentAttempt));
        Assert.Single(paymentAuditLogs.Where(auditLog => auditLog.Action == AuditLogActions.PaymentSuccess));
        Assert.Empty(paymentAuditLogs.Where(auditLog => auditLog.Action == AuditLogActions.PaymentFailed));
    }

    [Fact]
    public async Task RollbackTransaction_ClearsTrackedChangesBeforeFailureAuditSave()
    {
        var auditLogId = Guid.NewGuid();

        await using (var scope = _testApiFactory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var seat = await context.SEAT.SingleAsync(currentSeat => currentSeat.Id == _targetSeatId);

            await unitOfWork.BeginTransactionAsync();
            seat.Reserve();
            await unitOfWork.RollbackTransactionAsync();

            await context.AUDIT_LOG.AddAsync(new AuditLog
            {
                Id = auditLogId,
                UserId = _testUserId,
                Action = AuditLogActions.PaymentFailed,
                EntityType = AuditLogEntityTypes.Reservation,
                EntityId = Guid.NewGuid().ToString(),
                Details = "Rollback test failure audit.",
                CreatedAt = DateTime.UtcNow
            });
            await unitOfWork.SaveChangesAsync();
        }

        await using var assertScope = _testApiFactory.Services.CreateAsyncScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var persistedSeat = await assertContext.SEAT
            .AsNoTracking()
            .SingleAsync(currentSeat => currentSeat.Id == _targetSeatId);
        var failureAuditExists = await assertContext.AUDIT_LOG
            .AsNoTracking()
            .AnyAsync(auditLog => auditLog.Id == auditLogId);

        Assert.Equal(SeatStatuses.Available, persistedSeat.Status);
        Assert.True(failureAuditExists);
    }
}
