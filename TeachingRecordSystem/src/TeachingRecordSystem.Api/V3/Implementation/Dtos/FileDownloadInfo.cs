using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record FileDownloadInfo(Stream Contents, string Name, string ContentType)
{
    public FileResult ToFileResult() => new FileStreamResult(Contents, ContentType)
    {
        FileDownloadName = Name
    };
}
