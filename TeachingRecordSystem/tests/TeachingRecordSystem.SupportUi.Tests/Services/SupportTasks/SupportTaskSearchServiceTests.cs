namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public partial class SupportTaskSearchServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
}
