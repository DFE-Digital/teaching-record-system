namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ConnectOneLoginUser;

public class ConnectOneLoginUserLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ConnectOneLoginUser/Index", routeValues: new { supportTaskReference });

    public string Connect(string supportTaskReference, string trn) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ConnectOneLoginUser/Connect", routeValues: new { supportTaskReference, trn });
}
