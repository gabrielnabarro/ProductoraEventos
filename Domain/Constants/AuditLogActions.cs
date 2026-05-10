namespace Domain.Constants;

public static class AuditLogActions
{
    public const string ReserveAttempt = "RESERVE_ATTEMPT";
    public const string ReserveExpired = "RESERVE_EXPIRED";
    public const string ReserveRejected = "RESERVE_REJECTED";
    public const string ReserveSuccess = "RESERVE_SUCCESS";
}
