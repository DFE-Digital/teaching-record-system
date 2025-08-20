using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class SetStatusTests : TestBase
{
    public SetStatusTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Deactivate()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Deactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.SelectChangeReasonAsync("deactivate-reason-options", DeactivateReasonOption.ProblemWithTheRecord);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickButtonAsync("Confirm and deactivate record");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync(expectedMessage: "Ethelred The Unready’s record has been deactivated.");
    }

    [Fact]
    public async Task Reactivate()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Reactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.SelectChangeReasonAsync("reactivate-reason-options", ReactivateReasonOption.DeactivatedByMistake);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickButtonAsync("Confirm and reactivate record");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync(expectedMessage: "Ethelred The Unready’s record has been reactivated.");
    }

    [Fact]
    public async Task Deactivate_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Deactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.SelectChangeReasonAsync("deactivate-reason-options", DeactivateReasonOption.ProblemWithTheRecord);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickBackLink();

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickBackLink();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Reactivate_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Reactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.SelectChangeReasonAsync("reactivate-reason-options", ReactivateReasonOption.DeactivatedByMistake);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickBackLink();

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickBackLink();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Deactivate_CYA_ChangeReason_ContinuesToCYA()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Deactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.SelectChangeReasonAsync("deactivate-reason-options", DeactivateReasonOption.ProblemWithTheRecord);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickLinkForElementWithTestIdAsync("change-deactivate-reason-link");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Deactivated);
    }

    [Fact]
    public async Task Reactivate_CYA_ChangeReason_ContinuesToCYA()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Reactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.SelectChangeReasonAsync("reactivate-reason-options", ReactivateReasonOption.DeactivatedByMistake);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickLinkForElementWithTestIdAsync("change-reactivate-reason-link");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Active);
    }

    [Fact]
    public async Task Deactivate_CYA_NavigatesBackToCYA()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Deactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.SelectChangeReasonAsync("deactivate-reason-options", DeactivateReasonOption.ProblemWithTheRecord);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickLinkForElementWithTestIdAsync("change-deactivate-reason-link");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickBackLink();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickBackLink();

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Deactivated);
        await page.ClickBackLink();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Reactivate_CYA_NavigatesBackToCYA()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Ethelred")
            .WithMiddleName("The")
            .WithLastName("Unready"));

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Reactivate record");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.SelectChangeReasonAsync("reactivate-reason-options", ReactivateReasonOption.DeactivatedByMistake);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickLinkForElementWithTestIdAsync("change-reactivate-reason-link");

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickBackLink();

        await page.AssertOnPersonSetStatusCheckAnswersPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickBackLink();

        await page.AssertOnPersonSetStatusChangeReasonPageAsync(person.PersonId, PersonStatus.Active);
        await page.ClickBackLink();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }
}
