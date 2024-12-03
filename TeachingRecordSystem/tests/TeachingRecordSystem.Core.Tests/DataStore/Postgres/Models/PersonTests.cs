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
            (InductionExemptionReasons)1,
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
            (InductionExemptionReasons)1,
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
}