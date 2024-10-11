using System.Collections;

namespace TeachingRecordSystem.UiTestCommon;

public class MultipartFormDataContentBuilder : IEnumerable<HttpContent>
{
    private readonly MultipartFormDataContent _content = [];

    public MultipartFormDataContentBuilder Add(string name, object value)
    {
        _content.Add(new StringContent(value?.ToString() ?? ""), name);
        return this;
    }

    public MultipartFormDataContentBuilder Add(string name, HttpContent content, string fileName)
    {
        _content.Add(content, name, fileName);
        return this;
    }

    public static implicit operator MultipartFormDataContent(MultipartFormDataContentBuilder builder) => builder.Build();

    public MultipartFormDataContent Build() => _content;

    IEnumerator<HttpContent> IEnumerable<HttpContent>.GetEnumerator() => _content.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _content.GetEnumerator();
}
