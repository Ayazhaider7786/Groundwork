using backend.Services.Model.Common;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Common;

public static class PaginationExtensions
{
    public const int MaxPageSize = 200;

    /// <summary>
    /// Counts, clamps and pages a query in one place so no endpoint can ship an
    /// unbounded page size.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = safePageNumber,
            PageSize = safePageSize,
        };
    }
}
