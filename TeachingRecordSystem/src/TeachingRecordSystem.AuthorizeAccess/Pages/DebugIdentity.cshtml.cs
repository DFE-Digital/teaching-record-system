using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.FormFlowJourney)]
public class DebugIdentityModel(
    TrsDbContext dbContext,
    SignInJourneyHelper helper,
    IOptions<AuthorizeAccessOptions> optionsAccessor) : PageModel
{
    private OneLoginUser? _oneLoginUser;

    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    [Display(Name = "TRN token")]
    public string? TrnToken { get; set; }

    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [BindProperty]
    [Display(Name = "Identity verified")]
    public bool IdentityVerified { get; set; }

    [BindProperty]
    [Display(Name = "Verified names")]
    public string? VerifiedNames { get; set; }

    [BindProperty]
    [Display(Name = "Verified dates of birth")]
    public string? VerifiedDatesOfBirth { get; set; }

    public string? CoreIdentityJwt { get; set; }

    public PersonInfo? Person { get; set; }

    [BindProperty]
    public bool DetachPerson { get; set; }

    public void OnGet()
    {
        IdentityVerified = JourneyInstance!.State.IdentityVerified;

        if (IdentityVerified)
        {
            VerifiedNames = string.Join("\n", JourneyInstance.State.VerifiedNames!.Select(name => string.Join(" ", name)));
            VerifiedDatesOfBirth = string.Join("\n", JourneyInstance.State.VerifiedDatesOfBirth!.Select(dob => dob.ToString("dd/MM/yyyy")));
        }
    }

    public async Task<IActionResult> OnPost()
    {
        string[][]? verifiedNames;
        DateOnly[]? verifiedDatesOfBirth;

        if (IdentityVerified)
        {
            verifiedNames = (VerifiedNames ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray())
                .ToArray();

            if (verifiedNames.Length == 0)
            {
                ModelState.AddModelError(nameof(VerifiedNames), "Enter at least one name");
            }
            else if (verifiedNames.Any(name => name.Length < 2))
            {
                ModelState.AddModelError(nameof(VerifiedNames), "Each name must have at least two parts");
            }

            var dobs = (VerifiedDatesOfBirth ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => DateOnly.TryParseExact(line, "d/M/yyyy", out var parsed) ? parsed : (DateOnly?)null)
                .ToArray();

            if (dobs.Length == 0)
            {
                ModelState.AddModelError(nameof(VerifiedDatesOfBirth), "Enter at least one date of birth");
            }
            else if (dobs.Any(dob => dob is null))
            {
                ModelState.AddModelError(nameof(VerifiedDatesOfBirth), "Each date of birth must be in the dd/mm/yyyy format");
            }

            verifiedDatesOfBirth = dobs.Where(d => d.HasValue).Select(d => d!.Value).ToArray();
        }
        else
        {
            verifiedNames = null;
            verifiedDatesOfBirth = null;
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (_oneLoginUser!.PersonId is not null && !DetachPerson)
        {
            await JourneyInstance!.UpdateStateAsync(state => helper.Complete(state, _oneLoginUser.Person!.Trn!));
            return GetNextPage();
        }

        if (_oneLoginUser!.PersonId is not null && DetachPerson)
        {
            _oneLoginUser.PersonId = null;
        }

        if (IdentityVerified)
        {
            await helper.OnUserVerifiedCore(JourneyInstance!, verifiedNames!, verifiedDatesOfBirth!, coreIdentityClaimVc: null);
        }
        else
        {
            _oneLoginUser!.VerifiedOn = null;
            _oneLoginUser.VerificationRoute = null;
            _oneLoginUser.VerifiedNames = null;
            _oneLoginUser.VerifiedDatesOfBirth = null;

            await JourneyInstance!.UpdateStateAsync(state => state.ClearVerified());
        }

        await dbContext.SaveChangesAsync();

        return GetNextPage();

        IActionResult GetNextPage() => helper.GetNextPage(JourneyInstance!).ToActionResult();
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!optionsAccessor.Value.ShowDebugPages)
        {
            context.Result = NotFound();
            return;
        }

        if (JourneyInstance!.State.OneLoginAuthenticationTicket is null)
        {
            context.Result = BadRequest();
            return;
        }

        TrnToken = JourneyInstance.State.TrnToken;
        Subject = User.FindFirstValue("sub");
        Email = User.FindFirstValue("email");

        _oneLoginUser = await dbContext.OneLoginUsers
            .Include(o => o.Person)
            .FirstOrDefaultAsync(o => o.Subject == Subject);

        if (_oneLoginUser?.Person is Person person)
        {
            Person = new(person.PersonId, person.Trn, person.FirstName, person.LastName, person.DateOfBirth, person.NationalInsuranceNumber);
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record PersonInfo(Guid PersonId, string? Trn, string FirstName, string LastName, DateOnly? DateOfBirth, string? NationalInsuranceNumber);
}
