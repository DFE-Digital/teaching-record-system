namespace TeachingRecordSystem.SupportUi.Tests.Services.OneLogins;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public partial class OneLoginSearchServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
}
