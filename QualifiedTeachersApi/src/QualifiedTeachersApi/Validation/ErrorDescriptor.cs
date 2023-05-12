using QualifiedTeachersApi.Properties;

namespace QualifiedTeachersApi.Validation;

public class ErrorDescriptor
{
    private ErrorDescriptor(int errorCode, string title, string? detail)
    {
        ErrorCode = errorCode;
        Title = title;
        Detail = detail;
    }

    public int ErrorCode { get; }
    public string Title { get; }
    public string? Detail { get; }

    public static ErrorDescriptor Create(int errorCode)
    {
        var title = StringResources.ResourceManager.GetString($"Errors.{errorCode}.Title") ??
                    throw new ArgumentException($"No title defined for error {errorCode}.");

        var detail = StringResources.ResourceManager.GetString($"Errors.{errorCode}.Detail");

        return new ErrorDescriptor(errorCode, title, detail);
    }
}
