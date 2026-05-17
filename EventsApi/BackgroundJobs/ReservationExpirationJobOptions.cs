namespace EventsApi.BackgroundJobs;

public sealed class ReservationExpirationJobOptions
{
    public const string SectionName = "ReservationExpirationJob";
    public const int DefaultPollIntervalSeconds = 30;
    public const int DefaultBatchSize = 100;

    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = DefaultPollIntervalSeconds;
    public int BatchSize { get; set; } = DefaultBatchSize;
}
