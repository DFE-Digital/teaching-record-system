using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ConnectOneLoginUser;

public class ConnectModel(TrsDbContext dbContext, IPersonMatchingService personMatchingService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string? SupportTaskReference { get; set; }

    [FromQuery]
    public string? Trn { get; set; }

    public string? Email { get; set; }

    public PersonDetailViewModel? PersonDetail { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var data = _supportTask!.UpdateData<ConnectOneLoginUserData>(data => data with
        {
            PersonId = PersonDetail!.PersonId
        });
        _supportTask.Status = SupportTaskStatus.Closed;

        var matchedAttributes = (await personMatchingService.GetMatchedAttributesAsync(
                new(data.VerifiedNames!, data.VerifiedDatesOfBirth!, data.StatedNationalInsuranceNumber, data.StatedTrn, data.TrnTokenTrn),
                PersonDetail!.PersonId))
            .ToArray();

        var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == data.OneLoginUserSubject);
        oneLoginUser.SetMatched(
            PersonDetail!.PersonId,
            OneLoginUserMatchRoute.Support,
            matchedAttributes);

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            heading: "Teaching record connected",
            messageText: $"{PersonDetail.Name} will get an email to say they can sign in.");

        // TEMP, until we have a list page for these type of support tasks
        return Redirect(linkGenerator.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (Trn is null)
        {
            context.Result = BadRequest();
            return;
        }

        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var data = (ConnectOneLoginUserData)_supportTask.Data;

        PersonDetail = await dbContext.Persons
            .Where(p => p.Trn == Trn)
            .Select(p => new PersonDetailViewModel()
            {
                PersonId = p.PersonId,
                Options = PersonDetailViewModelOptions.None,
                Trn = p.Trn,
                Name = StringHelper.JoinNonEmpty(' ', p.FirstName, p.MiddleName, p.LastName),
                PreviousNames = Array.Empty<string>(),  // TODO When we've got previous names synced to TRS
                DateOfBirth = p.DateOfBirth,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                Gender = null,  // Not shown
                Email = p.EmailAddress,
                CanChangeDetails = false,
                IsActive = null  // Not shown
            })
            .SingleOrDefaultAsync();

        if (PersonDetail is null)
        {
            context.Result = BadRequest();
            return;
        }

        Email = data.OneLoginUserEmail;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
