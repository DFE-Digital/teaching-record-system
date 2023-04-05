#nullable disable
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QualifiedTeachersApi.Services.Certificates;

public interface ICertificateGenerator
{
    Task<Stream> GenerateCertificate(string templateName, IReadOnlyDictionary<string, string> fieldValues);
}
