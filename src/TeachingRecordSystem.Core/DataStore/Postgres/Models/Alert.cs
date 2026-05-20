using EntityFrameworkCore.Projectables;
using FluentValidation;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Alert
{
    public const string AlertTypeIdIndexName = "ix_alerts_alert_type_id";
    public const string AlertTypeForeignKeyName = "fk_alerts_alert_type";
    public const string PersonIdIndexName = "ix_alerts_person_id";
    public const string PersonForeignKeyName = "fk_alerts_person";

    public const int DetailsMaxLength = 4000;

    public required Guid AlertId { get; init; }
    public AlertType? AlertType { get; }
    public required Guid AlertTypeId { get; init; }
    public required Guid PersonId { get; init; }
    public Person? Person { get; }
    public required string? Details { get; set; }
    public required string? ExternalLink { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    [Projectable] public bool IsOpen => EndDate == null;
}

public static class AlertValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> AlertDetails<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        Func<int, string> maxLengthMessage)
    {
        return ruleBuilder
            .MaximumLength(Alert.DetailsMaxLength).WithMessage(maxLengthMessage(Alert.DetailsMaxLength));
    }

    public static IRuleBuilderOptions<T, string?> AlertLink<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string invalidUrlMessage)
    {
        return ruleBuilder
            .Must(link => TrsUriHelper.TryCreateWebsiteUri(link, out _)).WithMessage(invalidUrlMessage);
    }

    public static IRuleBuilderOptions<T, DateOnly?> AlertStartDate<T>(
        this IRuleBuilder<T, DateOnly?> ruleBuilder,
        DateOnly today,
        string requiredMessage,
        string dateInFutureMessage)
    {
        return ruleBuilder
            .NotNull().WithMessage(requiredMessage)
            .LessThanOrEqualTo(today).WithMessage(dateInFutureMessage);
    }

    public static IRuleBuilderOptions<T, Guid?> AlertType<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder,
        string requiredMessage)
    {
        return ruleBuilder
            .NotNull().WithMessage(requiredMessage);
    }
}
