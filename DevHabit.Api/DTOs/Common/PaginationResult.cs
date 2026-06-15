using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DevHabit.Api.DTOs.Common;

public sealed record PaginationResult<T> : ICollectionResponse<T>, ILinksResponse
{
    public required ReadOnlyCollection<T> Items { get; init; }
    public int Page {  get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public LinkDto[] Links { get; set; } = [];
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static async Task<PaginationResult<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        int totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new()
        {
            Items = items.AsReadOnly(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}