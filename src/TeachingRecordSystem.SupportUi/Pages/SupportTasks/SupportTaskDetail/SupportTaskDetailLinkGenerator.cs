namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

public class SupportTaskDetailLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference, bool? expandNotes = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/SupportTaskDetail/Index",
            routeValues: new { supportTaskReference, expandNotes, returnUrl });

    public string Events(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/SupportTaskDetail/Events",
            routeValues: new { supportTaskReference });

    public string AddNote(string supportTaskReference, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/SupportTaskDetail/AddNote",
            routeValues: new { supportTaskReference, returnUrl });
}
