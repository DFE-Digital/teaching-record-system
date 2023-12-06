using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class MqTests : TestBase
{
    public MqTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task AddMq()
    {
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetMqSpecialismByValue("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        var result = dfeta_qualification_dfeta_MQ_Status.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(personId);

        await page.AssertOnPersonQualificationsPage(personId);

        await page.ClickButton("Add a mandatory qualification");

        await page.AssertOnAddMqProviderPage();

        await page.FillAsync($"label:text-is('Training provider')", mqEstablishment.dfeta_name);

        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButton();

        await page.AssertOnAddMqSpecialismPage();

        await page.CheckAsync($"label:text-is('{specialism.dfeta_name}')");

        await page.ClickContinueButton();

        await page.AssertOnAddMqStartDatePage();

        await page.FillDateInput(startDate);

        await page.ClickContinueButton();

        await page.AssertOnAddMqResultPage();

        await page.CheckAsync($"label:text-is('{result}')");

        await page.FillDateInput(endDate);

        await page.ClickContinueButton();

        await page.AssertOnAddMqCheckAnswersPage();

        await page.ClickButton("Confirm mandatory qualification");

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification added");
    }

    [Fact]
    public async Task EditMqProvider()
    {
        var oldMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var newMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("961"); // University of Manchester        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"provider-change-link-{qualificationId}");

        await page.AssertOnEditMqProviderPage(qualificationId);

        await page.FillAsync($"label:text-is('Training provider')", newMqEstablishment.dfeta_name);

        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButton();

        await page.AssertOnEditMqProviderConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }

    [Fact]
    public async Task EditMqSpecialism()
    {
        var oldSpecialism = await TestData.ReferenceDataCache.GetMqSpecialismByValue("Hearing");
        var newSpecialism = await TestData.ReferenceDataCache.GetMqSpecialismByValue("Visual");
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"specialism-change-link-{qualificationId}");

        await page.AssertOnEditMqSpecialismPage(qualificationId);

        await page.IsCheckedAsync($"label:text-is('{oldSpecialism.dfeta_name}')");

        await page.CheckAsync($"label:text-is('{newSpecialism.dfeta_name}')");

        await page.ClickContinueButton();

        await page.AssertOnEditMqSpecialismConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }

    [Fact]
    public async Task EditMqStartDate()
    {
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(startDate: oldStartDate));
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"start-date-change-link-{qualificationId}");

        await page.AssertOnEditMqStartDatePage(qualificationId);

        await page.FillDateInput(newStartDate);

        await page.ClickContinueButton();

        await page.AssertOnEditMqStartDateConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }

    [Fact]
    public async Task EditMqResult()
    {
        var oldResult = dfeta_qualification_dfeta_MQ_Status.Failed;
        var newResult = dfeta_qualification_dfeta_MQ_Status.Passed;
        var newEndDate = new DateOnly(2021, 11, 5);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(result: oldResult));
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"result-change-link-{qualificationId}");

        await page.AssertOnEditMqResultPage(qualificationId);

        await page.IsCheckedAsync($"label:text-is('{oldResult}')");

        await page.CheckAsync($"label:text-is('{newResult}')");

        await page.FillDateInput(newEndDate);

        await page.ClickContinueButton();

        await page.AssertOnEditMqResultConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }
}
