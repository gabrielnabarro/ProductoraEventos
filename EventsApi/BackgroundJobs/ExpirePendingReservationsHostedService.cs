using Application.Interfaces;
using Application.UseCases.Reservations.Commands.ExpirePendingReservations;
using Microsoft.Extensions.Options;

namespace EventsApi.BackgroundJobs;

public sealed class ExpirePendingReservationsHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<ReservationExpirationJobOptions> _options;
    private readonly ILogger<ExpirePendingReservationsHostedService> _logger;

    public ExpirePendingReservationsHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ReservationExpirationJobOptions> options,
        ILogger<ExpirePendingReservationsHostedService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.Enabled)
        {
            _logger.LogInformation("El job de expiracion de reservas se encuentra deshabilitado.");
            return;
        }

        var pollIntervalSeconds = _options.Value.PollIntervalSeconds > 0
            ? _options.Value.PollIntervalSeconds
            : ReservationExpirationJobOptions.DefaultPollIntervalSeconds;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(pollIntervalSeconds));

        await ExecuteCycleSafelyAsync(stoppingToken);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteCycleSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ExecuteCycleSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteCycleAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ocurrio un error al ejecutar el job de expiracion de reservas.");
        }
    }

    private async Task ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        var batchSize = _options.Value.BatchSize > 0
            ? _options.Value.BatchSize
            : ReservationExpirationJobOptions.DefaultBatchSize;

        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<IExpirePendingReservationsCommandHandler>();
        var result = await handler.Handle(
            new ExpirePendingReservationsCommand
            {
                TimestampUtc = DateTime.UtcNow,
                BatchSize = batchSize
            },
            cancellationToken);

        if (result.ExpiredReservationsCount > 0)
        {
            _logger.LogInformation(
                "Se expiraron {ExpiredReservationsCount} reservas pendientes en esta ejecucion.",
                result.ExpiredReservationsCount);
        }
    }
}
