namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionLinkGenerator(LinkGenerator linkGenerator)
{
    // The four questions the induction page links to are each a way into the journey, so they have an
    // overload that takes no instance — the library starts one on arrival.
    public string Status(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", routeValues: new { personId });

    public string Status(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditInduction/Status", journeyInstanceId, returnUrl);

    public string ExemptionReasons(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", routeValues: new { personId });

    public string ExemptionReasons(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", journeyInstanceId, returnUrl);

    public string StartDate(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", routeValues: new { personId });

    public string StartDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditInduction/StartDate", journeyInstanceId, returnUrl);

    public string CompletedDate(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", routeValues: new { personId });

    public string CompletedDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditInduction/CompletedDate", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditInduction/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditInduction/CheckAnswers", journeyInstanceId);
}
