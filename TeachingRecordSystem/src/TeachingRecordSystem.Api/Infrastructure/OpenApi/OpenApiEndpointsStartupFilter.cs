using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class OpenApiEndpointsStartupFilter : IStartupFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<GetAnIdentityOptions> _identityOptionsAccessor;

    public OpenApiEndpointsStartupFilter(IWebHostEnvironment environment, IOptions<GetAnIdentityOptions> identityOptionsAccessor)
    {
        _environment = environment;
        _identityOptionsAccessor = identityOptionsAccessor;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        foreach (var version in Api.Constants.Versions)
        {
            app.UseOpenApi(settings =>
            {
                settings.DocumentName = OpenApiDocumentHelper.GetDocumentName(version);
                settings.Path = OpenApiDocumentHelper.GetDocumentPath(version);

                settings.PostProcess = (document, request) =>
                {
                    document.Host = null;
                    document.Generator = null;
                    document.Servers.Clear();
                };
            });
        }

        app.UseSwaggerUi3(settings =>
        {
            foreach (var version in Api.Constants.Versions)
            {
                settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUi3Route($"v{version}", OpenApiDocumentHelper.GetDocumentPath(version)));
            }

            settings.PersistAuthorization = true;

            if (_environment.IsDevelopment())
            {
                // We don't want to expose our client secret (we're a confidential client)

                settings.OAuth2Client = new NSwag.AspNetCore.OAuth2ClientSettings()
                {
                    ClientId = _identityOptionsAccessor.Value.ClientId,
                    ClientSecret = _identityOptionsAccessor.Value.ClientSecret,
                    UsePkceWithAuthorizationCodeGrant = true
                };

                settings.CustomJavaScriptPath = "/docs/swagger-custom.js";
            }
        });

        if (_environment.IsDevelopment())
        {
            // Needed to use custom Javascript for Swagger UI
            app.UseStaticFiles();
        }
    };
}
