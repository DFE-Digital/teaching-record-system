namespace TeachingRecordSystem.SupportUi.Pages.Users.EditUser;

public class EditUserLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid userId) =>
        linkGenerator.GetRequiredPathByPage("/Users/EditUser/Index", routeValues: new { userId });

    public string Deactivate(Guid userId) =>
        linkGenerator.GetRequiredPathByPage("/Users/EditUser/Deactivate", routeValues: new { userId });

    public string DeactivateCancel(Guid userId) =>
        linkGenerator.GetRequiredPathByPage("/Users/EditUser/Deactivate", "cancel", routeValues: new { userId });
}
