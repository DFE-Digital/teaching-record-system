using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Pages.ApiKeys;
using TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;
using TeachingRecordSystem.SupportUi.Pages.ChangeRequests;
using TeachingRecordSystem.SupportUi.Pages.Mqs;
using TeachingRecordSystem.SupportUi.Pages.Persons;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Users;

namespace TeachingRecordSystem.SupportUi;

public class SupportUiLinkGenerator(LinkGenerator linkGenerator)
{
    protected const string DateOnlyFormat = DateOnlyModelBinder.Format;

    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/Index");

    public string SignOut() =>
        linkGenerator.GetRequiredPathByPage("/SignOut");

    public string SignedOut() =>
        linkGenerator.GetRequiredPathByPage("/SignedOut");

    public AlertsLinkGenerator Alerts => new(linkGenerator);
    public ApiKeysLinkGenerator ApiKeys => new(linkGenerator);
    public ApplicationUsersLinkGenerator ApplicationUsers => new(linkGenerator);
    public ChangeRequestsLinkGenerator ChangeRequests => new(linkGenerator);
    public MqsLinkGenerator Mqs => new(linkGenerator);
    public PersonsLinkGenerator Persons => new(linkGenerator);
    public RoutesToProfessionalStatusLinkGenerator RoutesToProfessionalStatus => new(linkGenerator);
    public SupportTasksLinkGenerator SupportTasks => new(linkGenerator);
    public UsersLinkGenerator Users => new(linkGenerator);
}
