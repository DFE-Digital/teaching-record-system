using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class Index(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<Index> _validator = new()
    {
        v => v.RuleFor(m => m.ChecksCompleted)
            .NotNull().WithMessage("You must complete all checks before confirming")
    };

    [FromRoute]
    public string? SupportTaskReference { get; set; }

    [BindProperty]
    public bool? ChecksCompleted { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public int OpenAlertsCount { get; set; }

    public bool HasOpenAlerts => OpenAlertsCount > 0;

    public bool HasQts { get; set; }

    public bool HasEyts { get; set; }

    public int FlagCount { get; set; }

    public Guid PersonId { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        _validator.ValidateAndThrow(this);

        if (ChecksCompleted == false)
        {
            return Redirect(linkGenerator.SupportTasks.TrnRequestManualChecksNeeded.Index());
        }

        return Redirect(linkGenerator.SupportTasks.TrnRequestManualChecksNeeded.Resolve.Confirm(SupportTaskReference!));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        PersonId = supportTask.TrnRequestMetadata!.ResolvedPersonId!.Value;

        var person = await dbContext.Persons
            .Where(p => p.PersonId == PersonId)
            .Select(p => new
            {
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.DateOfBirth,
                p.EmailAddress,
                p.NationalInsuranceNumber,
                OpenAlertsCount = p.Alerts!.Count,
                HasQts = p.QtsDate != null,
                HasEyts = p.EytsDate != null
            })
            .SingleAsync();

        FirstName = person.FirstName;
        MiddleName = person.MiddleName;
        LastName = person.LastName;
        DateOfBirth = person.DateOfBirth!.Value;
        EmailAddress = person.EmailAddress;
        NationalInsuranceNumber = person.NationalInsuranceNumber;
        OpenAlertsCount = person.OpenAlertsCount;
        HasQts = person.HasQts;
        HasEyts = person.HasEyts;
        FlagCount = new[] { HasOpenAlerts, HasQts, HasEyts }.Count(f => f);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
