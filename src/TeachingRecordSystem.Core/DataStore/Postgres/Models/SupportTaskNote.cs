using FluentValidation;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTaskNote
{
    public const int ContentMaxLength = 4000;

    public required Guid SupportTaskNoteId { get; init; }
    public required string SupportTaskReference { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public User? CreatedBy { get; }
}

public static class SupportTaskNoteValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> SupportTaskNoteContent<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string notEmptyMessage,
        Func<int, string> maxLengthMessage)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(notEmptyMessage)
            .MaximumLength(SupportTaskNote.ContentMaxLength).WithMessage(maxLengthMessage(SupportTaskNote.ContentMaxLength));
    }
}
