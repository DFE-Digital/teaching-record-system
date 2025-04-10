using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                // Check the user has the ability to write alerts.
                // The AlertType page will deal with ensuring that only permitted alert types can be selected.
                model.EndpointMetadata.Add(new AuthorizeAttribute()
                {
                    Policy = AuthorizationPolicies.AlertWrite
                });

                model.Filters.Add(new CheckPersonExistsFilterFactory());
            });
    }
}
