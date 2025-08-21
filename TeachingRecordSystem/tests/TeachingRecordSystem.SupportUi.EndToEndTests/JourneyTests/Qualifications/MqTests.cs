using Microsoft.Playwright;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Qualifications;

public class MqTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task AddMq()
    {
        var person = await TestData.CreatePersonAsync();
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValueAsync("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetMqSpecialismByValueAsync("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        var result = dfeta_qualification_dfeta_MQ_Status.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(personId);

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.ClickButtonAsync("Add a mandatory qualification");

        await page.AssertOnAddMqProviderPageAsync();

        await page.FillAsync($"label:text-is('Training provider')", mqEstablishment.dfeta_name);

        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnAddMqSpecialismPageAsync();

        await page.CheckAsync($"label{TextIsSelector(specialism.dfeta_name)}");

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddMqStartDatePageAsync();

        await page.FillDateInputAsync(startDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddMqStatusPageAsync();

        await page.CheckAsync($"label{TextIsSelector(result.ToString())}");

        await page.FillDateInputAsync(endDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddMqCheckAnswersPageAsync();

        await page.ClickButtonAsync("Confirm mandatory qualification");

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.AssertFlashMessageAsync("Mandatory qualification added");
    }

    [Fact]
    public async Task EditMqProvider()
    {
        var oldMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValueAsync("959"); // University of Leeds
        var newMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValueAsync("961"); // University of Manchester
        var changeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider;
        var changeReasonDetail = "My change reason detail";
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"provider-change-link-{qualificationId}");

        await page.AssertOnEditMqProviderPageAsync(qualificationId);

        await page.FillAsync($"label:text-is('Training provider')", newMqEstablishment.dfeta_name);

        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqProviderReasonPageAsync(qualificationId);

        await page.CheckAsync($"label{TextIsSelector(changeReason.GetDisplayName())}");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestData.JpegImage
            });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqProviderConfirmPageAsync(qualificationId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.AssertFlashMessageAsync("Mandatory qualification changed");
    }

    [Fact]
    public async Task EditMqSpecialism()
    {
        var oldSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newSpecialism = MandatoryQualificationSpecialism.Visual;
        var changeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism;
        var changeReasonDetail = "My change reason detail";
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"specialism-change-link-{qualificationId}");

        await page.AssertOnEditMqSpecialismPageAsync(qualificationId);

        await page.IsCheckedAsync($"label{TextIsSelector(oldSpecialism.GetTitle())}");

        await page.CheckAsync($"label{TextIsSelector(newSpecialism.GetTitle())}");

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqSpecialismReasonPageAsync(qualificationId);

        await page.CheckAsync($"label{TextIsSelector(changeReason.GetDisplayName())}");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestData.JpegImage
            });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqSpecialismConfirmPageAsync(qualificationId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.AssertFlashMessageAsync("Mandatory qualification changed");
    }

    [Fact]
    public async Task EditMqStartDate()
    {
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var changeReason = MqChangeStartDateReasonOption.ChangeOfStartDate;
        var changeReasonDetail = "My change reason detail";
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"start-date-change-link-{qualificationId}");

        await page.AssertOnEditMqStartDatePageAsync(qualificationId);

        await page.FillDateInputAsync(newStartDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqStartDateReasonPageAsync(qualificationId);

        await page.CheckAsync($"label{TextIsSelector(changeReason.GetDisplayName())}");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestData.JpegImage
            });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqStartDateConfirmPageAsync(qualificationId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.AssertFlashMessageAsync("Mandatory qualification changed");
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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"status-change-link-{qualificationId}");

        await page.AssertOnEditMqStatusPageAsync(qualificationId);

        if (isStatusChange)
        {
            await page.IsCheckedAsync($"label{TextIsSelector(oldStatus.GetTitle())}");

            await page.CheckAsync($"label{TextIsSelector(newStatus.GetTitle())}");
        }

        if (isEndDateChange)
        {
            await page.FillDateInputAsync(newEndDate!.Value);
        }

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqStatusReasonPageAsync(qualificationId);

        await page.CheckAsync($"label{TextIsSelector(changeReason)}");

        await page.FillAsync("label:text-is('More detail about the reason for change')", changeReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
           "label:text-is('Upload a file')",
           new FilePayload()
           {
               Name = "evidence.jpg",
               MimeType = "image/jpeg",
               Buffer = TestData.JpegImage
           });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditMqStatusConfirmPageAsync(qualificationId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.AssertFlashMessageAsync("Mandatory qualification changed");
    }

    [Fact]
    public async Task DeleteMq()
    {
        var deletionReason = MqDeletionReasonOption.ProviderRequest;
        var deletionReasonDetail = "My deletion reason detail";
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var personId = person.PersonId;
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"delete-link-{qualificationId}");

        await page.AssertOnDeleteMqPageAsync(qualificationId);

        await page.CheckAsync($"label{TextIsSelector(deletionReason.GetDisplayName())}");

        await page.FillAsync("label:text-is('More detail about the reason for deleting')", deletionReasonDetail);

        await page.CheckAsync($"label:text-is('Yes')");

        await page.SetInputFilesAsync(
            "label:text-is('Upload a file')",
            new FilePayload()
            {
                Name = "evidence.jpg",
                MimeType = "image/jpeg",
                Buffer = TestData.JpegImage
            });

        await page.ClickContinueButtonAsync();

        await page.AssertOnDeleteMqConfirmPageAsync(qualificationId);

        await page.ClickButtonAsync("Delete qualification");

        await page.AssertOnPersonQualificationsPageAsync(personId);

        await page.AssertFlashMessageAsync("Mandatory qualification deleted");
    }
}
