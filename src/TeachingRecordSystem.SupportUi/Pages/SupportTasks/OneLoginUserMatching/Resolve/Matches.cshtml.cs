using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class Matches(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    IFeatureProvider featureProvider) :
    PageModel
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    private readonly InlineValidator<Matches> _validator = new()
    {
        v => v.RuleFor(m => m.MatchedPersonId)
            .NotNull().WithMessage("Select what you want to do with this GOV.UK One Login")
    };

    private SupportTask? _supportTask;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    public Guid? MatchedPersonId { get; set; }

    public string? BackLink { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }
    public string? QtsYearReceived { get; set; }
    public string? QtsProvider { get; set; }
    public string? QtsSubject { get; set; }

    public IReadOnlyCollection<SuggestedMatchViewModel>? SuggestedMatches { get; set; }

    public void OnGet()
    {
        journey.State.ApplySavedModelStateValues(nameof(Matches), ModelState);
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            journey.DeleteInstance();

            return Redirect(journey.State.CompletionUrl);
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
        }

        await this.ThrowIfInvalidAsync(_validator);

        var nextStepUrl = MatchedPersonId != ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel ?
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmConnect(journey.InstanceId) :
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.NotConnecting(journey.InstanceId);

        return journey.AdvanceTo(
            nextStepUrl,
            state =>
            {
                state.MatchedPersonId = MatchedPersonId;
                state.ClearSavedModelStateValues(nameof(Matches));
            });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(Matches),
            journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processType = _supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving :
            ProcessType.OneLoginUserRecordMatchingSupportTaskSaving;

        var processContext = new ProcessContext(processType, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = _supportTask.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(_supportTask.SupportTaskReference));
        }

        return Redirect(journey.State.CompletionUrl);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        BackLink = journey.GetBackLink() ?? journey.State.CompletionUrl;

        var oneLoginUser = _supportTask.OneLoginUser!;
        var data = _supportTask.GetData<IOneLoginUserMatchingData>();

        // For the time being only display first verified name and dob if there are multiples (but still match on both)
        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        Name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        DateOfBirth = data.VerifiedOrStatedDatesOfBirth!.First();
        NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(data.StatedNationalInsuranceNumber);
        Trn = TrnHelper.NormalizeTrn(data.StatedTrn);
        EmailAddress = oneLoginUser.EmailAddress;
        QtsYearReceived = data.YearQtsReceived;
        QtsProvider = data.TrainingProviderId is Guid trainingProviderId ?
            (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId)).Name :
            data.TrainingProviderName;
        QtsSubject = data.SubjectId is Guid subjectId ?
            (await referenceDataCache.GetTrainingSubjectByIdAsync(subjectId)).Name :
            data.SubjectName;

        var matchedPersonIds = journey.State.MatchedPersons.Select(m => m.PersonId).ToArray();
        var matches = (await dbContext.Persons
                .Include(p => p.PreviousNames)
                .Include(p => p.Qualifications)
                .Where(p => matchedPersonIds.Contains(p.PersonId))
            .Select(p => new
            {
                p.PersonId,
                p.Trn,
                p.EmailAddress,
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.DateOfBirth,
                p.NationalInsuranceNumber,
                p.PreviousNames,
                p.Qualifications
            })
            .ToArrayAsync())
            .OrderBy(p => Array.IndexOf(matchedPersonIds, p.PersonId));  // Ensure we maintain the order of matches

        var suggestedMatches = new List<SuggestedMatchViewModel>();

        foreach (var match in matches.Select((match, idx) => new { Match = match, Index = idx }))
        {
            suggestedMatches.Add(new SuggestedMatchViewModel
            {
                Identifier = (char)('A' + match.Index),
                PersonId = match.Match.PersonId,
                Trn = match.Match.Trn,
                EmailAddress = match.Match.EmailAddress,
                FirstName = match.Match.FirstName,
                MiddleName = match.Match.MiddleName,
                LastName = match.Match.LastName,
                DateOfBirth = match.Match.DateOfBirth,
                NationalInsuranceNumber = match.Match.NationalInsuranceNumber,
                PreviousNames = match.Match.PreviousNames!
                    .OrderBy(n => n.CreatedOn)
                    .Select(n => $"{n.FirstName} {n.MiddleName} {n.LastName}")
                    .ToArray(),
                QtlsDetails = await GetQtlsDetailsAsync(
                    match.Match.Qualifications!.OfType<RouteToProfessionalStatus>()
                        .Where(route => route.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && route.Status == RouteToProfessionalStatusStatus.Holds)
                        .OrderByDescending(route => route.HoldsFrom)
                        .ThenByDescending(route => route.CreatedOn)
                        .FirstOrDefault(),
                    QtsYearReceived,
                    QtsSubject),
                QtsDetails = await GetQtsDetailsAsync(
                    match.Match.Qualifications!.OfType<RouteToProfessionalStatus>(),
                    QtsYearReceived,
                    QtsProvider,
                    QtsSubject),
                MatchedAttributeTypes = journey.State.MatchedPersons.Single(m => m.PersonId == match.Match.PersonId)
                    .MatchedAttributes
                    .Select(kvp => kvp.Key)
                    .ToArray()
            });
        }

        SuggestedMatches = suggestedMatches;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    private async Task<SuggestedMatchProfessionalStatusDetailsViewModel?> GetQtlsDetailsAsync(
        RouteToProfessionalStatus? route,
        string? requestYearReceived,
        string? requestSubject)
    {
        if (route is null)
        {
            return null;
        }

        return await BuildProfessionalStatusDetailsAsync(
            route,
            heading: "QTS - QTLS and SET Membership",
            requestYearReceived,
            requestProvider: null,
            requestSubject,
            showProvider: false,
            referenceDataCache);
    }

    private async Task<IReadOnlyCollection<SuggestedMatchProfessionalStatusDetailsViewModel>> GetQtsDetailsAsync(
        IEnumerable<RouteToProfessionalStatus> routes,
        string? requestYearReceived,
        string? requestProvider,
        string? requestSubject)
    {
        var qtsRoutes = routes
            .Where(x => x.Status == RouteToProfessionalStatusStatus.Holds)
            .OrderByDescending(route => route.HoldsFrom)
            .ThenByDescending(route => route.CreatedOn);

        var qtsDetails = new List<SuggestedMatchProfessionalStatusDetailsViewModel>();

        foreach (var route in qtsRoutes)
        {
            var routeType = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(route.RouteToProfessionalStatusTypeId);

            if (routeType.ProfessionalStatusType is not ProfessionalStatusType.QualifiedTeacherStatus ||
                route.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId)
            {
                continue;
            }

            var details = await BuildProfessionalStatusDetailsAsync(
                route,
                heading: $"QTS - {routeType.Name}",
                requestYearReceived,
                requestProvider,
                requestSubject,
                showProvider: true,
                referenceDataCache);

            qtsDetails.Add(details);
        }

        return qtsDetails;
    }

    private static async Task<SuggestedMatchProfessionalStatusDetailsViewModel> BuildProfessionalStatusDetailsAsync(
        RouteToProfessionalStatus route,
        string heading,
        string? requestYearReceived,
        string? requestProvider,
        string? requestSubject,
        bool showProvider,
        ReferenceDataCache referenceDataCache)
    {
        var yearReceived = route.HoldsFrom?.Year.ToString(CultureInfo.InvariantCulture);
        var provider = showProvider && route.TrainingProviderId is Guid trainingProviderId ?
            (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId)).Name :
            null;
        var subjects = route.TrainingSubjectIds is { Length: > 0 } ?
            await Task.WhenAll(route.TrainingSubjectIds.Select(async subjectId => (await referenceDataCache.GetTrainingSubjectByIdAsync(subjectId)).Name)) :
            [];

        return new SuggestedMatchProfessionalStatusDetailsViewModel
        {
            Heading = heading,
            YearReceived = yearReceived,
            Provider = provider,
            Subjects = subjects,
        };
    }

    private static string? NormalizeComparisonValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
