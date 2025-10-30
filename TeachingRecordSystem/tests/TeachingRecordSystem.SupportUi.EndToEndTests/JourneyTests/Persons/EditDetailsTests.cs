using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class EditDetailsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickButtonAsync("Confirm changes");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync(expectedHeader: "Personal details have been updated");
    }

    [Fact]
    public async Task EditDetails_ChangeOtherDetails()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickButtonAsync("Confirm changes");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync(expectedHeader: "Personal details have been updated");
    }

    [Fact]
    public async Task EditDetails_ChangeNameAndOtherDetails()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickButtonAsync("Confirm changes");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync(expectedHeader: "Personal details have been updated");
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

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.AssertNameInputAsync("Alfred", "The", "Great");
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_ChangeOtherDetails_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.AssertDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_ChangeNameAndOtherDetails_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.AssertNameInputAsync("Alfred", "The", "Great");
        await page.AssertDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_CYA_ChangeNameOrReason_WhenNameAndOtherDetailsPreviouslyChanged_ContinuesToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("name-change-reason-link");

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("other-details-change-reason-link");

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_CYA_ChangeNameOrReason_WhenNameAndOtherDetailsPreviouslyChanged_NavigatesBackToCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("name-change-reason-link");

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("other-details-change-reason-link");

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_CYA_ChangeName_WhenNameNotPreviouslyChanged_ContinuesToNameChangeReasonAndThenCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("name-change-reason-link");

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditDetails_CYA_ChangeOtherDetails_WhenOtherDetailsNotPreviouslyChanged_ContinuesToOtherDetailsChangeReasonAndThenCYA()
    {
        var person = await TestData.CreatePersonAsync();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillNameInputsAsync("Alfred", "The", "Great");
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsNameChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsNameChangeReasonOption.CorrectingAnError);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("change-details-link");

        await page.AssertOnPersonEditDetailsPageAsync(person.PersonId);
        await page.FillDateInputAsync(DateOnly.Parse("1 Nov 1990"));
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync("change-reason-options", EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync("other-details-change-reason-link");

        await page.AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDetailsCheckAnswersPageAsync(person.PersonId);
    }
}
