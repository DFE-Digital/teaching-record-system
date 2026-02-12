using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class IndexModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.EmailAddress)
            .NotEmpty()
            .WithMessage("Enter a GOV.UK One Login email address")
            .EmailAddress()
    };

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public string? EmailAddress { get; set; }

    public string? Trn { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var validationResult = await _validator.ValidateAsync(this);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return this.PageWithErrors();
        }

        var oneLoginUser = await dbContext.OneLoginUsers
            .Where(u => u.EmailAddress == EmailAddress)
            .FirstOrDefaultAsync();

        if (oneLoginUser is null)
        {
            ModelState.AddModelError(nameof(EmailAddress), "No GOV.UK One Login user found with this email address");
            return this.PageWithErrors();
        }

        if (oneLoginUser.PersonId is not null)
        {
            var errorMessage = oneLoginUser.PersonId == PersonId
                ? "This GOV.UK One Login user is already connected to this record"
                : "This GOV.UK One Login user is already connected to another record";
            
            ModelState.AddModelError(nameof(EmailAddress), errorMessage);
            return this.PageWithErrors();
        }

        return Redirect(linkGenerator.Persons.PersonDetail.ConnectOneLogin.Match(PersonId, oneLoginUser.Subject));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var person = await dbContext.Persons
            .Where(p => p.PersonId == PersonId)
            .Select(p => new { p.PersonId, p.Trn })
            .FirstOrDefaultAsync();

        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        Trn = person.Trn;

        await next();
    }
}




