using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.EndToEndTests.SupportUiJourneys;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task IndexReturnsOk()
    {
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync("/");
        Assert.Equal(StatusCodes.Status200OK, response?.Status);
    }
}
