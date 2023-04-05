#nullable disable
namespace QualifiedTeachersApi.V3.Responses;

public record GetCertificateResponse
{
    public required string FileDownloadName { get; init; }
    public required byte[] FileContents { get; init; }
}
