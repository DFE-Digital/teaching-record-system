namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

public class SupportTaskDetailLinkGenerator(LinkGenerator linkGenerator)
{
    public string Events(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/SupportTaskDetail/Events", routeValues: new { supportTaskReference });

    public string AddNote(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/SupportTaskDetail/AddNote", routeValues: new { supportTaskReference });
}
