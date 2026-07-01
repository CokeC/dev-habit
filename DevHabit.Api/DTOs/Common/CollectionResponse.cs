using System.Collections.ObjectModel;

namespace DevHabit.Api.DTOs.Common;

public sealed class CollectionResponse<T> : ICollectionResponse<T>, ILinksResponse
{
    public required ReadOnlyCollection<T> Items { get; init; }
    public LinkDto[] Links { get; set; } = [];
}
