using FluentValidation;
using Microsoft.AspNetCore.Http;
using MimeDetective;

namespace TeachingRecordSystem.WebCommon.Validation;

public static class FileValidationExtensions
{
    private static readonly IContentInspector _inspector =
        new ContentInspectorBuilder
        {
            Definitions = MimeDetective.Definitions.DefaultDefinitions.All()
        }.Build();

    public static IRuleBuilderOptions<T, IFormFile?> PermittedFileType<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        IReadOnlyCollection<string> permittedMimeTypes)
    {
        return ruleBuilder.MustAsync(async (_, file, ctx, _) =>
        {
            if (file is null)
            {
                return true;
            }

            await using var stream = file.OpenReadStream();
            var matchedMimeTypes = _inspector.Inspect(stream).ByMimeType();

            var mimeType = matchedMimeTypes.FirstOrDefault(mt => permittedMimeTypes.Contains(mt.MimeType))?.MimeType;

            if (mimeType is not null)
            {
                ctx.RootContextData.Add(ValidationContextKeys.MimeTypeKey, mimeType);
            }

            return mimeType is not null;
        });
    }

    public static IRuleBuilderOptions<T, IFormFile?> MaxFileSize<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        int maxFileSizeMb)
    {
        return ruleBuilder.Must(file => file is null || file.Length <= maxFileSizeMb * 1024 * 1024);
    }

    public static string GetMimeType(this IValidationContext context)
    {
        return context.RootContextData.TryGetValue(ValidationContextKeys.MimeTypeKey, out var value) ?
            (string)value :
            throw new InvalidOperationException("MIME type is not set.");
    }
}
