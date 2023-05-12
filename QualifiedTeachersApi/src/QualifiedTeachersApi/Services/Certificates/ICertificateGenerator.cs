﻿namespace QualifiedTeachersApi.Services.Certificates;

public interface ICertificateGenerator
{
    Task<Stream> GenerateCertificate(string templateName, IReadOnlyDictionary<string, string> fieldValues);
}
