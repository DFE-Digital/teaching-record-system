using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

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
                    Policy = AuthorizationPolicies.InductionReadWrite
                });

                model.Filters.Add(new CheckPersonExistsFilterFactory());
            });
    }
}
