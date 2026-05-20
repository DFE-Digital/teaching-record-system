namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.AddApplicationUser;

public class AddApplicationUserLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/ApplicationUsers/AddApplicationUser/Index");
}
