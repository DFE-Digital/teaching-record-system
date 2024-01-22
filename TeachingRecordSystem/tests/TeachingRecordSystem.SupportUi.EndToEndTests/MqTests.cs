using Microsoft.Playwright;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

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
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5), "212", new DateTime(2021, 10, 5)));
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

        await page.AssertOnAddMqStatusPage();

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
        var changeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider;
        var changeReasonDetail = "My change reason detail";
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

        await page.AssertOnEditMqProviderReasonPage(qualificationId);

        await page.CheckAsync($"label:text-is('{changeReason.GetDisplayName()}')");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestCommon.TestData.JpegImage
            });

        await page.ClickContinueButton();

        await page.AssertOnEditMqProviderConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }

    [Fact]
    public async Task EditMqSpecialism()
    {
        var oldSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newSpecialism = MandatoryQualificationSpecialism.Visual;
        var changeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism;
        var changeReasonDetail = "My change reason detail";
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"specialism-change-link-{qualificationId}");

        await page.AssertOnEditMqSpecialismPage(qualificationId);

        await page.IsCheckedAsync($"label:text-is('{oldSpecialism.GetTitle()}')");

        await page.CheckAsync($"label:text-is('{newSpecialism.GetTitle()}')");

        await page.ClickContinueButton();

        await page.AssertOnEditMqSpecialismReasonPage(qualificationId);

        await page.CheckAsync($"label:text-is('{changeReason.GetDisplayName()}')");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestCommon.TestData.JpegImage
            });

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
        var changeReason = MqChangeStartDateReasonOption.ChangeOfStartDate;
        var changeReasonDetail = "My change reason detail";
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
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

        await page.AssertOnEditMqStartDateReasonPage(qualificationId);

        await page.CheckAsync($"label:text-is('{changeReason.GetDisplayName()}')");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestCommon.TestData.JpegImage
            });

        await page.ClickContinueButton();

        await page.AssertOnEditMqStartDateConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task EditMqStatus(
        bool isStatusChange,
        bool isEndDateChange)
    {
        MandatoryQualificationStatus oldStatus;
        MandatoryQualificationStatus newStatus;
        DateOnly? oldEndDate;
        DateOnly? newEndDate;
        string changeReason = "";

        if (isStatusChange)
        {
            if (isEndDateChange)
            {
                oldStatus = MandatoryQualificationStatus.Failed;
                newStatus = MandatoryQualificationStatus.Passed;
                oldEndDate = null;
                newEndDate = new DateOnly(2021, 12, 5);
                changeReason = MqChangeStatusReasonOption.ChangeOfStatus.GetDisplayName()!;
            }
            else
            {
                oldStatus = MandatoryQualificationStatus.InProgress;
                newStatus = MandatoryQualificationStatus.Failed;
                oldEndDate = null;
                newEndDate = null;
                changeReason = MqChangeStatusReasonOption.ChangeOfStatus.GetDisplayName()!;
            }
        }
        else
        {
            oldStatus = MandatoryQualificationStatus.Passed;
            newStatus = MandatoryQualificationStatus.Passed;
            oldEndDate = new DateOnly(2021, 12, 5);
            newEndDate = new DateOnly(2021, 12, 6);
            changeReason = MqChangeEndDateReasonOption.ChangeOfEndDate.GetDisplayName()!;
        }

        var changeReasonDetail = "My change reason detail";
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus).WithEndDate(oldEndDate)));
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"status-change-link-{qualificationId}");

        await page.AssertOnEditMqStatusPage(qualificationId);

        if (isStatusChange)
        {
            await page.IsCheckedAsync($"label:text-is('{oldStatus.GetTitle()}')");

            await page.CheckAsync($"label:text-is('{newStatus.GetTitle()}')");
        }

        if (isEndDateChange)
        {
            await page.FillDateInput(newEndDate!.Value);
        }

        await page.ClickContinueButton();

        await page.AssertOnEditMqStatusReasonPage(qualificationId);

        await page.CheckAsync($"label:text-is('{changeReason}')");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
           "label:text-is('Upload a file')",
           new FilePayload()
           {
               Name = "evidence.jpg",
               MimeType = "image/jpeg",
               Buffer = TestCommon.TestData.JpegImage
           });

        await page.ClickContinueButton();

        await page.AssertOnEditMqStatusConfirmPage(qualificationId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification changed");
    }

    [Fact]
    public async Task DeleteMq()
    {
        var deletionReason = MqDeletionReasonOption.ProviderRequest;
        var deletionReasonDetail = "My deletion reason detail";
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPage(person.PersonId);

        await page.AssertOnPersonQualificationsPage(person.PersonId);

        await page.ClickLinkForElementWithTestId($"delete-link-{qualificationId}");

        await page.AssertOnDeleteMqPage(qualificationId);

        await page.CheckAsync($"label:text-is('{deletionReason.GetDisplayName()}')");

        await page.FillAsync("label:text-is('More detail about the reason for deleting')", deletionReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestCommon.TestData.JpegImage
            });

        await page.ClickContinueButton();

        await page.AssertOnDeleteMqConfirmPage(qualificationId);

        await page.ClickButton("Delete qualification");

        await page.AssertOnPersonQualificationsPage(personId);

        await page.AssertFlashMessage("Mandatory qualification deleted");
    }
}
