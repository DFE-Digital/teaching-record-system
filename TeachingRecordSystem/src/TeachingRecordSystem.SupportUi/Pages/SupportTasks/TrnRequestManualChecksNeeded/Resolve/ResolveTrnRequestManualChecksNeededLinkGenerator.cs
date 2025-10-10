namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class ResolveTrnRequestManualChecksNeededLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Resolve/Index", routeValues: new { supportTaskReference });

    public string Confirm(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Resolve/Confirm", routeValues: new { supportTaskReference });
}
