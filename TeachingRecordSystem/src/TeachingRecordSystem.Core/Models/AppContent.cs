namespace TeachingRecordSystem.Core.Models;

public record AppContent
{
    public string? OneLoginCannotFindRecordEmailTemplateId { get; init; }
    public string? OneLoginNotVerifiedEmailTemplateId { get; init; }
    public string? OneLoginRecordMatchedEmailTemplateId { get; init; }
    public string? OneLoginNotConnectedEmailTemplateId { get; init; }
    public string? OneLoginNoMatchesPageContentHtml { get; init; }
    public string? OneLoginNoMatchesEmailSentFlashMessage { get; init; }
    public string? OneLoginNotConnectedEmailSentFlashMessage { get; init; }
    public string? OneLoginFoundPageLinkText { get; init; }
    public string? SignInUrl { get; init; }
}
