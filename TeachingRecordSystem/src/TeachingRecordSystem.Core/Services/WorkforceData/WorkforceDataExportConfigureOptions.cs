using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

internal class WorkforceDataExportConfigureOptions : IConfigureOptions<WorkforceDataExportOptions>
{
    private readonly IConfiguration _configuration;

    public WorkforceDataExportConfigureOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(WorkforceDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var section = _configuration.GetSection("WorkforceDataExport");

        options.BucketName = section["BucketName"]!;
        var credentialsJson = section["CredentialsJson"];

        if (!string.IsNullOrEmpty(credentialsJson))
        {
            var credentialsJsonDoc = JsonDocument.Parse(credentialsJson);

            if (credentialsJsonDoc.RootElement.TryGetProperty("private_key", out _))
            {
                var creds = GoogleCredential.FromJson(credentialsJson);
                options.StorageClient = StorageClient.Create(creds);
            }
        }
    }
}
