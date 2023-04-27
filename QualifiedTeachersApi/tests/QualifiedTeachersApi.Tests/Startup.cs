using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QualifiedTeachersApi.Tests.DataverseIntegration;
using QualifiedTeachersApi.Tests.Infrastructure;

namespace QualifiedTeachersApi.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var testConfiguration = new TestConfiguration();
        var dbHelper = new DbHelper(testConfiguration.Configuration.GetConnectionString("DefaultConnection") ??
            throw new Exception("Connection string DefaultConnection is missing."));
        var apiFixture = new ApiFixture(testConfiguration, dbHelper);

        apiFixture.Initialize().GetAwaiter().GetResult();

        services.AddSingleton(testConfiguration);
        services.AddSingleton(dbHelper);
        services.AddSingleton(apiFixture);
        services.AddSingleton<CrmClientFixture>();
        services.AddMemoryCache();
    }
}
