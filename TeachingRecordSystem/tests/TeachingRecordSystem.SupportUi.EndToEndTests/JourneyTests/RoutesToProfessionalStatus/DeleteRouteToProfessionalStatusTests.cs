using TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.RoutesToProfessionalStatus;

public class DeleteRouteToProfessionalStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task DeleteRouteToProfessionalStatus()
    {
        var deletionReason = ChangeReasonOption.CreatedInError;
        var person = await TestData.CreatePersonAsync(p => p.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.QualifiedTeacherStatus));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);

        await page.ClickLinkForElementWithTestIdAsync($"delete-route-link-{qualificationId}");

        await page.AssertOnRouteDeleteChangeReasonPage(qualificationId);
        await page.SelectRouteChangeReasonOption(deletionReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDeleteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Delete route");

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }
}
