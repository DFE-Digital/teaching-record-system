using System.Runtime.CompilerServices;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Person
{
    public required Guid PersonId { get; init; }
    public required DateTime? CreatedOn { get; init; }
    public required DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public required string? Trn { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }  // A few DQT records in prod have a null DOB
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public InductionStatus? InductionStatus { get; private set; }  // Nullable until migration has populated the value for all records
    public InductionExemptionReason? InductionExemptionReason { get; private set; }
    public DateOnly? InductionStartDate { get; private set; }
    public DateOnly? InductionCompletedDate { get; private set; }
    public ICollection<Qualification> Qualifications { get; } = new List<Qualification>();
    public ICollection<Alert> Alerts { get; } = new List<Alert>();
    public ICollection<Route> Routes { get; } = new List<Route>();

    public Guid? DqtContactId { get; init; }
    public DateTime? DqtFirstSync { get; set; }
    public DateTime? DqtLastSync { get; set; }
    public int? DqtState { get; set; }
    public DateTime? DqtCreatedOn { get; set; }
    public DateTime? DqtModifiedOn { get; set; }
    public string? DqtFirstName { get; set; }
    public string? DqtMiddleName { get; set; }
    public string? DqtLastName { get; set; }

    public void SetInductionStatus(InductionStatus status, DateOnly? startDate, DateOnly? completedDate, InductionExemptionReason? exemptionReason)
    {
        if (status is Core.Models.InductionStatus.None or Core.Models.InductionStatus.RequiredToComplete)
        {
            EnsureArgumentIsNull(startDate);
            EnsureArgumentIsNull(completedDate);
            EnsureArgumentIsNull(exemptionReason);
        }
        else if (status is Core.Models.InductionStatus.Exempt)
        {
            EnsureArgumentIsNull(startDate);
            EnsureArgumentIsNull(completedDate);
            EnsureArgumentIsNotNull(exemptionReason);
        }
        else if (status is Core.Models.InductionStatus.InProgress)
        {
            EnsureArgumentIsNotNull(startDate);
            EnsureArgumentIsNull(completedDate);
        }
        else if (status is Core.Models.InductionStatus.Passed)
        {
            EnsureArgumentIsNotNull(startDate);
            EnsureArgumentIsNotNull(completedDate);
        }
        else
        {
            throw new ArgumentException($"Unknown status: '{status}'.", nameof(status));
        }

        InductionStatus = status;
        InductionStartDate = startDate;
        InductionCompletedDate = completedDate;
        InductionExemptionReason = exemptionReason;

        void EnsureArgumentIsNull(object? arg, [CallerArgumentExpression(nameof(arg))] string? paramName = "")
        {
            if (arg is not null)
            {
                throw new ArgumentException($"{paramName} must be null when the status is '{status}'.", paramName);
            }
        }

        void EnsureArgumentIsNotNull(object? arg, [CallerArgumentExpression(nameof(arg))] string? paramName = "")
        {
            if (arg is null)
            {
                throw new ArgumentException($"{paramName} cannot be null when the status is '{status}'.", paramName);
            }
        }
    }
}
