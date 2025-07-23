using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                model.Filters.Add(new CheckSupportTaskExistsFilterFactory(openOnly: true, supportTaskType: SupportTaskType.TrnRequestManualChecksNeeded));
            });
    }
}
