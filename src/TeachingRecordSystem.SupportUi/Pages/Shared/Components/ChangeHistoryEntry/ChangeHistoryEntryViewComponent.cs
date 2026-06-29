using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;

public class ChangeHistoryEntryViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ChangeHistoryEntryViewModel model) => View(model);
}
