namespace TeachingRecordSystem.Api.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<ApiFixture>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var dbHelper = new DbHelper(configuration.GetRequiredConnectionString("DefaultConnection"));
        var apiFixture = new ApiFixture(configuration, dbHelper);
        var dbFixture = new DbFixture(configuration, dbHelper);

        apiFixture.Initialize().GetAwaiter().GetResult();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(dbHelper);
        services.AddSingleton(apiFixture);
        services.AddSingleton(dbFixture);
        services.AddMemoryCache();
    }
}
