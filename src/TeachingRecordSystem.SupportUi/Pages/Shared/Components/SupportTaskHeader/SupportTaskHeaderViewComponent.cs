using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.SupportTaskHeader;

public class SupportTaskHeaderViewComponent(TrsDbContext dbContext) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string? pageHeader = null)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        pageHeader ??= supportTask.GetSubject();

        var notes = await dbContext.SupportTaskNotes
            .Where(n => n.SupportTaskReference == supportTask.SupportTaskReference)
            .Select(n => new SupportTaskHeaderViewModelNote { Content = n.Content, CreatedBy = n.CreatedBy!.Name, CreatedOn = n.CreatedOn })
            .ToArrayAsync();

        var vm = new SupportTaskHeaderViewModel
        {
            PageHeader = pageHeader,
            SupportTaskReference = supportTask.SupportTaskReference,
            Type = supportTask.SupportTaskType,
            Status = supportTask.Status,
            AssignedToUserName = supportTask.AssignedTo?.Name,
            Notes = notes
        };

        return View(vm);
    }
}
