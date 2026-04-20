using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public record ResolveOneLoginUserMatchingState : IRegisterJourney, IJourneyWithSavedState
{
    public static Guid NotMatchedPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveOneLoginUserMatching,
        typeof(ResolveOneLoginUserMatchingState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public SavedJourneyState? SavedJourneyState { get; set; }

    public required IReadOnlyCollection<MatchPersonResult> MatchedPersons { get; set; }

    public bool? Verified { get; set; }

    public Guid? MatchedPersonId { get; set; }

    public OneLoginIdVerificationRejectReason? RejectReason { get; set; }

    public string? RejectionAdditionalDetails { get; set; }

    public OneLoginUserNotConnectingReason? NotConnectingReason { get; set; }

    public string? NotConnectingAdditionalDetails { get; set; }

    public AppContent? AppContent { get; set; }

    public RecordMatchingPolicy? RecordMatchingPolicy { get; set; }
}

[UsedImplicitly]
public class ResolveOneLoginUserMatchingStateFactory(
    OneLoginService oneLoginService,
    TrsDbContext dbContext) :
    IJourneyStateFactory<ResolveOneLoginUserMatchingState>
{
    public Task<ResolveOneLoginUserMatchingState> CreateAsync(CreateJourneyStateContext context)
    {
        var supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateAsync(supportTask);
    }

    public async Task<ResolveOneLoginUserMatchingState> CreateAsync(SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification or SupportTaskType.OneLoginUserRecordMatching);
        var requestData = supportTask.GetData<IOneLoginUserMatchingData>();
        var emailAddress = supportTask.OneLoginUser!.EmailAddress;

        IReadOnlyCollection<MatchPersonResult> suggestedMatches;

        if (string.IsNullOrWhiteSpace(requestData.StatedTrn))
        {
            suggestedMatches = [];
        }
        else
        {
            suggestedMatches = await oneLoginService.GetSuggestedPersonMatchesAsync(new(
                Names: requestData.VerifiedOrStatedNames!,
                DatesOfBirth: requestData.VerifiedOrStatedDatesOfBirth!,
                NationalInsuranceNumber: requestData.StatedNationalInsuranceNumber,
                EmailAddress: emailAddress,
                Trn: requestData.StatedTrn,
                TrnTokenTrnHint: requestData.TrnTokenTrn));

            var matchResult = oneLoginService.MatchPerson(suggestedMatches);
            if (matchResult is not null)
            {
                suggestedMatches = [matchResult];
            }
        }

        AppContent? appContent = null;
        RecordMatchingPolicy? recordMatchingPolicy = null;
        Guid clientApplicationUserId = requestData switch
        {
            OneLoginUserIdVerificationData idVerificationData => idVerificationData.ClientApplicationUserId,
            OneLoginUserRecordMatchingData recordMatchingData => recordMatchingData.ClientApplicationUserId,
            _ => Guid.Empty
        };

        if (clientApplicationUserId != Guid.Empty)
        {
            var applicationUser = await dbContext.ApplicationUsers
                .Where(u => u.UserId == clientApplicationUserId)
                .Select(u => new { u.AppContent, u.RecordMatchingPolicy })
                .SingleOrDefaultAsync();

            appContent = applicationUser?.AppContent;
            recordMatchingPolicy = applicationUser?.RecordMatchingPolicy;
        }

        return supportTask.ResolveJourneySavedState?.GetState<ResolveOneLoginUserMatchingState>() is { } existingState ?
            existingState with
            {
                MatchedPersons = suggestedMatches,
                AppContent = appContent,
                RecordMatchingPolicy = recordMatchingPolicy,
                SavedJourneyState = supportTask.ResolveJourneySavedState
            } :
            new ResolveOneLoginUserMatchingState
            {
                MatchedPersons = suggestedMatches,
                Verified = supportTask.SupportTaskType is SupportTaskType.OneLoginUserRecordMatching ? true : null,
                AppContent = appContent,
                RecordMatchingPolicy = recordMatchingPolicy
            };
    }
}
