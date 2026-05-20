using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                model.Filters.Add(new CheckAlertExistsFilterFactory(requiredPermission: Permissions.Alerts.Write));
                model.Filters.Add(new ServiceFilterAttribute<RequireClosedAlertFilter>());
            });
    }
}
