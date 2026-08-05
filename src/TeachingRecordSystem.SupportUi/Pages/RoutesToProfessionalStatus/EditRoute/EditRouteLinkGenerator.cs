namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class EditRouteLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId, bool? fromInductions = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Index", routeValues: new { qualificationId, fromInductions });

    public string Detail(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/Detail", journeyInstanceId, returnUrl);

    public string Status(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/Status", journeyInstanceId, returnUrl);

    public string StartAndEndDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/StartAndEndDates", journeyInstanceId, returnUrl);

    public string HoldsFrom(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/HoldsFrom", journeyInstanceId, returnUrl);

    public string InductionExemption(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/InductionExemption", journeyInstanceId, returnUrl);

    public string TrainingProvider(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/TrainingProvider", journeyInstanceId, returnUrl);

    public string DegreeType(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", journeyInstanceId, returnUrl);

    public string Country(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/Country", journeyInstanceId, returnUrl);

    public string AgeRangeSpecialism(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/AgeRangeSpecialism", journeyInstanceId, returnUrl);

    public string SubjectSpecialisms(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/EditRoute/CheckAnswers", journeyInstanceId, returnUrl);

    public string Page(EditRoutePage page, JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        page switch
        {
            EditRoutePage.Status => Status(journeyInstanceId, returnUrl),
            EditRoutePage.StartAndEndDate => StartAndEndDate(journeyInstanceId, returnUrl),
            EditRoutePage.HoldsFrom => HoldsFrom(journeyInstanceId, returnUrl),
            EditRoutePage.InductionExemption => InductionExemption(journeyInstanceId, returnUrl),
            EditRoutePage.TrainingProvider => TrainingProvider(journeyInstanceId, returnUrl),
            EditRoutePage.DegreeType => DegreeType(journeyInstanceId, returnUrl),
            EditRoutePage.Country => Country(journeyInstanceId, returnUrl),
            EditRoutePage.AgeRangeSpecialism => AgeRangeSpecialism(journeyInstanceId, returnUrl),
            EditRoutePage.SubjectSpecialisms => SubjectSpecialisms(journeyInstanceId, returnUrl),
            EditRoutePage.ChangeReason => Reason(journeyInstanceId, returnUrl),
            EditRoutePage.CheckAnswers => CheckAnswers(journeyInstanceId, returnUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
}
