using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TeachingRecordSystem.Core.Infrastructure.EntityFramework;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> propertyBuilder) where TProperty : class
    {
        var converter = new ValueConverter<TProperty, string>(
            v => Serialize(v),
            v => Deserialize<TProperty>(v));

        var comparer = new ValueComparer<TProperty>(
            (p1, p2) => Serialize(p1) == Serialize(p2),
            v => v == null ? 0 : Serialize(v).GetHashCode(),
            v => Deserialize<TProperty>(Serialize(v)));

        propertyBuilder.HasConversion(converter, comparer);
        return propertyBuilder;
    }

    private static string Serialize<TProperty>(TProperty value)
        => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null);

    private static TProperty Deserialize<TProperty>(string value)
        => JsonSerializer.Deserialize<TProperty>(value, (JsonSerializerOptions?)null)!;
}
