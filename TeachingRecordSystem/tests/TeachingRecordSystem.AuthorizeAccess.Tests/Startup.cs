using Npgsql;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices((context, services) =>
            {
                var pgConnectionString = new NpgsqlConnectionStringBuilder(context.Configuration.GetRequiredConnectionString("DefaultConnection"))
                {
                    // We rely on error details to get the offending duplicate key values in the TrsDataSyncHelper
                    IncludeErrorDetail = true
                }.ConnectionString;

                DbHelper.ConfigureDbServices(services, pgConnectionString);

                services.AddSingleton<HostFixture>();
                services.AddSingleton<DbFixture>();
                services.AddSingleton<FakeTrnGenerator>();
                services.AddCrmQueries();
                services.AddFakeXrm();
            });
}
