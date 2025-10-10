namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class AddRouteLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Index", routeValues: new { personId });

    public string Route(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Route", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RouteCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Route", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Status(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StatusCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string StartAndEndDate(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/StartAndEndDates", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StartAndEndDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/StartAndEndDates", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string HoldsFrom(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/HoldsFrom", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string HoldsFromCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/HoldsFrom", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string InductionExemption(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/InductionExemption", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string InductionExemptionCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/InductionExemption", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string TrainingProvider(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/TrainingProvider", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string TrainingProviderCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/TrainingProvider", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string DegreeType(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/DegreeType", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string DegreeTypeCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/DegreeType", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Country(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Country", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string CountryCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Country", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string SubjectSpecialisms(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/SubjectSpecialisms", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string SubjectSpecialismsCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/SubjectSpecialisms", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AgeRangeSpecialism(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/AgeRangeSpecialism", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AgeRangeSpecialismCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/AgeRangeSpecialism", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string ReasonCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Reason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AddRoutePage(AddRoutePage page, Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        page switch
        {
            RoutesToProfessionalStatus.AddRoutePage.Route => Route(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.Status => Status(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.StartAndEndDate => StartAndEndDate(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.HoldsFrom => HoldsFrom(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.InductionExemption => InductionExemption(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.TrainingProvider => TrainingProvider(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.DegreeType => DegreeType(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.Country => Country(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.AgeRangeSpecialism => AgeRangeSpecialism(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.SubjectSpecialisms => SubjectSpecialisms(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.ChangeReason => Reason(personId, journeyInstanceId, fromCheckAnswers),
            RoutesToProfessionalStatus.AddRoutePage.CheckAnswers => CheckAnswers(personId, journeyInstanceId),
            _ => throw new ArgumentOutOfRangeException($"{nameof(RoutesToProfessionalStatus.AddRoutePage)}: {page.ToString()}")
        };
}
