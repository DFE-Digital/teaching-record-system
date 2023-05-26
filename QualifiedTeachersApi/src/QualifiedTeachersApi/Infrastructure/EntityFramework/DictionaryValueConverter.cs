using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace QualifiedTeachersApi.Infrastructure.EntityFramework;

public class DictionaryValueConverter : ValueConverter<Dictionary<string, string>, string>
{
    public DictionaryValueConverter()
        : base(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?) null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?) null)!)
    {
    }
}
