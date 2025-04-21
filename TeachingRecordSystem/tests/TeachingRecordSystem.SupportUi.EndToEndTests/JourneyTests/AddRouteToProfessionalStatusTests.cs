namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class AddRouteToProfessionalStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Route_BackLink_QualificationPage()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync(true))
            .Single(r => r.Name == "Apprenticeship");
        var status = ProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", route.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStartDatePageAsync();
        await page.FillDateInputAsync(startDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddEndDatePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddStartDatePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddStatusPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddRoutePageAsync();
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Route_AddJourney()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync(true))
            .Single(r => r.Name == "Apprenticeship");
        var status = ProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddMonths(1);

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
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStartDatePageAsync();
        await page.FillDateInputAsync(startDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddEndDatePageAsync();
        await page.FillDateInputAsync(endDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddTrainingProviderAsync();
    }
}
