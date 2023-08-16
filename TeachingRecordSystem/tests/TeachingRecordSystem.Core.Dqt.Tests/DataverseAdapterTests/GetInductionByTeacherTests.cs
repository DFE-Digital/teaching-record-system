#nullable disable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class GetInductionByTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly ITrackedEntityOrganizationService _organizationService;

    public GetInductionByTeacherTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Given_InductionExistsForTeacher_ReturnsExpectedColumnValues(int numberOfInductionPeriods)
    {
        if (numberOfInductionPeriods < 0 || numberOfInductionPeriods > 2)
        {
            throw new ArgumentOutOfRangeException($"{nameof(numberOfInductionPeriods)} must be 0, 1 or 2");
        }

        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var teacherStatusValue = "100"; // Qualified Teacher: Assessment Only Route 
        var qtsDate = new DateOnly(1997, 4, 23);
        var dateOfBirth = new DateOnly(1975, 4, 5);
        var inductionStartDate = new DateOnly(1996, 2, 3);
        var inductionEndDate = new DateOnly(1996, 6, 7);
        var inductionStatus = dfeta_InductionStatus.Pass;
        var inductionPeriod1StartDate = new DateOnly(1996, 2, 3);
        var inductionPeriod1EndDate = new DateOnly(1996, 4, 5);
        var inductionPeriod1Terms = 3;
        var inductionPeriod1AppropriateBodyName = "My appropriate body 1";
        var inductionPeriod2StartDate = new DateOnly(1996, 4, 6);
        var inductionPeriod2EndDate = new DateOnly(1996, 6, 7);
        var inductionPeriod2Terms = 2;
        var inductionPeriod2AppropriateBodyName = "My appropriate body 2";

        var teacherId = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTime(),
            dfeta_QTSDate = qtsDate.ToDateTime()
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = teacherId,
                dfeta_TRNAllocateRequest = DateTime.UtcNow
            }
        });

        await _organizationService.CreateAsync(new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_Result = dfeta_ITTResult.Pass,
        });

        // Need QTS to be able to get induction records into CRM due to plugin validation
        var teacherStatus = await _dataverseAdapter.GetTeacherStatus(teacherStatusValue, null);
        await _organizationService.CreateAsync(new dfeta_qtsregistration()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id),
            dfeta_QTSDate = qtsDate.ToDateTime()
        });

        var inductionId = await _organizationService.CreateAsync(new dfeta_induction()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_StartDate = inductionStartDate.ToDateTime(),
            dfeta_CompletionDate = inductionEndDate.ToDateTime(),
            dfeta_InductionStatus = inductionStatus
        });

        if (numberOfInductionPeriods > 0)
        {
            var appropriateBody1Id = await _organizationService.CreateAsync(new Account()
            {
                Name = inductionPeriod1AppropriateBodyName
            });

            await _organizationService.CreateAsync(new dfeta_inductionperiod()
            {
                dfeta_InductionId = new EntityReference(dfeta_induction.EntityLogicalName, inductionId),
                dfeta_StartDate = inductionPeriod1StartDate.ToDateTime(),
                dfeta_EndDate = inductionPeriod1EndDate.ToDateTime(),
                dfeta_Numberofterms = inductionPeriod1Terms,
                dfeta_AppropriateBodyId = new EntityReference(Account.EntityLogicalName, appropriateBody1Id),
            });
        }

        if (numberOfInductionPeriods > 1)
        {
            var appropriateBody2Id = await _organizationService.CreateAsync(new Account()
            {
                Name = inductionPeriod2AppropriateBodyName
            });

            await _organizationService.CreateAsync(new dfeta_inductionperiod()
            {
                dfeta_InductionId = new EntityReference(dfeta_induction.EntityLogicalName, inductionId),
                dfeta_StartDate = inductionPeriod2StartDate.ToDateTime(),
                dfeta_EndDate = inductionPeriod2EndDate.ToDateTime(),
                dfeta_Numberofterms = inductionPeriod2Terms,
                dfeta_AppropriateBodyId = new EntityReference(Account.EntityLogicalName, appropriateBody2Id),
            });
        }

        // Act
        var (induction, inductionPeriods) = await _dataverseAdapter.GetInductionByTeacher(
            teacherId,
            columnNames: new[]
            {
                dfeta_induction.PrimaryIdAttribute,
                dfeta_induction.Fields.dfeta_StartDate,
                dfeta_induction.Fields.dfeta_CompletionDate,
                dfeta_induction.Fields.dfeta_InductionStatus
            },
            inductionPeriodColumnNames: new[]
            {
                dfeta_inductionperiod.Fields.dfeta_InductionId,
                dfeta_inductionperiod.Fields.dfeta_StartDate,
                dfeta_inductionperiod.Fields.dfeta_EndDate,
                dfeta_inductionperiod.Fields.dfeta_Numberofterms,
                dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId
            },
            appropriateBodyColumnNames: new[]
            {
                Account.PrimaryIdAttribute,
                Account.Fields.Name
            },
            contactColumnNames: new[]
            {
                Contact.PrimaryIdAttribute,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName
            });

        // Assert
        Assert.NotNull(induction);
        Assert.Equal(inductionId, induction.Id);
        Assert.Equal(inductionStartDate.ToDateTime(), induction.dfeta_StartDate);
        Assert.Equal(inductionEndDate.ToDateTime(), induction.dfeta_CompletionDate);
        Assert.Equal(inductionStatus, induction.dfeta_InductionStatus);

        var teacher = induction.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute);
        Assert.NotNull(teacher);
        Assert.Equal(firstName, teacher.FirstName);
        Assert.Equal(middleName, teacher.MiddleName);
        Assert.Equal(lastName, teacher.LastName);

        Assert.NotNull(inductionPeriods);

        if (numberOfInductionPeriods > 0)
        {
            var count = inductionPeriods.Length;
            Assert.True(count > 0);
            Assert.Equal(inductionPeriod1StartDate.ToDateTime(), inductionPeriods[0].dfeta_StartDate);
            Assert.Equal(inductionPeriod1EndDate.ToDateTime(), inductionPeriods[0].dfeta_EndDate);
            Assert.Equal(inductionPeriod1Terms, inductionPeriods[0].dfeta_Numberofterms);
            var appropriateBody = inductionPeriods[0].Extract<Account>("appropriatebody", Account.PrimaryIdAttribute);
            Assert.NotNull(appropriateBody);
            Assert.Equal(inductionPeriod1AppropriateBodyName, appropriateBody.Name);
        }

        if (numberOfInductionPeriods == 2)
        {
            Assert.Equal(2, inductionPeriods.Length);
            var inductionPeriod2 = inductionPeriods.Where(p => p.dfeta_StartDate == inductionPeriod2StartDate.ToDateTime()).SingleOrDefault();
            Assert.NotNull(inductionPeriod2);
            Assert.Equal(inductionPeriod2EndDate.ToDateTime(), inductionPeriod2.dfeta_EndDate);
            Assert.Equal(inductionPeriod2Terms, inductionPeriod2.dfeta_Numberofterms);
            var appropriateBody = inductionPeriod2.Extract<Account>("appropriatebody", Account.PrimaryIdAttribute);
            Assert.NotNull(appropriateBody);
            Assert.Equal(inductionPeriod2AppropriateBodyName, appropriateBody.Name);
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
