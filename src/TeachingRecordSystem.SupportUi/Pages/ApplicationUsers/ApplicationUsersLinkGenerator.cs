using TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.AddApplicationUser;
using TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.EditApplicationUser;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;

public class ApplicationUsersLinkGenerator(LinkGenerator linkGenerator)
{
    public AddApplicationUserLinkGenerator AddApplicationUser => new(linkGenerator);
    public EditApplicationUserLinkGenerator EditApplicationUser => new(linkGenerator);

    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/ApplicationUsers/Index");
}
