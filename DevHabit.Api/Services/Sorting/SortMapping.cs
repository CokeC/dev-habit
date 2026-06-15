using OpenTelemetry.Resources;
using System.Linq.Dynamic.Core;

namespace DevHabit.Api.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);

public interface ISortMappingDefinition;

public sealed class SortMappingDefinition : ISortMappingDefinition
{
    public required SortMapping[] Mappings { get; init; }
}

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition? sortMappingDefinition = sortMappingDefinitions.OfType<SortMappingDefinition>().FirstOrDefault();

        if (sortMappingDefinition == null)
            throw new InvalidOperationException($"从'{typeof(TSource).Name}'到'{typeof(TDestination).Name}'的定义未找到！");

        return sortMappingDefinition.Mappings;
    }

    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return true;

        var sortFields = sort.Split(',')
            .Select(x => x.Trim().Split(' ')[0])
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();

        var mapping = GetMappings<TSource, TDestination>();

        return sortFields.All(e => mapping.Any(m => m.SortField.Equals(e, StringComparison.OrdinalIgnoreCase)));
    }
}

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sort, SortMapping[] mappings, string defaultOrderBy = "Id")
    {
        if (string.IsNullOrEmpty(sort))
            return query.OrderBy(defaultOrderBy);

        var sortFields = sort.Split(',')
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToArray();

        var orderByParts = new List<string>();
        foreach(var field in sortFields)
        {
            (var sortField, var isDescending) = ParseSortField(field);
            var mapping = mappings.First(e => e.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));

            string direction = (isDescending, mapping.Reverse) switch
            {
                (false, false) => "ASC",
                (false, true) => "DESC",
                (true, false) => "DESC",
                (true, true) => "ASC"
            };

            orderByParts.Add($"{mapping.PropertyName} {direction}");
        }

        var orderBy = string.Join(",", orderByParts);
        return query.OrderBy(orderBy);
    }

    private static (string SortField, bool IsDescending) ParseSortField(string field)
    {
        var parts = field.Split('.');
        var sortField = parts[0];
        var isDescending = parts.Length > 1 &&
            parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (sortField, isDescending);
    }
}