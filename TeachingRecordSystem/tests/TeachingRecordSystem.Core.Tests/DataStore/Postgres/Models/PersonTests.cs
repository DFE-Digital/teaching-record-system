using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres.Models;

public class PersonTests
{
    public TestableClock Clock { get; } = new();

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public void SetCpdInductionStatus_SetsOverallStatusAndOutsEvent(InductionStatus status)
    {
        // Arrange
        var person = new Person
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
        Assert.Equal(Clock.UtcNow, person.InductionModifiedOn);
        Assert.Equal(Clock.UtcNow, person.CpdInductionModifiedOn);
    }

    [Fact]
    public void SetCpdInductionStatus_PersonIsExemptAndNewStatusIsPassed_SetsOverallStatusToPassed()
    {
        // Arrange
        var person = new Person
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
        Assert.Equal(Clock.UtcNow, person.InductionModifiedOn);
        Assert.Equal(Clock.UtcNow, person.CpdInductionModifiedOn);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public void SetCpdInductionStatus_PersonIsExemptAndNewStatusIsNotPassed_KeepsOverallStatusAsExempt(InductionStatus status)
    {
        // Arrange
        var person = new Person
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
        Assert.NotEqual(Clock.UtcNow, person.InductionModifiedOn);
        Assert.Equal(Clock.UtcNow, person.CpdInductionModifiedOn);
    }

    [Theory]
    [InlineData(true, InductionStatus.Exempt)]
    [InlineData(true, InductionStatus.InProgress)]
    [InlineData(true, InductionStatus.Passed)]
    [InlineData(true, InductionStatus.Failed)]
    [InlineData(false, InductionStatus.Exempt)]
    [InlineData(false, InductionStatus.InProgress)]
    [InlineData(false, InductionStatus.Passed)]
    [InlineData(false, InductionStatus.Failed)]
    public void TrySetWelshInductionStatus_StatusIsAlreadySetToHigherPriorityStatus_ReturnsFalse(bool passed, InductionStatus currentStatus)
    {
        // Arrange
        var person = new Person
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

        person.SetInductionStatus(
            currentStatus,
            startDate: currentStatus.RequiresStartDate() ? new(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new(2024, 10, 1) : null,
            exemptionReasonIds: currentStatus is InductionStatus.Exempt ? new[] { InductionExemptionReason.PassedInWalesId } : Array.Empty<Guid>(),
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        Clock.Advance();

        // Act
        var result = person.TrySetWelshInductionStatus(
            passed,
            startDate: !passed ? new(2024, 1, 1) : null,
            completedDate: !passed ? new(2024, 10, 1) : null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.False(result);
        Assert.Equal(currentStatus, person.InductionStatus);
    }

    [Fact]
    public void TrySetWelshInductionStatus_PassedAndStatusIsAtLowerPriorityStatus_UpdatesStatusAndReturnsTrue()
    {
        // Arrange
        var person = new Person
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
        var result = person.TrySetWelshInductionStatus(
            passed: true,
            startDate: null,
            completedDate: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.True(result);
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
        Assert.Collection(person.InductionExemptionReasonIds, id => Assert.Equal(InductionExemptionReason.PassedInWalesId, id));
    }

    [Fact]
    public void TrySetWelshInductionStatus_FailedAndStatusIsAtLowerPriorityStatus_UpdatesStatusAndReturnsTrue()
    {
        // Arrange
        var person = new Person
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
        var result = person.TrySetWelshInductionStatus(
            passed: false,
            startDate: new(2024, 1, 1),
            completedDate: new(2024, 10, 1),
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out _);

        // Assert
        Assert.True(result);
        Assert.Equal(InductionStatus.FailedInWales, person.InductionStatus);
        Assert.Empty(person.InductionExemptionReasonIds);
    }

    [Theory]
    [InlineData(-3, true)]
    [InlineData(-7, false)]
    public void InductionManagedByCpd_ReturnsExpected(int yearsSinceCompleted, bool expected)
    {
        // Arrange
        var dateTimeCompleted = Clock.UtcNow.AddYears(yearsSinceCompleted).AddDays(-1);
        var dateCompleted = Clock.Today.AddYears(yearsSinceCompleted).AddDays(-1);
        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            CreatedOn = dateTimeCompleted,
            UpdatedOn = dateTimeCompleted,
            Trn = "1234567",
            FirstName = "Joe",
            MiddleName = "",
            LastName = "Bloggs",
            DateOfBirth = new(1990, 1, 1)
        };
        person.SetCpdInductionStatus(
            status: InductionStatus.Passed,
            startDate: dateCompleted,
            completedDate: dateCompleted,
            cpdModifiedOn: dateTimeCompleted,
            SystemUser.SystemUserId,
            Clock.UtcNow, out _);

        // Act
        var result = person.InductionStatusManagedByCpd(Clock.Today);

        // Assert
        Assert.Equal(expected, result);
    }
}
