using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

public class Conventions : IConfigureFolderConventions
{
    public void Configure(RazorPagesOptions options)
    {
        options.Conventions.AddFolderApplicationModelConvention(
            this.GetFolderPathFromNamespace(),
            model =>
            {
                model.EndpointMetadata.Add(new AuthorizeAttribute()
                {
                    Policy = AuthorizationPolicies.NonPersonOrAlertDataEdit
                });
            });
    }
}
