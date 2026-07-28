namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class ResolveTrnRequestManualChecksNeededLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Resolve/Index", routeValues: new { supportTaskReference, returnUrl });

    public string Confirm(string supportTaskReference, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Resolve/Confirm", routeValues: new { supportTaskReference, returnUrl });
}
