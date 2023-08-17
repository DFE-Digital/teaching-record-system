namespace TeachingRecordSystem.Api.V3.Responses;

public record GetCertificateResponse
{
    public required string FileDownloadName { get; init; }
    public required Stream FileContents { get; init; }
}
