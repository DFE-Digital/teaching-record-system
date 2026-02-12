using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class ConnectOneLoginState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.ConnectOneLogin,
        typeof(ConnectOneLoginState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public required string Subject { get; set; }
    public MatchPersonResult? MatchedPerson { get; set; }
    public string? Reason { get; set; }

    public bool Initialized { get; set; }

    public bool IsComplete => !string.IsNullOrEmpty(Subject) && !string.IsNullOrEmpty(Reason);
}

[UsedImplicitly]
public class ConnectOneLoginStateFactory(
    TrsDbContext dbContext,
    OneLoginService oneLoginService) : IJourneyStateFactory<ConnectOneLoginState>
{
    public async Task<ConnectOneLoginState> CreateAsync(CreateJourneyStateContext context)
    {
        var personId = context.HttpContext.GetCurrentPersonFeature().PersonId;

        var subject = context.HttpContext.Request.Query["subject"].ToString();
        if (string.IsNullOrEmpty(subject))
        {
            throw new InvalidOperationException("Subject not found in query string.");
        }

        var oneLoginUser = await dbContext.OneLoginUsers
            .AsNoTracking()
            .Where(u => u.Subject == subject)
            .SingleOrDefaultAsync();

        if (oneLoginUser is null)
        {
            throw new InvalidOperationException($"OneLoginUser with subject '{subject}' not found.");
        }

        var suggestedMatches = await oneLoginService.GetSuggestedPersonMatchesAsync(new GetSuggestedPersonMatchesOptions(
            Names: oneLoginUser.VerifiedNames ?? [],
            DatesOfBirth: oneLoginUser.VerifiedDatesOfBirth ?? [],
            EmailAddress: oneLoginUser.EmailAddress,
            NationalInsuranceNumber: null,
            Trn: null,
            TrnTokenTrnHint: null,
            PersonId: personId));

        return new ConnectOneLoginState
        {
            Subject = subject,
            MatchedPerson = suggestedMatches.FirstOrDefault(),
            Initialized = true
        };
    }
}
