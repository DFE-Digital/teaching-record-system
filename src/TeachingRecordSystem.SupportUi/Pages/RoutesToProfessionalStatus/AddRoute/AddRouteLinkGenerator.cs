namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class AddRouteLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Index", routeValues: new { personId });

    public string Route(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/Route", journeyInstanceId, returnUrl);

    public string Status(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/Status", journeyInstanceId, returnUrl);

    public string StartAndEndDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/StartAndEndDates", journeyInstanceId, returnUrl);

    public string HoldsFrom(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/HoldsFrom", journeyInstanceId, returnUrl);

    public string InductionExemption(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/InductionExemption", journeyInstanceId, returnUrl);

    public string TrainingProvider(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/TrainingProvider", journeyInstanceId, returnUrl);

    public string DegreeType(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/DegreeType", journeyInstanceId, returnUrl);

    public string Country(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/Country", journeyInstanceId, returnUrl);

    public string AgeRangeSpecialism(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/AgeRangeSpecialism", journeyInstanceId, returnUrl);

    public string SubjectSpecialisms(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/SubjectSpecialisms", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/AddRoute/CheckAnswers", journeyInstanceId, returnUrl);

    public string Page(AddRoutePage page, JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        page switch
        {
            AddRoutePage.Route => Route(journeyInstanceId, returnUrl),
            AddRoutePage.Status => Status(journeyInstanceId, returnUrl),
            AddRoutePage.StartAndEndDate => StartAndEndDate(journeyInstanceId, returnUrl),
            AddRoutePage.HoldsFrom => HoldsFrom(journeyInstanceId, returnUrl),
            AddRoutePage.InductionExemption => InductionExemption(journeyInstanceId, returnUrl),
            AddRoutePage.TrainingProvider => TrainingProvider(journeyInstanceId, returnUrl),
            AddRoutePage.DegreeType => DegreeType(journeyInstanceId, returnUrl),
            AddRoutePage.Country => Country(journeyInstanceId, returnUrl),
            AddRoutePage.AgeRangeSpecialism => AgeRangeSpecialism(journeyInstanceId, returnUrl),
            AddRoutePage.SubjectSpecialisms => SubjectSpecialisms(journeyInstanceId, returnUrl),
            AddRoutePage.ChangeReason => Reason(journeyInstanceId, returnUrl),
            AddRoutePage.CheckAnswers => CheckAnswers(journeyInstanceId, returnUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
}
