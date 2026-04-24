namespace Application.UseCases.Events.Queries.GetAllEvents;

public sealed class GetAllEventsQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
