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
        var endDate = startDate.AddMonths(1);
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var setProviderName = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync(true))
            .RandomOne()
            .Name;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;

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
        await page.FillDateInputAsync(endDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.FillAsync($"label:text-is('Enter the training provider for this route')", setProviderName);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.FillAsync($"label:text-is('Enter the degree type awarded as part of this route')", setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.FillAsync($"label:text-is('Enter the country associated with their route')", setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCountryAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.ClickBackLink();

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
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var setProviderName = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync(true))
            .RandomOne()
            .Name;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;
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
        await page.FillAsync($"label:text-is('Enter the training provider for this route')", setProviderName);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.FillAsync($"label:text-is('Enter the degree type awarded as part of this route')", setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.FillAsync($"label:text-is('Enter the country associated with their route')", setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
    }

    [Fact]
    public async Task Route_Add_AwardedJourney()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync(true))
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory
                && r.InductionExemptionReason is not null
                && r.InductionExemptionReason.RouteImplicitExemption == false
                && r.TrainingProviderRequired == FieldRequirement.NotApplicable
                && r.DegreeTypeRequired == FieldRequirement.NotApplicable
                && r.TrainingCountryRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatus.Awarded;
        var awardedDate = new DateOnly(2021, 1, 1);
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;

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

        await page.AssertOnRouteAddAwardedDatePageAsync();
        await page.FillDateInputAsync(awardedDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddInductionExemptionPageAsync();
        await page.SetCheckedAsync($"label:text-is('Yes')", true);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.FillAsync($"label:text-is('Enter the country associated with their route')", setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickButtonAsync("Continue");

    }
}
