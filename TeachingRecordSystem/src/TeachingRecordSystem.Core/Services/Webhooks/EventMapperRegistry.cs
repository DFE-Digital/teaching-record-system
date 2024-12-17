using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.ApiSchema.V3;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class EventMapperRegistry
{
    private sealed record MapperKey(Type EventType, string CloudEventType, string ApiVersion);

    private sealed record MapperValue(Type MapperType, Type DataType);

    private readonly Dictionary<MapperKey, MapperValue> _mappers = DiscoverMappers();

    public Type? GetMapperType(Type eventType, string cloudEventType, string apiVersion, [MaybeNull] out Type dataType)
    {
        if (_mappers.TryGetValue(new(eventType, cloudEventType, apiVersion), out var result))
        {
            dataType = result.DataType;
            return result.MapperType;
        }

        dataType = null;
        return null;
    }

    private static Dictionary<MapperKey, MapperValue> DiscoverMappers()
    {
        var mapperTypes = typeof(EventMapperRegistry).Assembly.GetTypes()
            .Where(t => t.IsPublic && !t.IsAbstract && t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition().IsAssignableTo(typeof(IEventMapper<,>))));

        var mappers = new Dictionary<MapperKey, MapperValue>();

        foreach (var type in mapperTypes)
        {
            var mapperTypeArgs = type.GetInterface(typeof(IEventMapper<,>).Name)!.GetGenericArguments();
            var eventType = mapperTypeArgs[0];
            var dataType = mapperTypeArgs[1];

            var cloudEventType = (string)dataType.GetProperty("CloudEventType")!.GetValue(null)!;

            var version = type.Namespace!.Split('.').SkipWhile(ns => ns != "V3").Skip(1).First().TrimStart('V');

            mappers.Add(new MapperKey(eventType, cloudEventType, version), new MapperValue(type, dataType));
        }

        return mappers;
    }
}
