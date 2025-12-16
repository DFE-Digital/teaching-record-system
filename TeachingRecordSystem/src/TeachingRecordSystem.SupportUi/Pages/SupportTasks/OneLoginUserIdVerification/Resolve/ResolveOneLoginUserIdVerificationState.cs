using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public class ResolveOneLoginUserIdVerificationState : IRegisterJourney
{
    public static Guid NotMatchedPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveOneLoginUserIdVerification,
        typeof(ResolveOneLoginUserIdVerificationState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public required IReadOnlyCollection<ResolveOneLoginUserIdVerificationStateMatch> MatchedPersons { get; set; }

    public bool? Verified { get; set; }

    public Guid? MatchedPersonId { get; set; }

    public OneLoginIdVerificationRejectReason? RejectReason { get; set; }

    public string? RejectionAdditionalDetails { get; set; }

    public OneLoginIdVerificationNotConnectingReason? NotConnectingReason { get; set; }

    public string? NotConnectingAdditionalDetails { get; set; }
}

[UsedImplicitly]
public class ResolveOneLoginUserIdVerificationStateFactory(OneLoginService oneLoginService) :
    IJourneyStateFactory<ResolveOneLoginUserIdVerificationState>
{
    public Task<ResolveOneLoginUserIdVerificationState> CreateAsync(CreateJourneyStateContext context)
    {
        var supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return CreateAsync(supportTask);
    }

    public async Task<ResolveOneLoginUserIdVerificationState> CreateAsync(SupportTask supportTask)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification);
        var requestData = supportTask.Data as OneLoginUserIdVerificationData;

        var matchResult = await oneLoginService.GetSuggestedPersonMatchesAsync(new(
            Names: [[requestData!.StatedFirstName, requestData.StatedLastName]],
            DatesOfBirth: [requestData.StatedDateOfBirth],
            NationalInsuranceNumber: requestData.StatedNationalInsuranceNumber,
            Trn: requestData.StatedTrn,
            TrnTokenTrnHint: null));

        var state = new ResolveOneLoginUserIdVerificationState
        {
            MatchedPersons = matchResult.Select(m => new ResolveOneLoginUserIdVerificationStateMatch(m.PersonId, m.MatchedAttributes)).ToArray()
        };

        return state;
    }
}

public record ResolveOneLoginUserIdVerificationStateMatch(Guid PersonId, IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes);
