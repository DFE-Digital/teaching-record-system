namespace TeachingRecordSystem.Core;

public static class TrsUriHelper
{
    public static bool TryCreateWebsiteUri(string? uriString, out Uri? uri)
    {
        var isValidUri = Uri.TryCreate(uriString, UriKind.Absolute, out var uri2) ||
            Uri.TryCreate("http://" + uriString, UriKind.Absolute, out uri2) &&
            (uri2.Scheme == "http" || uri2.Scheme == "https");

        uri = uri2;
        return isValidUri;
    }
}
