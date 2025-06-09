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

    [Theory]
    [MemberData(nameof(GetRefreshProfessionalStatusAttributesData))]
    public void RefreshProfessionalStatusAttributes(RefreshProfessionalStatusAttributesTestCaseData testCaseData)
    {
        // Arrange
        var person = CreatePerson();

        var allRoutes = new List<RouteToProfessionalStatusType>();
        var professionalStatuses = new List<RouteToProfessionalStatus>();

        DateOnly? existingRouteAwarded = Clock.Today.AddDays(-10);
        ProfessionalStatusStatus existingRouteStatus =
            testCaseData.ExistingRouteIsAwardedOrApproved ? ProfessionalStatusStatus.Awarded : ProfessionalStatusStatus.InTraining;
        DateOnly? newRouteAwardedDate = testCaseData.NewAwardedDateIsAfterExistingAwardedDate ? Clock.Today.AddDays(-1) : Clock.Today.AddDays(-20);
        ProfessionalStatusStatus newRouteStatus =
            testCaseData.NewRouteIsAwardedOrApproved ? ProfessionalStatusStatus.Awarded : ProfessionalStatusStatus.InTraining;

        if (testCaseData.HaveExistingRouteForStatus)
        {
            var anotherRoute = CreateRouteForProfessionalStatusType(testCaseData.ProfessionalStatusType);
            allRoutes.Add(anotherRoute);

            var existingProfessionalStatus = CreateProfessionalStatus(
                anotherRoute,
                status: existingRouteStatus,
                awardedDate: testCaseData.ExistingRouteIsAwardedOrApproved && testCaseData.ProfessionalStatusType is not ProfessionalStatusType.EarlyYearsProfessionalStatus ? existingRouteAwarded : null);
            professionalStatuses.Add(existingProfessionalStatus);

            if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus)
            {
                person.QtsDate = existingProfessionalStatus.AwardedDate;
            }
            else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus)
            {
                person.EytsDate = existingProfessionalStatus.AwardedDate;
            }
            else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus)
            {
                person.PqtsDate = existingProfessionalStatus.AwardedDate;
            }
            else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus)
            {
                person.HasEyps = existingProfessionalStatus.Status is ProfessionalStatusStatus.Approved or ProfessionalStatusStatus.Awarded;
            }
        }

        if (testCaseData.HaveNewRouteForStatus)
        {
            var route = CreateRouteForProfessionalStatusType(testCaseData.ProfessionalStatusType);
            allRoutes.Add(route);

            var newProfessionalStatus = CreateProfessionalStatus(
                route,
                status: newRouteStatus,
                awardedDate: testCaseData.NewRouteIsAwardedOrApproved && testCaseData.ProfessionalStatusType is not ProfessionalStatusType.EarlyYearsProfessionalStatus ? newRouteAwardedDate : null);
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
                    ExpectedAttributeValue.NewRouteAwarded => newRouteAwardedDate,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteAwarded,
                    _ => null
                },
                person.QtsDate);
        }
        else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus)
        {
            Assert.Equal(
                testCaseData.ExpectedAttributeValue switch
                {
                    ExpectedAttributeValue.NewRouteAwarded => newRouteAwardedDate,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteAwarded,
                    _ => null
                },
                person.EytsDate);
        }
        else if (testCaseData.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus)
        {
            Assert.Equal(
                testCaseData.ExpectedAttributeValue switch
                {
                    ExpectedAttributeValue.NewRouteAwarded => newRouteStatus is ProfessionalStatusStatus.Awarded or ProfessionalStatusStatus.Approved,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteStatus is ProfessionalStatusStatus.Awarded or ProfessionalStatusStatus.Approved,
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
                    ExpectedAttributeValue.NewRouteAwarded => newRouteAwardedDate,
                    ExpectedAttributeValue.ExistingRouteAwarded => existingRouteAwarded,
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
                AwardDateRequired = FieldRequirement.Optional,
                InductionExemptionRequired = FieldRequirement.Optional,
                TrainingProviderRequired = FieldRequirement.Optional,
                DegreeTypeRequired = FieldRequirement.Optional,
                TrainingCountryRequired = FieldRequirement.Optional,
                TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
                TrainingSubjectsRequired = FieldRequirement.Optional,
                InductionExemptionReasonId = null
            };

        RouteToProfessionalStatus CreateProfessionalStatus(RouteToProfessionalStatusType route, ProfessionalStatusStatus status, DateOnly? awardedDate) =>
            new RouteToProfessionalStatus()
            {
                QualificationId = Guid.NewGuid(),
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow,
                DeletedOn = null,
                PersonId = person.PersonId,
                RouteToProfessionalStatusTypeId = route.RouteToProfessionalStatusTypeId,
                SourceApplicationUserId = null,
                SourceApplicationReference = null,
                Status = status,
                AwardedDate = awardedDate,
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

    private Person CreatePerson() => new Person
    {
        PersonId = Guid.NewGuid(),
        CreatedOn = Clock.UtcNow,
        UpdatedOn = Clock.UtcNow,
        Trn = "1234567",
        FirstName = "Joe",
        MiddleName = "",
        LastName = "Bloggs",
        DateOfBirth = new DateOnly(1990, 1, 1),
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
                ExistingRouteIsAwardedOrApproved: false,
                HaveNewRouteForStatus: false,
                NewRouteIsAwardedOrApproved: false,
                NewAwardedDateIsAfterExistingAwardedDate: false,
                ExpectedAttributeValue: ExpectedAttributeValue.Null,
                ExpectedResult: false));

            // No existing routes, new route added but not awarded or approved status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: false,
                ExistingRouteIsAwardedOrApproved: false,
                HaveNewRouteForStatus: true,
                NewRouteIsAwardedOrApproved: false,
                NewAwardedDateIsAfterExistingAwardedDate: false,
                ExpectedAttributeValue: ExpectedAttributeValue.Null,
                ExpectedResult: false));

            // Existing route but not awarded or approved, new route added but not awarded or approved status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: true,
                ExistingRouteIsAwardedOrApproved: false,
                HaveNewRouteForStatus: true,
                NewRouteIsAwardedOrApproved: false,
                NewAwardedDateIsAfterExistingAwardedDate: false,
                ExpectedAttributeValue: ExpectedAttributeValue.Null,
                ExpectedResult: false));

            // Existing route but not awarded or approved, new route added at awarded or approved status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: true,
                ExistingRouteIsAwardedOrApproved: false,
                HaveNewRouteForStatus: true,
                NewRouteIsAwardedOrApproved: true,
                NewAwardedDateIsAfterExistingAwardedDate: false,
                ExpectedAttributeValue: ExpectedAttributeValue.NewRouteAwarded,
                ExpectedResult: true));

            // Existing route at awarded or approved, new route added but not awarded or approved status
            data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                professionalStatusType,
                HaveExistingRouteForStatus: true,
                ExistingRouteIsAwardedOrApproved: true,
                HaveNewRouteForStatus: true,
                NewRouteIsAwardedOrApproved: false,
                NewAwardedDateIsAfterExistingAwardedDate: false,
                ExpectedAttributeValue: ExpectedAttributeValue.ExistingRouteAwarded,
                ExpectedResult: false));

            if (professionalStatusType is not ProfessionalStatusType.EarlyYearsProfessionalStatus)
            {
                // Existing route at awarded or approved with awarded date before new route, new route added at awarded or approved status
                data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                    professionalStatusType,
                    HaveExistingRouteForStatus: true,
                    ExistingRouteIsAwardedOrApproved: true,
                    HaveNewRouteForStatus: true,
                    NewRouteIsAwardedOrApproved: true,
                    NewAwardedDateIsAfterExistingAwardedDate: true,
                    ExpectedAttributeValue: ExpectedAttributeValue.ExistingRouteAwarded,
                    ExpectedResult: false));

                // Existing route at awarded or approved with after date before new route, new route added at awarded or approved status
                data.Add(new RefreshProfessionalStatusAttributesTestCaseData(
                    professionalStatusType,
                    HaveExistingRouteForStatus: true,
                    ExistingRouteIsAwardedOrApproved: true,
                    HaveNewRouteForStatus: true,
                    NewRouteIsAwardedOrApproved: true,
                    NewAwardedDateIsAfterExistingAwardedDate: false,
                    ExpectedAttributeValue: ExpectedAttributeValue.NewRouteAwarded,
                    ExpectedResult: true));
            }
        }

        return data;
    }

    public enum ExpectedAttributeValue { Null, ExistingRouteAwarded, NewRouteAwarded }

    public record RefreshProfessionalStatusAttributesTestCaseData(
        ProfessionalStatusType ProfessionalStatusType,
        bool HaveExistingRouteForStatus,
        bool ExistingRouteIsAwardedOrApproved,
        bool HaveNewRouteForStatus,
        bool NewRouteIsAwardedOrApproved,
        bool NewAwardedDateIsAfterExistingAwardedDate,
        ExpectedAttributeValue ExpectedAttributeValue,
        bool ExpectedResult);
}
