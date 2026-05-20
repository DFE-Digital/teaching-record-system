namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;

public class NoMatchesNpqTrnRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/NoMatches/Index", routeValues: new { supportTaskReference });

    public string CheckAnswers(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/NoMatches/CheckAnswers", routeValues: new { supportTaskReference });

    public string CheckAnswersCancel(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/NoMatches/CheckAnswers", handler: "Cancel", routeValues: new { supportTaskReference });
}
