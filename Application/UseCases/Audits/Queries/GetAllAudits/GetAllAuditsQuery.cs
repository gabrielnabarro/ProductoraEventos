namespace Application.UseCases.Audits.Queries.GetAllAudits;

public sealed class GetAllAuditsQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
