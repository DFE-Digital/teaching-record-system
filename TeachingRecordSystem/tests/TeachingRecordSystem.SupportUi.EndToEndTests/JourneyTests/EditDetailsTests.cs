using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class EditDetailsTests : TestBase
{
    public EditDetailsTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditDetails_ChangeName()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickButtonAsync("Confirm changes");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync(expectedMessage: "Personal details have been updated successfully.");
    }

    [Fact]
    public async Task EditDetails_ChangeName_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.AssertNameInputAsync("Alfred", "The", "Great");
        await page.ClickBackLink();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_CYA_ChangeNameOrReason_ContinuesToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");
        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-reason-link");
        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_CYA_ChangeNameOrReason_NavigatesBackToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");
        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-reason-link");
        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonEditDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }
}
