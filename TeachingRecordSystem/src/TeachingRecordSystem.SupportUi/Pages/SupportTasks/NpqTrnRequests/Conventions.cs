using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                var relativePath = model.RelativePath;

                // Exclude this specific file
                if (relativePath.EndsWith("Index.cshtml", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                model.Filters.Add(new CheckSupportTaskExistsFilterFactory(openOnly: true, SupportTaskType.NpqTrnRequest));

            });
    }
}
