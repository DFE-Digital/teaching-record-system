namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

internal static class StreamHelper
{
    public static async Task<string> GetBase64EncodedFileContent(Stream file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var buffer = ms.ToArray();
        return Convert.ToBase64String(buffer);
    }
}
