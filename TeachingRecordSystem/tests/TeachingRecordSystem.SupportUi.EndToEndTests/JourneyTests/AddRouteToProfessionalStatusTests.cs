namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class AddRouteToProfessionalStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Route_BackLink_QualificationPage()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync(true))
            .RandomOne();
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Route_QualificationPage_Continue_StatusPage()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync(true))
            .Single(r => r.Name == "Overseas Trained Teacher Recognition");
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", setRoute.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
    }
}
