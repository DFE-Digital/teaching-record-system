using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres.Models;

public class PersonTests
{
    public TestableClock Clock { get; } = new();

    [Fact]
    public void SetInductionStatus_None_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.None,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.None, person.InductionStatus);
    }

    [Fact]
    public void SetInductionStatus_RequiredToComplete_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.RequiredToComplete,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.RequiredToComplete, person.InductionStatus);
    }

    [Fact]
    public void SetInductionStatus_InProgress_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.InProgress,
            startDate: new(2024, 1, 1),
            completedDate: null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.InProgress, person.InductionStatus);
    }

    [Fact]
    public void SetInductionStatus_Passed_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.Passed,
            startDate: new(2024, 1, 1),
            completedDate: new(2025, 1, 1),
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Passed, person.InductionStatus);
    }

    [Fact]
    public void SetInductionStatus_Failed_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.Failed,
            startDate: new(2024, 1, 1),
            completedDate: new(2025, 1, 1),
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Failed, person.InductionStatus);
    }

    [Fact]
    public void SetInductionStatus_Exempt_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.Exempt,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: [InductionExemptionReason.QtlsId],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
    }

    [Fact]
    public void SetInductionStatus_FailedInWales_UpdatesStatus()
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetInductionStatus(
            InductionStatus.FailedInWales,
            startDate: new(2024, 1, 1),
            completedDate: new(2025, 1, 1),
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.FailedInWales, person.InductionStatus);
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public void SetCpdInductionStatus_SetsOverallStatusAndOutsEvent(InductionStatus status)
    {
        // Arrange
        var person = CreatePerson();

        // Act
        person.SetCpdInductionStatus(
            status,
            startDate: status != InductionStatus.RequiredToComplete ? new(2024, 1, 1) : null,
            completedDate: status == InductionStatus.Passed ? new(2024, 10, 1) : null,
            cpdModifiedOn: Clock.UtcNow,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(status, person.InductionStatus);
        Assert.Equal(status, person.InductionStatusWithoutExemption);
        Assert.Equal(Clock.UtcNow, person.InductionModifiedOn);
        Assert.Equal(Clock.UtcNow, person.CpdInductionModifiedOn);
    }

    [Fact]
    public void SetCpdInductionStatus_PersonIsExemptAndNewStatusIsPassed_SetsOverallStatusToPassed()
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            InductionStatus.Exempt,
            startDate: null,
            completedDate: null,
            [InductionExemptionReason.PassedInWalesId],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);
        Debug.Assert(person.InductionStatus == InductionStatus.Exempt);

        Clock.Advance();

        // Act
        person.SetCpdInductionStatus(
            InductionStatus.Passed,
            startDate: new(2024, 1, 1),
            completedDate: new(2024, 10, 1),
            cpdModifiedOn: Clock.UtcNow,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Passed, person.InductionStatus);
        Assert.Equal(InductionStatus.Passed, person.InductionStatusWithoutExemption);
        Assert.Equal(Clock.UtcNow, person.InductionModifiedOn);
        Assert.Equal(Clock.UtcNow, person.CpdInductionModifiedOn);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public void SetCpdInductionStatus_PersonIsExemptAndNewStatusIsNotPassed_KeepsOverallStatusAsExempt(InductionStatus status)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            InductionStatus.Exempt,
            startDate: null,
            completedDate: null,
            [InductionExemptionReason.PassedInWalesId],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);
        Debug.Assert(person.InductionStatus == InductionStatus.Exempt);

        Clock.Advance();

        // Act
        person.SetCpdInductionStatus(
            status,
            startDate: status == InductionStatus.InProgress ? new(2024, 1, 1) : null,
            completedDate: null,
            cpdModifiedOn: Clock.UtcNow,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
        Assert.Equal(status, person.InductionStatusWithoutExemption);
        Assert.Equal(Clock.UtcNow, person.InductionModifiedOn);
        Assert.Equal(Clock.UtcNow, person.CpdInductionModifiedOn);
    }

    [Theory]
    [InlineData(true, InductionStatus.Exempt)]
    [InlineData(true, InductionStatus.Passed)]
    [InlineData(true, InductionStatus.Failed)]
    [InlineData(false, InductionStatus.InProgress)]
    [InlineData(false, InductionStatus.Passed)]
    [InlineData(false, InductionStatus.Failed)]
    public void TrySetWelshInductionStatus_StatusIsAlreadySetToHigherPriorityStatus_DoesNotChangeStatus(bool passed, InductionStatus currentStatus)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            currentStatus,
            startDate: currentStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new(2024, 10, 1) : null,
            exemptionReasonIds: currentStatus is InductionStatus.Exempt ? new[] { InductionExemptionReason.QtlsId } : Array.Empty<Guid>(),
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        Clock.Advance();

        // Act
        person.TrySetWelshInductionStatus(
            passed,
            startDate: !passed ? new(2024, 1, 1) : null,
            completedDate: !passed ? new(2024, 10, 1) : null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(currentStatus, person.InductionStatus);
    }

    [Theory]
    [InlineData(InductionStatus.RequiredToComplete)]
    [InlineData(InductionStatus.InProgress)]
    public void TrySetWelshInductionStatus_PassedAndStatusIsAtLowerPriorityStatus_UpdatesStatusAndReturnsTrue(InductionStatus currentStatus)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            currentStatus,
            startDate: currentStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new(2024, 10, 1) : null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        Clock.Advance();

        // Act
        person.TrySetWelshInductionStatus(
            passed: true,
            startDate: null,
            completedDate: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
        Assert.Collection(person.InductionExemptionReasonIds, id => Assert.Equal(InductionExemptionReason.PassedInWalesId, id));
    }

    [Fact]
    public void TrySetWelshInductionStatus_FailedAndStatusIsAtLowerPriorityStatus_UpdatesStatusAndReturnsTrue()
    {
        // Arrange
        var person = CreatePerson();
        var currentStatus = InductionStatus.RequiredToComplete;

        person.SetInductionStatus(
            currentStatus,
            startDate: currentStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new(2024, 10, 1) : null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        Clock.Advance();

        // Act
        person.TrySetWelshInductionStatus(
            passed: false,
            startDate: new(2024, 1, 1),
            completedDate: new(2024, 10, 1),
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.FailedInWales, person.InductionStatus);
        Assert.Empty(person.InductionExemptionReasonIds);
    }

    [Fact]
    public void InductionManagedByCpd_CpdStatusIsNotNullWithNoCompletedDate_ReturnsTrue()
    {
        // Arrange
        var person = CreatePerson();

        person.SetCpdInductionStatus(
            status: InductionStatus.InProgress,
            startDate: new(2024, 1, 1),
            completedDate: null,
            cpdModifiedOn: Clock.UtcNow,
            SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Act
        var result = person.InductionStatusManagedByCpd(Clock.Today);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InductionManagedByCpd_CpdStatusIsNotNullCompletedDateWithin7Years_ReturnsTrue()
    {
        // Arrange
        var person = CreatePerson();

        person.SetCpdInductionStatus(
            status: InductionStatus.Passed,
            startDate: Clock.Today.AddYears(-1),
            completedDate: Clock.Today,
            cpdModifiedOn: Clock.UtcNow,
            SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Act
        var result = person.InductionStatusManagedByCpd(Clock.Today);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InductionManagedByCpd_CpdStatusIsNotNullCompletedDateMoreThan7YearsAgo_ReturnsFalse()
    {
        // Arrange
        var person = CreatePerson();

        person.SetCpdInductionStatus(
            status: InductionStatus.Passed,
            startDate: Clock.Today.AddYears(-9),
            completedDate: Clock.Today.AddYears(-7),
            cpdModifiedOn: Clock.UtcNow,
            SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Act
        var result = person.InductionStatusManagedByCpd(Clock.Today);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InductionManagedByCpd_CpdStatusIsNull_ReturnsFalse()
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            status: InductionStatus.InProgress,
            startDate: Clock.Today,
            completedDate: null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Act
        var result = person.InductionStatusManagedByCpd(Clock.Today);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(InductionStatus.None)]
    [InlineData(InductionStatus.RequiredToComplete)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.FailedInWales)]
    public void AddInductionExemptionReason_CurrentStatusIsLowerPriorityThanExempt_UpdatesStatus(InductionStatus currentStatus)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            currentStatus,
            startDate: currentStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new(2025, 1, 1) : null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Act
        person.AddInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
    }

    [Theory]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.Passed)]
    public void AddInductionExemptionReason_CurrentStatusIsHigherPriorityThanExempt_DoesNotChangeStatus(InductionStatus currentStatus)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            currentStatus,
            startDate: currentStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new(2025, 1, 1) : null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Act
        person.AddInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(currentStatus, person.InductionStatus);
    }

    [Theory]
    [InlineData(InductionStatus.None)]
    [InlineData(InductionStatus.RequiredToComplete)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.FailedInWales)]
    public void RemoveInductionExemptionReason_StatusIsExemptWithNoOtherReasons_RollsBackStatus(InductionStatus initialStatus)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            initialStatus,
            startDate: initialStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: initialStatus.RequiresCompletedDate() ? new(2025, 1, 1) : null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        person.AddInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        Debug.Assert(person.InductionStatus == InductionStatus.Exempt);

        // Act
        person.RemoveInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(initialStatus, person.InductionStatus);
    }

    [Theory]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.Passed)]
    public void RemoveInductionExemptionReason_StatusIsHigherPriorityToExempt_DoesNotChangeStatus(InductionStatus currentStatus)
    {
        // Arrange
        var person = CreatePerson();

        person.SetInductionStatus(
            currentStatus,
            startDate: new(2024, 1, 1),
            completedDate: null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        person.AddInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        Debug.Assert(person.InductionStatus == currentStatus);

        // Act
        person.RemoveInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(currentStatus, person.InductionStatus);
    }

    private Person CreatePerson() => new Person
    {
        PersonId = Guid.NewGuid(),
        CreatedOn = Clock.UtcNow,
        UpdatedOn = Clock.UtcNow,
        Trn = "1234567",
        FirstName = "Joe",
        MiddleName = "",
        LastName = "Bloggs",
        DateOfBirth = new(1990, 1, 1),
    };
}
