using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Views.Shared.Components.UploadEvidence
{
    public class UploadEvidenceViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(UploadEvidenceViewModel model)
        {
            return View(model);
        }
    }
}
