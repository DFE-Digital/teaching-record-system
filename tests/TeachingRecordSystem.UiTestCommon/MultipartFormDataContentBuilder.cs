using System.Collections;

namespace TeachingRecordSystem.UiTestCommon;

public class MultipartFormDataContentBuilder : IEnumerable<HttpContent>
{
    private readonly MultipartFormDataContent _content = [];

    public MultipartFormDataContentBuilder Add(string name, object? value)
    {
        _content.Add(new StringContent(value?.ToString() ?? ""), name);
        return this;
    }

    public MultipartFormDataContentBuilder Add(string name, (HttpContent Content, string FileName)? evidenceFile)
    {
        if (evidenceFile is null)
        {
            _content.Add(new StringContent(""), name);
        }
        else
        {
            _content.Add(evidenceFile.Value.Content, name, evidenceFile.Value.FileName);
        }
        return this;
    }

    public static implicit operator MultipartFormDataContent(MultipartFormDataContentBuilder builder) => builder.Build();

    public MultipartFormDataContent Build() => _content;

    IEnumerator<HttpContent> IEnumerable<HttpContent>.GetEnumerator() => _content.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _content.GetEnumerator();
}
