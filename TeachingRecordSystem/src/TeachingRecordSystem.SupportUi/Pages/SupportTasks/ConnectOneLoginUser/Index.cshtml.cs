using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ConnectOneLoginUser;

public class IndexModel(TrsDbContext dbContext, IPersonMatchingService personMatchingService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    public const string NoneOfTheAboveTrnSentinel = "0000000";

    [FromRoute]
    public string? SupportTaskReference { get; set; }

    [BindProperty]
    public string? Trn { get; set; }

    [BindProperty]
    public string? TrnOverride { get; set; }

    public string? Email { get; set; }
    public string? VerifiedName { get; set; }
    public string[]? VerifiedPreviousNames { get; set; }
    public DateOnly VerifiedDateOfBirth { get; set; }
    public string? StatedNationalInsuranceNumber { get; set; }
    public string? StatedTrn { get; set; }
    public PersonDetailViewModel[]? SuggestedMatches { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (SuggestedMatches!.Length > 0 && Trn is null)
        {
            ModelState.AddModelError(nameof(Trn), "Select the record you wish to connect");
            return this.PageWithErrors();
        }

        var trn = Trn!;

        if (Trn == NoneOfTheAboveTrnSentinel)
        {
            trn = TrnOverride!;

            if (!await dbContext.Persons.AnyAsync(p => p.Trn == trn))
            {
                ModelState.AddModelError(nameof(TrnOverride), "Enter a valid TRN");
                return this.PageWithErrors();
            }
        }

        return Redirect(linkGenerator.SupportTasks.ConnectOneLoginUser.Connect(SupportTaskReference!, trn));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var data = (ConnectOneLoginUserData)supportTask.Data;

        var suggestedMatches = await personMatchingService.GetSuggestedOneLoginUserMatchesAsync(new(
            data.VerifiedNames!,
            data.VerifiedDatesOfBirth!,
            data.StatedNationalInsuranceNumber,
            data.StatedTrn,
            data.TrnTokenTrn));

        SupportTaskReference = supportTask.SupportTaskReference;
        Email = data.OneLoginUserEmail;
        VerifiedName = FormatName(data.VerifiedNames!.First());
        VerifiedPreviousNames = data.VerifiedNames!.Skip(1).Select(FormatName).ToArray();
        VerifiedDateOfBirth = data.VerifiedDatesOfBirth!.First();
        StatedNationalInsuranceNumber = data.StatedNationalInsuranceNumber;
        StatedTrn = data.StatedTrn;

        SuggestedMatches = suggestedMatches
            .Select(m => new PersonDetailViewModel()
            {
                PersonId = m.PersonId,
                Options = PersonDetailViewModelOptions.None,
                Trn = m.Trn,
                Name = $"{m.FirstName} {m.MiddleName} {m.LastName}",
                PreviousNames = [],  // TODO When we've got previous names synced to TRS
                DateOfBirth = m.DateOfBirth,
                NationalInsuranceNumber = m.NationalInsuranceNumber,
                Gender = null,  // Not shown
                Email = m.EmailAddress,
                CanChangeDetails = false,
                IsActive = null  // Not shown
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);

        static string FormatName(string[] nameParts) => string.Join(" ", nameParts);
    }
}
