using Microsoft.Playwright;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class RequestTrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("yes")]
    [InlineData("no")]
    public async Task RequestTrn_AlwaysAsksForNameAndProvider(string hasRegisteredForNpq)
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var accessToken = HostFixture.Configuration["RequestTrnAccessToken"];
        await page.GotoAsync($"/request-trn?AccessToken={accessToken}");
        await page.ClickButtonAsync("Start now");
        await page.WaitForUrlPathAsync("/request-trn/taking-npq");

        await page.CheckAsync($"text=yes");
        await page.ClickButtonAsync("Continue");

        //npq check
        await page.WaitForUrlPathAsync("/request-trn/npq-check");
        await page.CheckAsync($"text={hasRegisteredForNpq}");
        await page.ClickButtonAsync("Continue");

        //npq name
        await page.WaitForUrlPathAsync("/request-trn/npq-name");
        await page.FillAsync("input[name=NpqName]", "SomeNPQName");
        await page.ClickButtonAsync("Continue");

        //npq provider
        await page.WaitForUrlPathAsync("/request-trn/npq-provider");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync("/request-trn/npq-name");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync("/request-trn/npq-check");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync("/request-trn/taking-npq");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequestTrn_HasNationalInsuranceNumber_YesNo_FollowsExpectedNextPages(bool hasNationalInsuranceNumber)
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var accessToken = HostFixture.Configuration["RequestTrnAccessToken"];
        await page.GotoAsync($"/request-trn?AccessToken={accessToken}");
        await page.ClickButtonAsync("Start now");
        await page.WaitForUrlPathAsync("/request-trn/taking-npq");


        await page.CheckAsync($"text=yes");
        await page.ClickButtonAsync("Continue");

        //npq check
        await page.WaitForUrlPathAsync("/request-trn/npq-check");
        await page.CheckAsync("text=yes");
        await page.ClickButtonAsync("Continue");

        //npq name
        await page.WaitForUrlPathAsync("/request-trn/npq-name");
        await page.FillAsync("input[name=NpqName]", "SomeNPQName");
        await page.ClickButtonAsync("Continue");

        //npq provider
        await page.WaitForUrlPathAsync("/request-trn/npq-provider");
        await page.FillAsync("input[name=NpqTrainingProvider]", "SOME PROVIDER");
        await page.ClickButtonAsync("Continue");

        //working in school or educational setting
        await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
        await page.CheckAsync("text=No");
        await page.ClickButtonAsync("Continue");

        //personal email
        await page.WaitForUrlPathAsync("/request-trn/personal-email");
        await page.FillAsync("input[name=PersonalEmail]", Faker.Internet.Email());
        await page.ClickButtonAsync("Continue");

        //name
        await page.WaitForUrlPathAsync("/request-trn/name");
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        await page.FillAsync("input[name=FirstName]", firstName);
        await page.FillAsync("input[name=MiddleName]", middleName);
        await page.FillAsync("input[name=LastName]", lastName);
        await page.ClickButtonAsync("Continue");

        //previous name
        await page.WaitForUrlPathAsync("/request-trn/previous-name");
        await page.CheckAsync("text=No");
        await page.ClickButtonAsync("Continue");

        //dob
        await page.WaitForUrlPathAsync("/request-trn/date-of-birth");
        var dateOfBirth = new DateOnly(1980, 10, 12);
        await page.FillDateInputAsync(dateOfBirth);
        await page.ClickButtonAsync("Continue");

        //identity
        await page.WaitForUrlPathAsync("/request-trn/identity");
        await page
                .Locator("input[type='file']")
                .SetInputFilesAsync(
                    new FilePayload()
                    {
                        Name = "evidence.jpg",
                        MimeType = "image/jpeg",
                        Buffer = TestData.JpegImage
                    });
        await page.ClickButtonAsync("Continue");

        //NI
        await page.WaitForUrlPathAsync("/request-trn/national-insurance-number");

        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        if (hasNationalInsuranceNumber)
        {
            await page.CheckAsync("text=Yes");
            await page.FillAsync("input[name=NationalInsuranceNumber]", nationalInsuranceNumber);
            await page.ClickButtonAsync("Continue");

            await page.WaitForUrlPathAsync("/request-trn/check-answers");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/national-insurance-number");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/identity");
        }
        else
        {
            await page.CheckAsync("text=No");
            await page.ClickButtonAsync("Continue");
            await page.WaitForUrlPathAsync("/request-trn/address");

            var addressLine1 = Faker.Address.StreetAddress();
            var addressLine2 = Faker.Address.SecondaryAddress();
            var townOrCity = Faker.Address.City();
            var postalCode = Faker.Address.ZipCode();
            var country = TestData.GenerateCountry();

            await page.FillAsync("input[name=AddressLine1]", addressLine1);
            await page.FillAsync("input[name=AddressLine2]", addressLine2);
            await page.FillAsync("input[name=TownOrCity]", townOrCity);
            await page.FillAsync("input[name=PostalCode]", postalCode);
            await page.FillAsync("input[name=Country]", country);
            await page.ClickButtonAsync("Continue");

            await page.WaitForUrlPathAsync("/request-trn/check-answers");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/address");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/national-insurance-number");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/identity");
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequestTrn_WorkingInEducationalSetting_YesNo_FollowsExpectedNextPages(bool isWorkingInSchoolOrEducationalSetting)
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var accessToken = HostFixture.Configuration["RequestTrnAccessToken"];
        await page.GotoAsync($"/request-trn?AccessToken={accessToken}");
        await page.ClickButtonAsync("Start now");
        await page.WaitForUrlPathAsync("/request-trn/taking-npq");


        await page.CheckAsync($"text=yes");
        await page.ClickButtonAsync("Continue");

        //npq check
        await page.WaitForUrlPathAsync("/request-trn/npq-check");
        await page.CheckAsync("text=yes");
        await page.ClickButtonAsync("Continue");

        //npq name
        await page.WaitForUrlPathAsync("/request-trn/npq-name");
        await page.FillAsync("input[name=NpqName]", "SomeNPQName");
        await page.ClickButtonAsync("Continue");

        //npq provider
        await page.WaitForUrlPathAsync("/request-trn/npq-provider");
        await page.FillAsync("input[name=NpqTrainingProvider]", "SOME PROVIDER");
        await page.ClickButtonAsync("Continue");

        //working in school or educational setting
        if (isWorkingInSchoolOrEducationalSetting)
        {
            await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
            await page.CheckAsync("text=Yes");
            await page.ClickButtonAsync("Continue");

            //work email
            await page.WaitForUrlPathAsync("/request-trn/work-email");
            await page.ClickButtonAsync("Continue");

            // work email validation
            await page.WaitForUrlPathAsync("/request-trn/work-email");
            await page.FillAsync("input[name=WorkEmail]", Faker.Internet.Email());
            await page.ClickButtonAsync("Continue");

            //personal email
            await page.WaitForUrlPathAsync("/request-trn/personal-email");
            await page.FillAsync("input[name=PersonalEmail]", Faker.Internet.Email());
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/work-email");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/npq-provider");
        }
        else
        {
            await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
            await page.CheckAsync("text=No");
            await page.ClickButtonAsync("Continue");

            //personal email
            await page.WaitForUrlPathAsync("/request-trn/personal-email");
            await page.FillAsync("input[name=PersonalEmail]", Faker.Internet.Email());
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
            await page.ClickBackLinkAsync();

            await page.WaitForUrlPathAsync("/request-trn/npq-provider");
        }
    }
}
