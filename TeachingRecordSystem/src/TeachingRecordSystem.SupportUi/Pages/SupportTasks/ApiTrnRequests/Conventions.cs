using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                // The Index page shows a list rather than a single task
                if (model.RelativePath.EndsWith("/Index.cshtml"))
                {
                    return;
                }

                model.Filters.Add(new CheckSupportTaskExistsFilterFactory(openOnly: true, supportTaskType: SupportTaskType.ApiTrnRequest));
            });
    }
}
