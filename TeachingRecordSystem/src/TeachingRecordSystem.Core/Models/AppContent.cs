namespace TeachingRecordSystem.Core.Models;

public record AppContent
{
    public string? OneLoginCannotFindRecordEmailTemplateId { get; init; }
    public string? OneLoginNoMatchesPageContentHtml { get; init; }
    public string? OneLoginNoMatchesEmailSentFlashMessage { get; init; }
    public string? OneLoginFoundPageLinkText { get; init; }
}
