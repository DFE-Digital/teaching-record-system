namespace TeachingRecordSystem.SupportUi.Tests;

public class SharedDependenciesDataSourceAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    public override IServiceScope CreateScope(DataGeneratorMetadata dataGeneratorMetadata) =>
        Setup.Services.CreateScope();

    public override object? Create(IServiceScope scope, Type type) =>
        scope.ServiceProvider.GetService(type);
}
