namespace Domain.Constants;

public static class AuditLogActions
{
    //Reservas
    public const string ReserveAttempt = "RESERVE_ATTEMPT";
    public const string ReserveExpired = "RESERVE_EXPIRED";
    public const string ReserveRejected = "RESERVE_REJECTED";
    public const string ReserveSuccess = "RESERVE_SUCCESS";
   
    //Pagos
    public const string PaymentAttempt = "PAYMENT_ATTEMPT";
    public const string PaymentSuccess = "PAYMENT_SUCCESS";
    public const string PaymentFailed = "PAYMENT_FAILED";
}
