namespace QualifiedTeachersApi.V3.Responses;

public record GetCertificateResponse
{
    public string FileDownloadName { get; init; }
    public byte[] FileContents { get; init; }
}
