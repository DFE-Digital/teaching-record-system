namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class RequestTrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task RequestTrn()
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/request-trn");
        await page.ClickButton("Start now");
        await page.WaitForUrlPathAsync("/request-trn/email");

        var email = Faker.Internet.Email();
        await page.FillAsync("input[name=Email]", email);
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/request-trn/name");
    }
}
