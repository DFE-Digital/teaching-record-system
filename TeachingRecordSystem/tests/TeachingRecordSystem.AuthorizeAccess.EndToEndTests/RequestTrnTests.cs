using Microsoft.Playwright;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class RequestTrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequestTrn(bool hasNationalInsuranceNumber)
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

        var name = Faker.Name.FullName();
        var previousName = Faker.Name.FullName();
        await page.FillAsync("input[name=Name]", name);
        await page.CheckAsync("text=Yes");
        await page.FillAsync("input[name=PreviousName]", previousName);
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/request-trn/date-of-birth");

        var dateOfBirth = new DateOnly(1980, 10, 12);
        await page.FillDateInput(dateOfBirth);
        await page.ClickButton("Continue");

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
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/request-trn/national-insurance-number");

        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        if (hasNationalInsuranceNumber)
        {
            await page.CheckAsync("text=Yes");
            await page.FillAsync("input[name=NationalInsuranceNumber]", nationalInsuranceNumber);
            await page.ClickButton("Continue");

            await page.WaitForUrlPathAsync("/request-trn/check-answers");
        }
        else
        {
            await page.CheckAsync("text=No");
            await page.ClickButton("Continue");
            await page.WaitForUrlPathAsync("/request-trn/address");
        }
    }
}
