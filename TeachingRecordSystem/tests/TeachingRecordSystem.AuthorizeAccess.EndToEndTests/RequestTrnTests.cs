using Microsoft.Playwright;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class RequestTrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory(Skip = "CI FLAKY TEST")]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task RequestTrnWithApplicationId(bool isTakingNpq, bool hasNationalInsuranceNumber)
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var accessToken = HostFixture.Configuration["RequestTrnAccessToken"];
        await page.GotoAsync($"/request-trn?AccessToken={accessToken}");
        await page.ClickButtonAsync("Start now");
        await page.WaitForUrlPathAsync("/request-trn/taking-npq");

        //Is Taking NPQ
        if (isTakingNpq)
        {
            await page.CheckAsync("text=Yes");
            await page.ClickButtonAsync("Continue");

            //npq check
            await page.WaitForUrlPathAsync("/request-trn/npq-check");
            await page.CheckAsync("text=Yes");
            await page.ClickButtonAsync("Continue");

            //npq applicationid
            await page.WaitForUrlPathAsync("/request-trn/npq-application");
            await page.FillAsync("input[name=NpqApplicationId]", "SomeApplicationID");
            await page.ClickButtonAsync("Continue");

            //working in school or educational setting
            await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
            await page.CheckAsync("text=Yes");
            await page.ClickButtonAsync("Continue");

            //work email
            await page.WaitForUrlPathAsync("/request-trn/work-email");
            await page.FillAsync("input[name=WorkEmail]", Faker.Internet.Email());
            await page.ClickButtonAsync("Continue");

            //personal email
            await page.WaitForUrlPathAsync("/request-trn/personal-email");
            await page.FillAsync("input[name=PersonalEmail]", Faker.Internet.Email());
            await page.ClickButtonAsync("Continue");

            //name
            await page.WaitForUrlPathAsync("/request-trn/name");
            var name = TestData.GenerateName();
            var previousName = TestData.GenerateName();
            await page.FillAsync("input[name=Name]", name);
            await page.ClickButtonAsync("Continue");

            //previous name
            await page.WaitForUrlPathAsync("/request-trn/previous-name");
            await page.CheckAsync("text=Yes");
            await page.FillAsync("input[name=PreviousName]", previousName);
            await page.ClickButtonAsync("Continue");

            //dob
            await page.WaitForUrlPathAsync("/request-trn/date-of-birth");
            var dateOfBirth = new DateOnly(1980, 10, 12);
            await page.FillDateInputAsync(dateOfBirth);
            await page.ClickButtonAsync("Continue");

            //identity
            await page.WaitForUrlPathAsync("/request-trn/identity");
            await page
                    .GetByLabel("Upload file")
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
                var country = Faker.Address.Country();

                await page.FillAsync("input[name=AddressLine1]", addressLine1);
                await page.FillAsync("input[name=AddressLine2]", addressLine2);
                await page.FillAsync("input[name=TownOrCity]", townOrCity);
                await page.FillAsync("input[name=PostalCode]", postalCode);
                await page.FillAsync("input[name=Country]", country);
                await page.ClickButtonAsync("Continue");

                await page.WaitForUrlPathAsync("/request-trn/check-answers");
            }
        }
        else
        {
            await page.CheckAsync("text=No");
            await page.ClickButtonAsync("Continue");

            await page.WaitForUrlPathAsync("/request-trn/not-eligible");
        }
    }

    [Theory(Skip = "CI FLAKY TEST")]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public async Task RequestTrnWithNpqNameAndNpqProvider(bool hasNationalInsuranceNumber, bool isWorkingInSchoolOrEducationalSetting)
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        var accessToken = HostFixture.Configuration["RequestTrnAccessToken"];
        await page.GotoAsync($"/request-trn?AccessToken={accessToken}");
        await page.ClickButtonAsync("Start now");
        await page.WaitForUrlPathAsync("/request-trn/taking-npq");


        await page.CheckAsync("text=Yes");
        await page.ClickButtonAsync("Continue");

        //npq check
        await page.WaitForUrlPathAsync("/request-trn/npq-check");
        await page.CheckAsync("text=no");
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
            await page.FillAsync("input[name=WorkEmail]", Faker.Internet.Email());
            await page.ClickButtonAsync("Continue");
        }
        else
        {
            await page.WaitForUrlPathAsync("/request-trn/school-or-educational-setting");
            await page.CheckAsync("text=No");
            await page.ClickButtonAsync("Continue");
        }

        //personal email
        await page.WaitForUrlPathAsync("/request-trn/personal-email");
        await page.FillAsync("input[name=PersonalEmail]", Faker.Internet.Email());
        await page.ClickButtonAsync("Continue");

        //name
        await page.WaitForUrlPathAsync("/request-trn/name");
        var name = TestData.GenerateName();
        var previousName = TestData.GenerateName();
        await page.FillAsync("input[name=Name]", name);
        await page.ClickButtonAsync("Continue");

        //previous name
        await page.WaitForUrlPathAsync("/request-trn/previous-name");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("input[name=PreviousName]", previousName);
        await page.ClickButtonAsync("Continue");

        //dob
        await page.WaitForUrlPathAsync("/request-trn/date-of-birth");
        var dateOfBirth = new DateOnly(1980, 10, 12);
        await page.FillDateInputAsync(dateOfBirth);
        await page.ClickButtonAsync("Continue");

        //identity
        await page.WaitForUrlPathAsync("/request-trn/identity");
        await page
                .GetByLabel("Upload file")
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
            var country = Faker.Address.Country();

            await page.FillAsync("input[name=AddressLine1]", addressLine1);
            await page.FillAsync("input[name=AddressLine2]", addressLine2);
            await page.FillAsync("input[name=TownOrCity]", townOrCity);
            await page.FillAsync("input[name=PostalCode]", postalCode);
            await page.FillAsync("input[name=Country]", country);
            await page.ClickButtonAsync("Continue");

            await page.WaitForUrlPathAsync("/request-trn/check-answers");
        }
    }
}
