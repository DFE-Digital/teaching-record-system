namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.EditApplicationUser;

public class EditApplicationUserLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid userId) =>
        linkGenerator.GetRequiredPathByPage("/ApplicationUsers/EditApplicationUser/Index", routeValues: new { userId });
}
