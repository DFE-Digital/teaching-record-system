using TeachingRecordSystem.SupportUi.Pages.Users.AddUser;
using TeachingRecordSystem.SupportUi.Pages.Users.EditUser;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

public class UsersLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? keywords = null, string? status = null, string? role = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/Users/Index", routeValues: new { keywords, status, role, pageNumber });

    public AddUserLinkGenerator AddUser { get; } = new(linkGenerator);
    public EditUserLinkGenerator EditUser { get; } = new(linkGenerator);
}
