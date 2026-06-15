using DevHabit.Api.DTOs.Common;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Reflection;

namespace DevHabit.Api.Services;

public sealed class DataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        var fieldsSet = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        //遍历属性信息消耗的资源较多
        var propertyInfos = PropertiesCache.GetOrAdd(typeof(T), e => e.GetProperties(BindingFlags.Instance | BindingFlags.Public));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
            .Where(e => fieldsSet.Contains(e.Name))
            .ToArray();
        }

        IDictionary<string, object?> shapedObject = new ExpandoObject();

        foreach (var propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }

        return (ExpandoObject)shapedObject;
    }

    //不可用where遍历执行止面的代码，因为每次都要遍历属性信息
    public ReadOnlyCollection<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> entities, string? fields, Func<T, LinkDto[]>? linksFactory = null)
    {
        var fieldsSet = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        var propertyInfos = PropertiesCache.GetOrAdd(typeof(T), e => e.GetProperties(BindingFlags.Instance | BindingFlags.Public));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
            .Where(e => fieldsSet.Contains(e.Name))
            .ToArray();
        }

        var shapedObjects = new List<ExpandoObject>();

        foreach (var entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }

            if (linksFactory != null)
                shapedObject["links"] = linksFactory(entity);

            shapedObjects.Add((ExpandoObject)shapedObject);
        }
        return shapedObjects.AsReadOnly();
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return true;
        var fieldsSet = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var propertyInfos = PropertiesCache.GetOrAdd(typeof(T), e => e.GetProperties(BindingFlags.Instance | BindingFlags.Public));

        return fieldsSet.All(e => propertyInfos.Any(p => p.Name.Equals(e, StringComparison.OrdinalIgnoreCase)));
    }
}
