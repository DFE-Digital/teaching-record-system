using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                // Check the user has either the AlertsReadWrite or DbsAlertsReadWrite role.
                // The AlertType page will deal with ensuring that only permitted alert types can be selected.
                model.EndpointMetadata.Add(new AuthorizeAttribute()
                {
                    Roles = $"{UserRoles.Administrator},{UserRoles.AlertsReadWrite},{UserRoles.DbsAlertsReadWrite}"
                });

                model.Filters.Add(new CheckPersonExistsFilterFactory());
            });
    }
}
