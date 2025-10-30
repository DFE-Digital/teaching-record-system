using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres.Models;

public class PersonTests
{
    public TestableClock Clock { get; } = new TestableClock();

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
            startDate: new DateOnly(2024, 1, 1),
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
            startDate: new DateOnly(2024, 1, 1),
            completedDate: new DateOnly(2025, 1, 1),
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
            startDate: new DateOnly(2024, 1, 1),
            completedDate: new DateOnly(2025, 1, 1),
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
            startDate: new DateOnly(2024, 1, 1),
            completedDate: new DateOnly(2025, 1, 1),
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
            startDate: status != InductionStatus.RequiredToComplete ? new DateOnly(2024, 1, 1) : null,
            completedDate: status == InductionStatus.Passed ? new DateOnly(2024, 10, 1) : null,
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
            startDate: new DateOnly(2024, 1, 1),
            completedDate: new DateOnly(2024, 10, 1),
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
            startDate: status == InductionStatus.InProgress ? new DateOnly(2024, 1, 1) : null,
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
            startDate: currentStatus.RequiresStartDate() ? new DateOnly(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new DateOnly(2024, 10, 1) : null,
            exemptionReasonIds: currentStatus is InductionStatus.Exempt ?
                new[] { InductionExemptionReason.QtlsId } :
                Array.Empty<Guid>(),
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
            startDate: !passed ? new DateOnly(2024, 1, 1) : null,
            completedDate: !passed ? new DateOnly(2024, 10, 1) : null,
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
            startDate: currentStatus.RequiresStartDate() ? new DateOnly(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new DateOnly(2024, 10, 1) : null,
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
            startDate: currentStatus.RequiresStartDate() ? new DateOnly(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new DateOnly(2024, 10, 1) : null,
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
            startDate: new DateOnly(2024, 1, 1),
            completedDate: new DateOnly(2024, 10, 1),
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
            startDate: new DateOnly(2024, 1, 1),
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
            startDate: currentStatus.RequiresStartDate() ? new DateOnly(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new DateOnly(2025, 1, 1) : null,
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
            startDate: currentStatus.RequiresStartDate() ? new DateOnly(2024, 1, 1) : null,
            completedDate: currentStatus.RequiresCompletedDate() ? new DateOnly(2025, 1, 1) : null,
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
            startDate: initialStatus.RequiresStartDate() ? new DateOnly(2024, 1, 1) : null,
            completedDate: initialStatus.RequiresCompletedDate() ? new DateOnly(2025, 1, 1) : null,
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
            startDate: new DateOnly(2024, 1, 1),
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

    [Fact]
    public void RemoveInductionExemptionReason_PersonIsAlsoExemptWithoutReason_DoesNotChangeStatus()
    {
        // Arrange
        var person = CreatePerson();

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

        person.InductionExemptWithoutReason = true;

        // Act
        person.RemoveInductionExemptionReason(
            InductionExemptionReason.QtlsId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
    }

    [Fact]
    public void RemoveInductionExemptionReason_PersonIsAlsoExemptThroughRoute_DoesNotChangeStatus()
    {
        // Arrange
        var exemptionReasonId = Guid.NewGuid();

        var person = CreatePersonWithInductionStatus(
            InductionStatus.Exempt,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [exemptionReasonId]);

        person.Qualifications!.Add(CreateAwardedProfessionalStatus(person, exemptFromInduction: true));

        // Act
        person.RemoveInductionExemptionReason(
            exemptionReasonId,
            updatedBy: SystemUser.SystemUserId,
            now: Clock.UtcNow,
            out _);

        // Assert
        Assert.Equal(InductionStatus.Exempt, person.InductionStatus);
    }

    [Theory]
    [MemberData(nameof(GetRefreshProfessionalStatusAttributesData))]
    public void RefreshProfessionalStatusAttributes(RefreshProfessionalStatusAttributesTestCaseData testCaseData)
    {
        // Arrange
        var person = CreatePerson();

        var allRoutes = new List<RouteToProfessionalStatusType>();
        var professionalStatuses = new List<RouteToProfessionalStatus>();

        DateOnly? existingRouteHoldsFrom = Clock.Today.AddDays(-10);
        RouteToProfessionalStatusStatus existingRouteStatus =
            testCaseData.ExistingRouteIsHoldsStatus ? RouteToProfessionalStatusStatus.Holds : RouteToProfessionalStatusStatus.InTraining;
        DateOnly? newRouteHoldsFrom = testCaseData.NewHoldsFromIsAfterExistingHoldsFrom ? Clock.Today.AddDays(-1) : Clock.Today.AddDays(-20);
        RouteToProfessionalStatusStatus newRouteStatus =
            testCaseData.NewRouteIsHoldsStatus ? RouteToProfessionalStatusStatus.Holds : RouteToProfessionalStatusStatus.InTraining;

        if (testCaseData.HaveExistingRouteForStatus)
        {
            var anotherRoute = CreateRouteForProfessionalStatusType(testCaseData.ProfessionalStatusType);
            allRoutes.Add(anotherRoute);

            var existingProfessionalStatus = CreateProfessionalStatus(
                anotherRoute,
                status: existingRouteStatus,
                holdsFrom: testCaseData.ExistingRouteIsHoldsStatus && testCaseData.ProfessionalStatusType is not ProfessionalStatusType.EarlyYearsProfessionalStatus ? existingRouteHoldsFrom : null);
            professionalStatuses.Add(existingProfessionalStatus);

            if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus)
            {
                person.QtsDate = existingProfessionalStatus.HoldsFrom;
            }
            else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus)
            {
                person.EytsDate = existingProfessionalStatus.HoldsFrom;
            }
            else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus)
            {
                person.PqtsDate = existingProfessionalStatus.HoldsFrom;
            }
            else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus)
            {
                person.HasEyps = existingProfessionalStatus.Status is RouteToProfessionalStatusStatus.Holds;
            }
        }

        if (testCaseData.HaveNewRouteForStatus)
        {
            var route = CreateRouteForProfessionalStatusType(testCaseData.ProfessionalStatusType);
            allRoutes.Add(route);

            var newProfessionalStatus = CreateProfessionalStatus(
                route,
                status: newRouteStatus,
                holdsFrom: testCaseData.NewRouteIsHoldsStatus && testCaseData.ProfessionalStatusType is not ProfessionalStatusType.EarlyYearsProfessionalStatus ? newRouteHoldsFrom : null);
            professionalStatuses.Add(newProfessionalStatus);
        }

        // Act
        var result = person.RefreshProfessionalStatusAttributes(
            testCaseData.ProfessionalStatusType,
            allRoutes,
            professionalStatuses);

        // Assert
        Assert.Equal(testCaseData.ExpectedResult, result);

        if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus)
        {
            Assert.Equal(
                testCaseData.ExpectedAttributeValue switch
                {
                    ExpectedAttributeValue.NewRouteAwarded => newRouteHoldsFrom,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteHoldsFrom,
                    _ => null
                },
                person.QtsDate);
        }
        else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus)
        {
            Assert.Equal(
                testCaseData.ExpectedAttributeValue switch
                {
                    ExpectedAttributeValue.NewRouteAwarded => newRouteHoldsFrom,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteHoldsFrom,
                    _ => null
                },
                person.EytsDate);
        }
        else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus)
        {
            Assert.Equal(
                testCaseData.ExpectedAttributeValue switch
                {
                    ExpectedAttributeValue.NewRouteAwarded => newRouteStatus is RouteToProfessionalStatusStatus.Holds,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteStatus is RouteToProfessionalStatusStatus.Holds,
                    _ => false
                },
                person.HasEyps);
        }
        else
        {
            Debug.Assert(testCaseData.ProfessionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus);
            Assert.Equal(
                testCaseData.ExpectedAttributeValue switch
                {
                    ExpectedAttributeValue.NewRouteAwarded => newRouteHoldsFrom,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteHoldsFrom,
                    _ => null
                },
                person.PqtsDate);
        }

        static RouteToProfessionalStatusType CreateRouteForProfessionalStatusType(ProfessionalStatusType professionalStatusType) =>
            new RouteToProfessionalStatusType()
            {
                RouteToProfessionalStatusTypeId = Guid.NewGuid(),
                Name = $"A {professionalStatusType} route",
                ProfessionalStatusType = professionalStatusType,
                IsActive = true,
                TrainingStartDateRequired = FieldRequirement.Optional,
                TrainingEndDateRequired = FieldRequirement.Optional,
                HoldsFromRequired = FieldRequirement.Optional,
                InductionExemptionRequired = FieldRequirement.Optional,
                TrainingProviderRequired = FieldRequirement.Optional,
                DegreeTypeRequired = FieldRequirement.Optional,
                TrainingCountryRequired = FieldRequirement.Optional,
                TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
                TrainingSubjectsRequired = FieldRequirement.Optional,
                InductionExemptionReasonId = null
            };

        RouteToProfessionalStatus CreateProfessionalStatus(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, DateOnly? holdsFrom) =>
            new TestableRouteToProfessionalStatus()
            {
                QualificationId = Guid.NewGuid(),
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow,
                DeletedOn = null,
                PersonId = person.PersonId,
                RouteToProfessionalStatusTypeId = route.RouteToProfessionalStatusTypeId,
                RouteToProfessionalStatusType = route,
                SourceApplicationUserId = null,
                SourceApplicationReference = null,
                Status = status,
                HoldsFrom = holdsFrom,
                TrainingStartDate = null,
                TrainingEndDate = null,
                TrainingSubjectIds = [],
                TrainingAgeSpecialismType = null,
                TrainingAgeSpecialismRangeFrom = null,
                TrainingAgeSpecialismRangeTo = null,
                TrainingCountryId = null,
                TrainingProviderId = null,
                ExemptFromInduction = null,
                DegreeTypeId = null,
                DqtTeacherStatusName = null,
                DqtTeacherStatusValue = null,
                DqtEarlyYearsStatusName = null,
                DqtEarlyYearsStatusValue = null,
                DqtInitialTeacherTrainingId = null,
                DqtQtsRegistrationId = null
            };
    }

    [Fact]
    public void RefreshInductionStatusForQtsProfessionalStatusChanged_NoQtsRoutesAndNoPersonLevelExemptions_SetsInductionStatusToNone()
    {
        // Arrange
        var person = CreatePerson();
        Debug.Assert(person.InductionExemptionReasonIds.Length == 0);

        // Act
        person.RefreshInductionStatusForQtsProfessionalStatusChanged(
            Clock.UtcNow,
            []);

        // Assert
        Assert.Equal(InductionStatus.None, person.InductionStatus);
    }

    [Theory]
    [MemberData(nameof(GetGetRefreshInductionStatusForQtsProfessionalStatusChangedDataData))]
    public void RefreshInductionStatusForQtsProfessionalStatusChanged(
        Person person,
        IReadOnlyCollection<RouteToProfessionalStatus> newRoutes,
        InductionStatus expectedInductionStatus,
        InductionStatus expectedInductionStatusWithoutExemption)
    {
        // Arrange
        var allRouteTypes = newRoutes
            .Select(r => r.RouteToProfessionalStatusType!)
            .ToArray();

        // Act
        person.RefreshInductionStatusForQtsProfessionalStatusChanged(Clock.UtcNow, allRouteTypes, newRoutes);

        // Assert
        Assert.Equal(expectedInductionStatus, person.InductionStatus);
        Assert.Equal(expectedInductionStatusWithoutExemption, person.InductionStatusWithoutExemption);
    }

    private Person CreatePerson() => new TestablePerson()
    {
        PersonId = Guid.NewGuid(),
        Trn = "1234567",
        FirstName = "Joe",
        MiddleName = "",
        LastName = "Bloggs",
        DateOfBirth = new DateOnly(1990, 1, 1),
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow
    };

    public static TheoryData<RefreshProfessionalStatusAttributesTestCaseData> GetRefreshProfessionalStatusAttributesData()
    {
        var data = new TheoryData<RefreshProfessionalStatusAttributesTestCaseData>();

        var allProfessionalStatusTypes = new[]
        {
            ProfessionalStatusType.QualifiedTeacherStatus,
            ProfessionalStatusType.EarlyYearsTeacherStatus,
            ProfessionalStatusType.EarlyYearsProfessionalStatus,
            ProfessionalStatusType.PartialQualifiedTeacherStatus
        };

        foreach (var professionalStatusType in allProfessionalStatusTypes)
        {
            // No existing routes, no route added
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: false,
                ExistingRouteIsHoldsStatus: false,
                HaveNewRouteForStatus: false,
                NewRouteIsHoldsStatus: false,
                NewHoldsFromIsAfterExistingHoldsFrom: false,
                ExpectedAttributeValue: ExpectedAttributeValue.Null,
                ExpectedResult: false));

            // No existing routes, new route added but not holds status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: false,
                ExistingRouteIsHoldsStatus: false,
                HaveNewRouteForStatus: true,
                NewRouteIsHoldsStatus: false,
                NewHoldsFromIsAfterExistingHoldsFrom: false,
                ExpectedAttributeValue: ExpectedAttributeValue.Null,
                ExpectedResult: false));

            // Existing route but not holds, new route added but not holds status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: true,
                ExistingRouteIsHoldsStatus: false,
                HaveNewRouteForStatus: true,
                NewRouteIsHoldsStatus: false,
                NewHoldsFromIsAfterExistingHoldsFrom: false,
                ExpectedAttributeValue: ExpectedAttributeValue.Null,
                ExpectedResult: false));

            // Existing route but not holds, new route added at holds status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: true,
                ExistingRouteIsHoldsStatus: false,
                HaveNewRouteForStatus: true,
                NewRouteIsHoldsStatus: true,
                NewHoldsFromIsAfterExistingHoldsFrom: false,
                ExpectedAttributeValue: ExpectedAttributeValue.NewRouteAwarded,
                ExpectedResult: true));

            // Existing route at holds, new route added but not holds status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: true,
                ExistingRouteIsHoldsStatus: true,
                HaveNewRouteForStatus: true,
                NewRouteIsHoldsStatus: false,
                NewHoldsFromIsAfterExistingHoldsFrom: false,
                ExpectedAttributeValue: ExpectedAttributeValue.ExistingRouteAwarded,
                ExpectedResult: false));

            if (professionalStatusType is not ProfessionalStatusType.EarlyYearsProfessionalStatus)
            {
                // Existing route at holds with awarded date before new route, new route added at holds status
                data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                    professionalStatusType,
                    HaveExistingRouteForStatus: true,
                    ExistingRouteIsHoldsStatus: true,
                    HaveNewRouteForStatus: true,
                    NewRouteIsHoldsStatus: true,
                    NewHoldsFromIsAfterExistingHoldsFrom: true,
                    ExpectedAttributeValue: ExpectedAttributeValue.ExistingRouteAwarded,
                    ExpectedResult: false));

                // Existing route at holds with after date before new route, new route added at holds status
                data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                    professionalStatusType,
                    HaveExistingRouteForStatus: true,
                    ExistingRouteIsHoldsStatus: true,
                    HaveNewRouteForStatus: true,
                    NewRouteIsHoldsStatus: true,
                    NewHoldsFromIsAfterExistingHoldsFrom: false,
                    ExpectedAttributeValue: ExpectedAttributeValue.NewRouteAwarded,
                    ExpectedResult: true));
            }
        }

        return data;
    }

    public static TheoryData<Person, IReadOnlyCollection<RouteToProfessionalStatus>, InductionStatus, InductionStatus>
        GetGetRefreshInductionStatusForQtsProfessionalStatusChangedDataData()
    {
        var data = new TheoryData<Person, IReadOnlyCollection<RouteToProfessionalStatus>, InductionStatus, InductionStatus>();

        // Person with 'None' status then QTS route added but not-awarded should stay at 'None'
        WithPerson(
            InductionStatus.None,
            InductionStatus.None,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateNotAwardedProfessionalStatus(person)],
                InductionStatus.None,
                InductionStatus.None));

        // Person with 'None' status then QTS route added and Awarded goes to 'RequiredToComplete'
        WithPerson(
            InductionStatus.None,
            InductionStatus.None,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person)],
                InductionStatus.RequiredToComplete,
                InductionStatus.RequiredToComplete));

        // Person with 'RequiredToComplete' then QTS route removed goes to 'None'
        WithPerson(
            InductionStatus.RequiredToComplete,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [],
                InductionStatus.None,
                InductionStatus.None));

        // Person with 'RequiredToComplete' then route status amended from Awarded goes to 'None'
        WithPerson(
            InductionStatus.RequiredToComplete,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateNotAwardedProfessionalStatus(person)],
                InductionStatus.None,
                InductionStatus.None));

        // Person with 'None' then not-awarded route added with exemption stays at 'None'
        WithPerson(
            InductionStatus.None,
            InductionStatus.None,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateNotAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.None,
                InductionStatus.None));

        // Person with 'None' then awarded route with exemption added goes to 'Exempt'
        WithPerson(
            InductionStatus.None,
            InductionStatus.None,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Exempt,
                InductionStatus.RequiredToComplete));

        // Person with 'RequiredToComplete' then not-awarded route added with exemption stays 'RequiredToComplete'
        WithPerson(
            InductionStatus.RequiredToComplete,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateNotAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.RequiredToComplete,
                InductionStatus.RequiredToComplete));

        // Person with 'RequiredToComplete' then awarded route with exemption added goes to 'Exempt'
        WithPerson(
            InductionStatus.RequiredToComplete,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Exempt,
                InductionStatus.RequiredToComplete));

        // Person with 'InProgress' then not-awarded route with exemption stays 'RequiredToComplete'
        WithPerson(
            InductionStatus.InProgress,
            InductionStatus.InProgress,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateNotAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.InProgress,
                InductionStatus.InProgress));

        // Person with 'InProgress' then awarded route with exemption added goes to 'Exempt'
        WithPerson(
            InductionStatus.InProgress,
            InductionStatus.InProgress,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Exempt,
                InductionStatus.InProgress));

        // Person with 'Passed' then not-awarded route with exemption added stays 'Passed'
        WithPerson(
            InductionStatus.Passed,
            InductionStatus.Passed,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateNotAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Passed,
                InductionStatus.Passed));

        // Person with 'Passed' then awarded route with exemption added stays 'Passed'
        WithPerson(
            InductionStatus.Passed,
            InductionStatus.Passed,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Passed,
                InductionStatus.Passed));

        // Person with 'Failed' then not-awarded route with exemption added stays 'Failed'
        WithPerson(
            InductionStatus.Failed,
            InductionStatus.Failed,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateNotAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Failed,
                InductionStatus.Failed));

        // Person with 'Failed' then awarded route with exemption added stays 'Failed'
        WithPerson(
            InductionStatus.Failed,
            InductionStatus.Failed,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person), CreateAwardedProfessionalStatus(person, exemptFromInduction: true)],
                InductionStatus.Failed,
                InductionStatus.Failed));

        // Person with 'Exempt' (and person-level exemption) then QTS route added but not-awarded stays 'Exempt'
        WithPerson(
            InductionStatus.Exempt,
            InductionStatus.None,
            exemptionReasonIds: [Guid.NewGuid()],
            person => data.Add(
                person,
                [CreateNotAwardedProfessionalStatus(person)],
                InductionStatus.Exempt,
                InductionStatus.None));

        // Person with 'Exempt' (and person-level exemption) then awarded QTS route added stays 'Exempt'
        WithPerson(
            InductionStatus.Exempt,
            InductionStatus.None,
            exemptionReasonIds: [Guid.NewGuid()],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person)],
                InductionStatus.Exempt,
                InductionStatus.RequiredToComplete));

        // Person with 'Exempt' at person-level and route-level then QTS route removed stays 'Exempt'
        WithPerson(
            InductionStatus.Exempt,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [Guid.NewGuid()],
            person => data.Add(
                person,
                [],
                InductionStatus.Exempt,
                InductionStatus.None));

        // Person with 'Exempt' at route-level then route removed goes to 'None'
        WithPerson(
            InductionStatus.Exempt,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [],
                InductionStatus.None,
                InductionStatus.None));

        // Person with 'Exempt' at route-level with another awarded QTS route then route removed goes to 'RequiredToComplete'
        WithPerson(
            InductionStatus.Exempt,
            InductionStatus.RequiredToComplete,
            exemptionReasonIds: [],
            person => data.Add(
                person,
                [CreateAwardedProfessionalStatus(person, exemptFromInduction: false)],
                InductionStatus.RequiredToComplete,
                InductionStatus.RequiredToComplete));

        return data;

        static void WithPerson(
            InductionStatus status,
            InductionStatus statusWithoutExemption,
            Guid[] exemptionReasonIds,
            Action<Person> action)
        {
            var person = CreatePersonWithInductionStatus(status, statusWithoutExemption, exemptionReasonIds);
            action(person);
        }
    }

    private static RouteToProfessionalStatus CreateAwardedProfessionalStatus(Person person, bool exemptFromInduction = false)
    {
        var routeId = Guid.NewGuid();

        return new TestableRouteToProfessionalStatus()
        {
            RouteToProfessionalStatusTypeId = routeId,
            RouteToProfessionalStatusType = new RouteToProfessionalStatusType()
            {
                RouteToProfessionalStatusTypeId = routeId,
                Name = "Test route",
                ProfessionalStatusType = ProfessionalStatusType.QualifiedTeacherStatus,
                IsActive = true,
                TrainingStartDateRequired = FieldRequirement.Optional,
                TrainingEndDateRequired = FieldRequirement.Optional,
                HoldsFromRequired = FieldRequirement.Optional,
                InductionExemptionRequired = FieldRequirement.Optional,
                TrainingProviderRequired = FieldRequirement.Optional,
                DegreeTypeRequired = FieldRequirement.Optional,
                TrainingCountryRequired = FieldRequirement.Optional,
                TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
                TrainingSubjectsRequired = FieldRequirement.Optional,
                InductionExemptionReasonId = exemptFromInduction ? Guid.NewGuid() : null
            },
            SourceApplicationUserId = null,
            SourceApplicationReference = null,
            Status = RouteToProfessionalStatusStatus.Holds,
            HoldsFrom = new(2023, 1, 1),
            TrainingStartDate = null,
            TrainingEndDate = null,
            TrainingSubjectIds = [],
            TrainingAgeSpecialismType = null,
            TrainingAgeSpecialismRangeFrom = null,
            TrainingAgeSpecialismRangeTo = null,
            TrainingCountryId = null,
            TrainingProviderId = null,
            ExemptFromInduction = exemptFromInduction,
            DegreeTypeId = null,
            DqtTeacherStatusName = null,
            DqtTeacherStatusValue = null,
            DqtEarlyYearsStatusName = null,
            DqtEarlyYearsStatusValue = null,
            DqtInitialTeacherTrainingId = null,
            DqtQtsRegistrationId = null,
            QualificationId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            PersonId = person.PersonId
        };
    }

    private static RouteToProfessionalStatus CreateNotAwardedProfessionalStatus(Person person, bool exemptFromInduction = false)
    {
        var routeId = Guid.NewGuid();

        return new TestableRouteToProfessionalStatus()
        {
            RouteToProfessionalStatusTypeId = routeId,
            RouteToProfessionalStatusType = new RouteToProfessionalStatusType()
            {
                RouteToProfessionalStatusTypeId = routeId,
                Name = "Test route",
                ProfessionalStatusType = ProfessionalStatusType.QualifiedTeacherStatus,
                IsActive = true,
                TrainingStartDateRequired = FieldRequirement.Optional,
                TrainingEndDateRequired = FieldRequirement.Optional,
                HoldsFromRequired = FieldRequirement.Optional,
                InductionExemptionRequired = FieldRequirement.Optional,
                TrainingProviderRequired = FieldRequirement.Optional,
                DegreeTypeRequired = FieldRequirement.Optional,
                TrainingCountryRequired = FieldRequirement.Optional,
                TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
                TrainingSubjectsRequired = FieldRequirement.Optional,
                InductionExemptionReasonId = null
            },
            SourceApplicationUserId = null,
            SourceApplicationReference = null,
            Status = RouteToProfessionalStatusStatus.InTraining,
            HoldsFrom = null,
            TrainingStartDate = null,
            TrainingEndDate = null,
            TrainingSubjectIds = [],
            TrainingAgeSpecialismType = null,
            TrainingAgeSpecialismRangeFrom = null,
            TrainingAgeSpecialismRangeTo = null,
            TrainingCountryId = null,
            TrainingProviderId = null,
            ExemptFromInduction = exemptFromInduction,
            DegreeTypeId = null,
            DqtTeacherStatusName = null,
            DqtTeacherStatusValue = null,
            DqtEarlyYearsStatusName = null,
            DqtEarlyYearsStatusValue = null,
            DqtInitialTeacherTrainingId = null,
            DqtQtsRegistrationId = null,
            QualificationId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            PersonId = person.PersonId
        };
    }

    private static TestablePerson CreatePersonWithInductionStatus(
        InductionStatus status,
        InductionStatus statusWithoutExemption,
        Guid[] exemptionReasonIds)
    {
        var person = new TestablePerson()
        {
            PersonId = Guid.NewGuid(),
            CreatedOn = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Trn = "1234567",
            FirstName = "Joe",
            MiddleName = "",
            LastName = "Bloggs",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };

        person.UnsafeSetInductionStatus(
            status,
            statusWithoutExemption,
            startDate: status is InductionStatus.InProgress or InductionStatus.Passed or InductionStatus.Failed ? new(2024, 1, 1) : null,
            completedDate: status is InductionStatus.Passed ? new(2025, 1, 1) : null,
            exemptionReasonIds: status is InductionStatus.Exempt ? exemptionReasonIds : []);

        return person;
    }

    public enum ExpectedAttributeValue { Null, ExistingRouteAwarded, NewRouteAwarded }

    public record RefreshProfessionalStatusAttributesTestCaseData(
        ProfessionalStatusType ProfessionalStatusType,
        bool HaveExistingRouteForStatus,
        bool ExistingRouteIsHoldsStatus,
        bool HaveNewRouteForStatus,
        bool NewRouteIsHoldsStatus,
        bool NewHoldsFromIsAfterExistingHoldsFrom,
        ExpectedAttributeValue ExpectedAttributeValue,
        bool ExpectedResult);

    private class TestablePerson : Person
    {
        public TestablePerson()
        {
            Qualifications = [];
        }
    }

    private class TestableRouteToProfessionalStatus : RouteToProfessionalStatus
    {
        public new DateOnly? HoldsFrom
        {
            get => base.HoldsFrom;
            set => base.HoldsFrom = value;
        }

        public new required RouteToProfessionalStatusType RouteToProfessionalStatusType
        {
            get => base.RouteToProfessionalStatusType!;
            set => base.RouteToProfessionalStatusType = value;
        }
    }
}
