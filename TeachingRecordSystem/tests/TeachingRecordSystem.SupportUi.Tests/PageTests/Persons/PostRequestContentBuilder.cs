using System.Reflection;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons;

public abstract class PostRequestContentBuilder
{
    public FormUrlEncodedContent BuildFormUrlEncoded()
    {
        return new FormUrlEncodedContent(BuildContentEntries()
            .Select(e => e.AsKeyValuePair()));
    }

    public MultipartFormDataContent BuildMultipartFormData()
    {
        var builder = new MultipartFormDataContentBuilder();

        foreach (var entry in BuildContentEntries())
        {
            entry.Write(builder);
        }

        return builder.Build();
    }

    private IEnumerable<PostRequestEntry> BuildContentEntries(string? prefix = null, object? parent = null)
    {
        prefix ??= "";
        parent ??= this;
        var properties = parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.GetValue(this) != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(parent);
            if (value is DateOnly date)
            {
                yield return new($"{prefix}{property.Name}.Day", date.Day.ToString());
                yield return new($"{prefix}{property.Name}.Month", date.Month.ToString());
                yield return new($"{prefix}{property.Name}.Year", date.Year.ToString());
            }
            else if (value is Array array)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    yield return new($"{prefix}{property.Name}[{i}]", array.GetValue(i)?.ToString());
                }
            }
            else if (value is (HttpContent content, string filename))
            {
                yield return new PostRequestFileEntry($"{prefix}{property.Name}", content, filename);
            }
            else if (!value.GetType().IsValueType && !value.GetType().IsPrimitive && !value.GetType().IsEnum)
            {
                foreach (var entry in BuildContentEntries($"{prefix}{property.Name}.", value))
                {
                    yield return entry;
                }
            }
            else
            {
                yield return new($"{prefix}{property.Name}", value?.ToString());
            }
        }
    }

    private record PostRequestEntry(string Key, string? Value)
    {
        public KeyValuePair<string, string?> AsKeyValuePair() => new(Key, Value);

        public virtual void Write(MultipartFormDataContentBuilder builder)
        {
            builder.Add(Key, Value ?? "");
        }
    }

    private record PostRequestFileEntry(string Key, HttpContent Content, string Filename) : PostRequestEntry(Key, Filename)
    {
        public override void Write(MultipartFormDataContentBuilder builder)
        {
            builder.Add(Key, Content, Filename);
        }
    }
}
