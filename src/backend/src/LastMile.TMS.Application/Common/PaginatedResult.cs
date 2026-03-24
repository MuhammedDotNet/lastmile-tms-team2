namespace LastMile.TMS.Application.Common;

/// <summary>
/// Standard page of results for list endpoints (REST and GraphQL).
/// </summary>
public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
