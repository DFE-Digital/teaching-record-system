using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

public class AddNote(
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) :
    PageModel
{
    private readonly InlineValidator<AddNote> _validator = new()
    {
        v => v.RuleFor(m => m.Content)
            .SupportTaskNoteContent(
                notEmptyMessage: "Enter text for the note",
                maxLengthMessage: maxLength => $"Note must be {maxLength} characters or less")
    };

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public string SupportTaskTypeTitle { get; set; } = null!;

    public SupportTaskType SupportTaskType { get; set; }

    [BindProperty]
    public new string? Content { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        var processContext = new ProcessContext(ProcessType.SupportTaskNoteCreating, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.CreateNoteAsync(
            new CreateSupportTaskNoteOptions
            {
                SupportTaskReference = SupportTaskReference,
                Content = Content!,
                CreatedByUserId = User.GetUserId()
            },
            processContext);

        return Redirect(linkGenerator.SupportTaskDetail(SupportTaskReference, SupportTaskType));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        SupportTaskType = supportTask.SupportTaskType;
        SupportTaskTypeTitle = supportTask.SupportTaskType.GetTitle();
    }
}
