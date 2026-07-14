namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

public class SupportTaskDetailLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference, bool? expandNotes = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/SupportTaskDetail/Index", routeValues: new { supportTaskReference, expandNotes });

    public string Events(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/SupportTaskDetail/Events", routeValues: new { supportTaskReference });

    public string AddNote(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/SupportTaskDetail/AddNote", routeValues: new { supportTaskReference });
}
