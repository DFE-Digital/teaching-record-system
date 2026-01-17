namespace TeachingRecordSystem.AuthorizeAccess;

public static class SignInJourneyExtensions
{
    public static SignInJourneyCoordinator? GetSignInJourneyCoordinator(this IJourneyInstanceProvider journeyInstanceProvider, HttpContext httpContext) =>
        journeyInstanceProvider.GetJourneyInstance(httpContext) as SignInJourneyCoordinator;
}
