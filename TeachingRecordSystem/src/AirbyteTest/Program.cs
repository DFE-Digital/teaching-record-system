using Dfe.Analytics.EFCore;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;

string connectionString = args[0];
string airbyteConnectionId = "980292d1-d6f6-4aa5-84c6-67de746fa478";
string apiBaseUrl = "https://airbyte-tra-development.test.teacherservices.cloud/";
string clientId = args[1];
string clientSecret = args[2];

var sp = new ServiceCollection()
    .AddDatabase(connectionString)
    .AddDfeAnalyticsDeploymentTools()
    .ConfigureAirbyteOptions(options =>
    {
        options.ApiBaseUrl = apiBaseUrl;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
    })
    .BuildServiceProvider();

var deployer = sp.GetRequiredService<AnalyticsDeployer>();

using var dbContext = sp.GetRequiredService<TrsDbContext>();

await deployer.DeployAsync(dbContext, airbyteConnectionId);
