using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class PersonDetailLinkGenerator(LinkGenerator linkGenerator)
{
    public EditDetailsLinkGenerator EditDetails { get; } = new(linkGenerator);
    public EditInductionLinkGenerator EditInduction { get; } = new(linkGenerator);
    public SetStatusLinkGenerator SetStatus { get; } = new(linkGenerator);
    public DisconnectOneLoginLinkGenerator DisconnectOneLogin { get; } = new(linkGenerator);

    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/Index", routeValues: new { personId });

    public string Qualifications(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/Qualifications", routeValues: new { personId });

    public string Induction(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/Induction", routeValues: new { personId });

    public string Alerts(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/Alerts", routeValues: new { personId });

    public string ChangeHistory(Guid personId, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ChangeHistory", routeValues: new { personId, pageNumber });

    public string Notes(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/Notes", routeValues: new { personId });

    public string AddNote(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/AddNote", routeValues: new { personId });
}
