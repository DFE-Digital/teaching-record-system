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

    private IEnumerable<PostRequestEntry> BuildContentEntries()
    {
        var properties = GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetValue(this) != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            if (value is DateOnly date)
            {
                yield return new($"{property.Name}.Day", date.Day.ToString());
                yield return new($"{property.Name}.Month", date.Month.ToString());
                yield return new($"{property.Name}.Year", date.Year.ToString());
            }
            else if (value is Array array)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    yield return new($"{property.Name}[{i}]", array.GetValue(i)?.ToString());
                }
            }
            else if (value is (HttpContent content, string filename))
            {
                yield return new PostRequestFileEntry(property.Name, content, filename);
            }
            else
            {
                yield return new(property.Name, value?.ToString());
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
