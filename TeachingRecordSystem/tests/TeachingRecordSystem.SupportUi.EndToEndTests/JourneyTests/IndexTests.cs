namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task IndexReturnsOk()
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync("/");
        Assert.Equal(StatusCodes.Status200OK, response?.Status);
    }
}
