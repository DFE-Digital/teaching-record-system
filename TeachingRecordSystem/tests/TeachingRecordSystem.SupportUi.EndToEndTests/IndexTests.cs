using TeachingRecordSystem.SupportUi.EndToEndTests.Shared;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task IndexReturnsOk()
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync("/");
        Assert.Equal(StatusCodes.Status200OK, response?.Status);
    }
}
