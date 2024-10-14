using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.SupportUi.Pages;

public interface IConfigureFolderConventions : IConfigureOptions<RazorPagesOptions>
{
}

public static class ConfigureFolderConventionsExtensions
{
    public static string GetFolderPathFromNamespace(this IConfigureFolderConventions conventions)
    {
        var rootNamespace = typeof(ConfigureFolderConventionsExtensions).Namespace!;
        var conventionsNamespace = conventions.GetType().Namespace!;
        return conventionsNamespace.Substring(rootNamespace.Length).Replace(".", "/");
    }
}
