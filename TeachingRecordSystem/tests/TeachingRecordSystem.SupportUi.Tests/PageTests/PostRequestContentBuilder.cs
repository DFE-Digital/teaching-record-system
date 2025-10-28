using System.Reflection;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests;

public abstract class PostRequestContentBuilder
{
    public FormUrlEncodedContent BuildFormUrlEncoded()
    {
        var entries = BuildContentEntries().Select(e => e.AsKeyValuePair());

        return new FormUrlEncodedContent(entries);
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
        var properties = parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(parent);

            if (value is null)
            {
                yield return new($"{prefix}{property.Name}", "");

                continue;
            }

            if (value is DateOnly date)
            {
                yield return new($"{prefix}{property.Name}.Day", date.Day.ToString());
                yield return new($"{prefix}{property.Name}.Month", date.Month.ToString());
                yield return new($"{prefix}{property.Name}.Year", date.Year.ToString());

                continue;
            }

            if (value is Array array)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    yield return new($"{prefix}{property.Name}[{i}]", array.GetValue(i)?.ToString());
                }

                continue;
            }

            if (value is (HttpContent content, string filename))
            {
                yield return new PostRequestFileEntry($"{prefix}{property.Name}", (content, filename));

                continue;
            }

            if (value is string str)
            {
                yield return new($"{prefix}{property.Name}", value?.ToString());

                continue;
            }

            if (value.GetType() is Type t && !t.IsValueType && !t.IsPrimitive && !t.IsEnum)
            {
                foreach (var entry in BuildContentEntries($"{prefix}{property.Name}.", value))
                {
                    yield return entry;
                }

                continue;
            }

            yield return new($"{prefix}{property.Name}", value?.ToString());
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

    private record PostRequestFileEntry(string Key, (HttpContent Content, string Filename) File) : PostRequestEntry(Key, File.Filename)
    {
        public override void Write(MultipartFormDataContentBuilder builder)
        {
            builder.Add(Key, File);
        }
    }
}
