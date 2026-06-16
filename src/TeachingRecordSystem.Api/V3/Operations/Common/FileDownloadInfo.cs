using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.Operations.Common;

public record FileDownloadInfo(Stream Contents, string Name, string ContentType)
{
    public FileResult ToFileResult() => new FileStreamResult(Contents, ContentType)
    {
        FileDownloadName = Name
    };
}
