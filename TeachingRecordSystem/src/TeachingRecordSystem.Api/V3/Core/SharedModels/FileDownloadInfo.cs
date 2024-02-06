using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record FileDownloadInfo(Stream Contents, string Name, string ContentType)
{
    public FileResult ToFileResult() => new FileStreamResult(Contents, ContentType)
    {
        FileDownloadName = Name
    };
}
