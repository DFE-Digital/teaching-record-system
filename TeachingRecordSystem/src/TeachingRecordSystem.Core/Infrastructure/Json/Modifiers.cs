using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Optional;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Infrastructure.Json;

public static class Modifiers
{
    /// <summary>
    /// Only serialize Option{T} properties if they have a value.
    /// </summary>
    public static void OptionProperties(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        foreach (var property in typeInfo.Properties)
        {
            var isOptionType = property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(Option<>);

            if (isOptionType)
            {
                var underlyingType = property.PropertyType.GenericTypeArguments[0];

                property.ShouldSerialize = CreateShouldSerializePredicate(property.PropertyType);
                property.CustomConverter = (JsonConverter)Activator.CreateInstance(typeof(OptionJsonConverter<>).MakeGenericType(underlyingType))!;
                property.IsRequired = false;
            }
        }

        static Func<object, object?, bool> CreateShouldSerializePredicate(Type propertyType)
        {
            var parentParameter = Expression.Parameter(typeof(object));
            var propertyParameter = Expression.Parameter(typeof(object));

            return (Func<object, object?, bool>)Expression.Lambda(
                typeof(Func<object, object?, bool>),
                body: Expression.Property(
                    Expression.Convert(propertyParameter, propertyType),
                    "HasValue"),
                parentParameter,
                propertyParameter).Compile();
        }
    }

    public static void SupportTaskData(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(ISupportTaskData))
        {
            return;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$support-task-type",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
        };

        foreach (var supportTaskType in SupportTaskTypeRegistry.All)
        {
            typeInfo.PolymorphismOptions.DerivedTypes.Add(
                new JsonDerivedType(supportTaskType.DataType, (int)supportTaskType.SupportTaskType));
        }
    }

    public static void Events(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(IEvent))
        {
            return;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$event-name",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
        };

        var eventTypes = typeof(IEvent).Assembly.GetTypes()
            .Where(t => typeof(IEvent).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        foreach (var eventType in eventTypes)
        {
            var eventName = eventType.Name;

            typeInfo.PolymorphismOptions.DerivedTypes.Add(
                new JsonDerivedType(eventType, eventName));
        }
    }
}
