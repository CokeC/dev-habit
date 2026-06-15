using System.Collections.ObjectModel;

namespace DevHabit.Api.DTOs.Common;

public interface ICollectionResponse<T>
{
    ReadOnlyCollection<T> Items { get; init; }
}
