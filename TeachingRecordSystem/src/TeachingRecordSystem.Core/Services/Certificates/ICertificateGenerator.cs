namespace TeachingRecordSystem.Core.Services.Certificates;

public interface ICertificateGenerator
{
    Task<Stream> GenerateCertificateAsync(string templateName, IReadOnlyDictionary<string, string> fieldValues);
}
