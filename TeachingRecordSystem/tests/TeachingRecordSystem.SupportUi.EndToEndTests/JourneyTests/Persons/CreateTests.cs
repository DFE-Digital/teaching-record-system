using System.Text.RegularExpressions;
using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class CreateTests : TestBase
{
    public CreateTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Test]
    public async Task Create_Success()
    {
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonCreatePageAsync();

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", CreateReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickButtonAsync("Confirm and create record");

        await page.WaitForURLAsync(new Regex(@"/persons/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));
        await page.AssertFlashMessageAsync(expectedMessage: "Record created successfully for Alfred The Great.");
    }

    [Test]
    public async Task Create_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonCreatePageAsync();

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", CreateReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.AssertNameInputAsync("Alfred", "The", "Great");
    }

    [Test]
    public async Task Create_CYA_ChangeDetailsOrReason_ContinuesToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonCreatePageAsync();

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", CreateReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Megan", "Thee", "Stallion");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-create-reason-link");

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
    }

    [Test]
    public async Task Create_CYA_ChangeDetailsOrReason_NavigatesBackToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonCreatePageAsync();

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.SelectChangeReasonAsync("create-reason-options", CreateReasonOption.MandatoryQualification);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.FillNameInputsAsync("Megan", "Thee", "Stallion");
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickLinkForElementWithTestIdAsync("change-create-reason-link");

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonCreateCheckAnswersPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonCreateCreateReasonPageAsync();
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonCreatePersonalDetailsPageAsync();
        await page.AssertNameInputAsync("Alfred", "The", "Great");
    }
}
