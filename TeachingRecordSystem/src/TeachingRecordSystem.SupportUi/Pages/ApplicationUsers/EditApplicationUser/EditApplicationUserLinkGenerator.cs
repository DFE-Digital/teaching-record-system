namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.EditApplicationUser;

public class EditApplicationUserLinkGenerator(LinkGenerator linkGenerator)
{
    public string EditApplicationUser(Guid userId) =>
        linkGenerator.GetRequiredPathByPage("/ApplicationUsers/Index", routeValues: new { userId });
}
