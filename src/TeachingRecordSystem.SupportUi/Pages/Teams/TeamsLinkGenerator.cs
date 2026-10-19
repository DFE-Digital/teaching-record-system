namespace TeachingRecordSystem.SupportUi.Pages.Teams;

public class TeamsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() => linkGenerator.GetRequiredPathByPage("/Teams/Index");
    public string Add() => linkGenerator.GetRequiredPathByPage("/Teams/Add");
    public string Edit(Guid teamId) => linkGenerator.GetRequiredPathByPage("/Teams/Edit", routeValues: new { teamId });
    public string Delete(Guid teamId) => linkGenerator.GetRequiredPathByPage("/Teams/Delete", routeValues: new { teamId });
}
