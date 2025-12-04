using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public class ResolveOneLoginUserIdVerificationState : IRegisterJourney
{
    public static Guid DoNotConnectARecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveOneLoginUserIdVerification,
        typeof(ResolveOneLoginUserIdVerificationState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public bool? CanIdentityBeVerified { get; set; }

    public Guid? MatchedPersonId { get; set; }

    public class ResolveOneLoginUserIdVerificationStateFactory() : IJourneyStateFactory<ResolveOneLoginUserIdVerificationState>
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

            var state = new ResolveOneLoginUserIdVerificationState
            {

            };
            return state;
        }
    }
}
