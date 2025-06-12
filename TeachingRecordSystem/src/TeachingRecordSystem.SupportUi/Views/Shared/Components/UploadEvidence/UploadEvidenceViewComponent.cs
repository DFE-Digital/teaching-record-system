using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages;

namespace TeachingRecordSystem.SupportUi.Views.Shared.Components.UploadEvidence
{
    public delegate string GetFullHtmlFieldNameDelegate(ViewContext viewContext, string expression);

    public class UploadEvidenceViewComponent(IFileService fileService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(ModelExpression aspFor)
        {

            var s_getFullHtmlFieldNameDelegate =
                (GetFullHtmlFieldNameDelegate)typeof(IHtmlGenerator).Assembly
                    .GetType("Microsoft.AspNetCore.Mvc.ViewFeatures.NameAndIdProvider", throwOnError: true)!
                    .GetMethod("GetFullHtmlFieldName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
                    .CreateDelegate(typeof(GetFullHtmlFieldNameDelegate));

            var model = aspFor.Model is UploadEvidenceViewModel uploadEvidence ? uploadEvidence : new UploadEvidenceViewModel();
            var prefix = s_getFullHtmlFieldNameDelegate(ViewContext, aspFor.Name);
            this.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = prefix;

            if (HttpContext.Request.Method == "GET")
            {
                model.UploadedEvidenceFileUrl = model.EvidenceFileId is null ? null :
                    await fileService.GetFileUrlAsync(model.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
            }
            else if (HttpContext.Request.Method == "POST")
            {
            }

            return View(model);
        }
    }
}
