using TeachingRecordSystem.SupportUi.Endpoints;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Pages.ApiKeys;
using TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;
using TeachingRecordSystem.SupportUi.Pages.Mqs;
using TeachingRecordSystem.SupportUi.Pages.OneLogins;
using TeachingRecordSystem.SupportUi.Pages.Persons;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Users;

namespace TeachingRecordSystem.SupportUi;

public class SupportUiLinkGenerator(LinkGenerator linkGenerator)
{
    protected const string DateOnlyFormat = DateOnlyModelBinder.Format;

    public string Index(string? selectedTab = null)
    {
        var path = linkGenerator.GetRequiredPathByPage("/Index");
        return selectedTab is not null ? $"{path}#{selectedTab}" : path;
    }

    public string SignOut() =>
        linkGenerator.GetRequiredPathByPage("/SignOut");

    public string SignedOut() =>
        linkGenerator.GetRequiredPathByPage("/SignedOut");

    public string SupportTaskResolve(string supportTaskReference, SupportTaskType supportTaskType, string? returnUrl = null) =>
        supportTaskType switch
        {
            SupportTaskType.OneLoginUserRecordMatching => SupportTasks.OneLoginUserMatching.Resolve.Index(supportTaskReference, returnUrl),
            SupportTaskType.TrnRequest => SupportTasks.TrnRequests.Resolve.Index(supportTaskReference, returnUrl),
            SupportTaskType.TrnRequestManualChecksNeeded => SupportTasks.TrnRequestManualChecksNeeded.Resolve.Index(supportTaskReference, returnUrl),
            SupportTaskType.ChangeDateOfBirthRequest => SupportTasks.ChangeRequests.EditChangeRequest.Index(supportTaskReference, returnUrl),
            SupportTaskType.ChangeNameRequest => SupportTasks.ChangeRequests.EditChangeRequest.Index(supportTaskReference, returnUrl),
            SupportTaskType.TeacherPensionsPotentialDuplicate => SupportTasks.TeacherPensions.Resolve.Index(supportTaskReference, returnUrl),
            SupportTaskType.OneLoginUserIdVerification => SupportTasks.OneLoginUserMatching.Resolve.Index(supportTaskReference, returnUrl),
            _ => throw new ArgumentException($"Unknown {nameof(SupportTaskType)}: '{supportTaskType}'.", nameof(supportTaskType))
        };

    public AlertsLinkGenerator Alerts => new(linkGenerator);
    public ApiKeysLinkGenerator ApiKeys => new(linkGenerator);
    public ApplicationUsersLinkGenerator ApplicationUsers => new(linkGenerator);
    public MqsLinkGenerator Mqs => new(linkGenerator);
    public OneLoginsLinkGenerator OneLogins => new(linkGenerator);
    public PersonsLinkGenerator Persons => new(linkGenerator);
    public RoutesToProfessionalStatusLinkGenerator RoutesToProfessionalStatus => new(linkGenerator);
    public SupportTasksLinkGenerator SupportTasks => new(linkGenerator);
    public UsersLinkGenerator Users => new(linkGenerator);
    public FilesLinkGenerator Files => new(linkGenerator);
}
