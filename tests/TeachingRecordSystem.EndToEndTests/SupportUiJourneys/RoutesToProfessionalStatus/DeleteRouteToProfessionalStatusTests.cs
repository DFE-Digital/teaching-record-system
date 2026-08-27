using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.EndToEndTests.SupportUiJourneys.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.EndToEndTests.SupportUiJourneys.RoutesToProfessionalStatus;

public class DeleteRouteToProfessionalStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task DeleteRouteToProfessionalStatus()
    {
        var deletionReason = ChangeReasonOption.CreatedInError;
        var person = await TestData.CreatePersonAsync(p => p.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.QualifiedTeacherStatus));
        var personId = person.PersonId;
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single().QualificationId;

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"delete-route-link-{qualificationId}");

        await page.AssertOnRouteDeleteChangeReasonPage(qualificationId);
        await page.SelectRouteChangeReasonOption(deletionReason.ToString());
        await page.SelectProvideAdditionalInformationAsync("provide-more-information-options", ProvideMoreInformationOption.No);
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDeleteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickGovUkButtonAsync("Confirm and delete route");

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }

    [Fact]
    public async Task DeleteRouteToProfessionalStatusProvideAdditionalDetails()
    {
        var deletionReason = ChangeReasonOption.AnotherReason;
        var person = await TestData.CreatePersonAsync(p => p.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.QualifiedTeacherStatus));
        var personId = person.PersonId;
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single().QualificationId;

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"delete-route-link-{qualificationId}");

        await page.AssertOnRouteDeleteChangeReasonPage(qualificationId);
        await page.SelectRouteChangeReasonOption(deletionReason.ToString(), "some reason");
        await page.SelectProvideAdditionalInformationAsync("provide-more-information-options", ProvideMoreInformationOption.Yes, "Some additional information");
        await page.SelectUploadEvidenceAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDeleteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickGovUkButtonAsync("Confirm and delete route");

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }
}
