namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public class ResolveOneLoginUserIdVerificationState : IRegisterJourney
{
    public static JourneyDescriptor Journey { get; } = new (
        JourneyNames.ResolveOneLoginUserIdVerification,
        typeof(ResolveOneLoginUserIdVerificationState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public bool? CanIdentityBeVerified { get; set; }
}
