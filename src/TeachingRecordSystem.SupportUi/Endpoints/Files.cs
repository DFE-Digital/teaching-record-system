using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Endpoints;

public static class Files
{
    public static IEndpointConventionBuilder MapFiles(this IEndpointRouteBuilder endpoints)
    {
        // This endpoint is used to proxy files from external URLs (e.g. Azure Blob Storage SAS URLs) to allow us to use the filename with extension in the URL
        // which helps the browser choose an appropriate app to open the file with (in particular for Outlook .msg files).
        return endpoints.MapGet("/files/{fileName}", [Authorize] async (HttpClient httpClient, [FromRoute] string fileName, [FromQuery] string fileUrl) =>
        {
            var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            var stream = await response.Content.ReadAsStreamAsync();
            return Results.Stream(stream, contentType: response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream");
        }).WithName("Files");
    }
}
