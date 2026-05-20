namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

public class AddUserLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/Users/AddUser/Index");

    public string Confirm(string userId) =>
        linkGenerator.GetRequiredPathByPage("/Users/AddUser/Confirm", routeValues: new { userId });
}
