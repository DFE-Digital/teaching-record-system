using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTaskNotes;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AddNote(
    TrsDbContext dbContext,
    SupportTaskNoteService supportTaskNoteService,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<AddNote> _validator = new()
    {
        v => v.RuleFor(m => m.Content)
            .NotEmpty().WithMessage("Enter text for the note")
            .MaximumLength(4000).WithMessage("Note must be 4000 characters or less")
    };

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public string SupportTaskTypeTitle { get; set; } = null!;

    public SupportTaskType SupportTaskType { get; set; }

    [BindProperty]
    public new string? Content { get; set; }

    public async Task OnGetAsync()
    {
        await LoadSupportTaskAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSupportTaskAsync();

        _validator.ValidateAndThrow(this);

        await supportTaskNoteService.CreateNoteAsync(
            new CreateSupportTaskNoteOptions
            {
                SupportTaskReference = SupportTaskReference,
                Content = Content!,
                CreatedByUserId = User.GetUserId()
            });

        return Redirect(linkGenerator.SupportTaskDetail(SupportTaskReference, SupportTaskType));
    }

    private async Task LoadSupportTaskAsync()
    {
        var supportTask = await dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == SupportTaskReference)
            .SingleAsync();

        SupportTaskType = supportTask.SupportTaskType;
        SupportTaskTypeTitle = supportTask.SupportTaskType.GetTitle();
    }
}
