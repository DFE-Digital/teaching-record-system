using Microsoft.AspNetCore.Http;
using Microsoft.Playwright;

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

    [Fact]
    public async Task IndexShowsTeamsLinkWhenTeamManagementEnabled()
    {
        // Arrange
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync("/");

        // Assert
        var teamsLink = page.GetByRole(AriaRole.Link, new() { Name = "Add and manage teams" });
        Assert.True(await teamsLink.IsVisibleAsync());
    }
}
