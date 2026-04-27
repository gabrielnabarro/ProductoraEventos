namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class ReservationMessageFactory
{
    public string CreateSuccessAuditMessage(ReservationSelectionResult selectionResult)
    {
        if (selectionResult.ReusedExistingReservation)
        {
            return selectionResult.ReleasedPreviousReservations
                ? "La butaca ya estaba reservada por el usuario y se liberaron las reservas anteriores del evento."
                : "La butaca ya estaba reservada por el usuario.";
        }

        return selectionResult.ReleasedPreviousReservations
            ? "La butaca fue reservada correctamente y se liberaron las reservas anteriores del evento."
            : "La butaca fue reservada correctamente.";
    }

    public string CreateSuccessResponseMessage(ReservationSelectionResult selectionResult)
    {
        if (selectionResult.ReusedExistingReservation)
        {
            return selectionResult.ReleasedPreviousReservations
                ? "La reserva ya existia y se liberaron las butacas anteriores."
                : "La butaca ya estaba reservada por el usuario.";
        }

        return selectionResult.ReleasedPreviousReservations
            ? "Reserva actualizada correctamente y se liberaron las butacas anteriores."
            : "Reserva creada correctamente.";
    }
}
