namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Test]
    public async Task IndexReturnsOk()
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var response = await page.GotoAsync("/");
        await Assert.That(response?.Status).IsEqualTo(StatusCodes.Status200OK);
    }
}
