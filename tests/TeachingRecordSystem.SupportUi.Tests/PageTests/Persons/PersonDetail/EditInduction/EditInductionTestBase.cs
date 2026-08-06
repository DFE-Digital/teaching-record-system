using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public abstract class EditInductionTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    // Which of the induction page's Change links the journey was started from. The four questions it
    // links to are each a way into the journey, and the one used is step 0 of the path.
    public enum StartPage
    {
        Status = 0,
        ExemptionReasons,
        StartDate,
        CompletedDate
    }

    protected Task<EditInductionJourneyCoordinator> CreateJourneyInstanceAsync(
        Guid personId,
        EditInductionState? state = null,
        StartPage startPage = StartPage.Status)
    {
        state ??= new EditInductionState();

        return JourneyHelper.CreateInstanceAsync<EditInductionJourneyCoordinator>(
            JourneyNames.EditInduction,
            new RouteValueDictionary { ["personId"] = personId },
            _ => Task.FromResult<object>(state),
            pathUrls: GetPathUrls(personId, state.InductionStatus, startPage),
            coordinatorFactory: CreateJourneyCoordinator<EditInductionJourneyCoordinator>);
    }

    // The evidence answer, in the shape the reason question would have left it.
    protected static EvidenceUploadModel CreateEvidence(bool uploadFile = false, Guid? evidenceFileId = null) =>
        new()
        {
            UploadEvidence = uploadFile,
            UploadedEvidenceFile = uploadFile
                ? new()
                {
                    FileId = evidenceFileId ?? Guid.NewGuid(),
                    FileName = "evidence.jpeg",
                    FileSizeDescription = "5MB"
                }
                : null
        };

    protected EditInductionState? GetJourneyInstanceState(EditInductionJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditInductionState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    // Seeds the questions the journey would have walked through, so that the page under test is
    // reachable and its back link is the one the real journey would give it. A question is in the path
    // when the journey started at or before it and the status asks for it.
    private static string[] GetPathUrls(Guid personId, InductionStatus status, StartPage startPage)
    {
        List<string> urls = [];

        if (startPage == StartPage.Status)
        {
            urls.Add(PageUrl("status"));
        }

        if (startPage <= StartPage.ExemptionReasons && status.RequiresExemptionReasons())
        {
            urls.Add(PageUrl("exemption-reasons"));
        }

        if (startPage <= StartPage.StartDate && status.RequiresStartDate())
        {
            urls.Add(PageUrl("start-date"));
        }

        if (startPage <= StartPage.CompletedDate && status.RequiresCompletedDate())
        {
            urls.Add(PageUrl("date-completed"));
        }

        urls.AddRange([PageUrl("reason"), PageUrl("check-answers")]);

        return urls.ToArray();

        string PageUrl(string page) => $"/persons/{personId}/edit-induction/{page}";
    }
}
