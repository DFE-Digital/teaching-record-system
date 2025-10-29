using System.Text.RegularExpressions;
using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class AddPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task AddPerson_Success()
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAddPersonPageAsync();

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", AddPersonReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickButtonAsync("Confirm and create record");

        await page.WaitForURLAsync(new Regex(@"/persons/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));
        await page.AssertFlashMessageAsync(expectedHeader: "Record created for Alfred The Great");
    }

    [Fact]
    public async Task AddPerson_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAddPersonPageAsync();

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", AddPersonReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnAddPersonReasonPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.AssertNameInputAsync("Alfred", "The", "Great");
    }

    [Fact]
    public async Task AddPerson_CYA_ChangeDetailsOrReason_ContinuesToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAddPersonPageAsync();

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", AddPersonReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Megan", "Thee", "Stallion");
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-create-reason-link");

        await page.AssertOnAddPersonReasonPageAsync();
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
    }

    [Fact]
    public async Task AddPerson_CYA_ChangeDetailsOrReason_NavigatesBackToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAddPersonPageAsync();

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", AddPersonReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Megan", "Thee", "Stallion");
        await page.ClickBackLinkAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-create-reason-link");

        await page.AssertOnAddPersonReasonPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnAddPersonCheckAnswersPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnAddPersonReasonPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnAddPersonPersonalDetailsPageAsync();
        await page.AssertNameInputAsync("Alfred", "The", "Great");
    }
}
