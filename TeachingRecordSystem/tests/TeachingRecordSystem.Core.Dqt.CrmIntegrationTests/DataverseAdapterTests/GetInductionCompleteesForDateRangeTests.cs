using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class GetInductionCompleteesForDateRangeTests : IAsyncLifetime
{
    private CrmClientFixture.TestDataScope _dataScope;
    private DataverseAdapter _dataverseAdapter;
    private ITrackedEntityOrganizationService _organizationService;

    public GetInductionCompleteesForDateRangeTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task GetInductionCompleteesForDateRange_WhenCalledForDataGeneratedInTest_ReturnsExpectedQtsAwardees()
    {
        // Arrange
        var teacher1FirstName = Faker.Name.First();
        var teacher1LastName = Faker.Name.Last();
        var teacher1EmailAddress1 = Faker.Internet.Email();
        var teacher1DateOfBirth = new DateOnly(1997, 4, 22);
        var teacher1QtsDate = new DateOnly(2022, 08, 23);
        var teacher1StatusValue = "100"; // Qualified Teacher: Assessment Only Route
        var teacher1InitialInductionStatus = dfeta_InductionStatus.InProgress;
        var teacher1FinalInductionStatus = dfeta_InductionStatus.Pass;

        var teacher2FirstName = Faker.Name.First();
        var teacher2LastName = Faker.Name.Last();
        var teacher2EmailAddress1 = Faker.Internet.Email();
        var teacher2DateOfBirth = new DateOnly(1997, 4, 23);
        var teacher2QtsDate = new DateOnly(2022, 08, 24);
        var teacher2StatusValue = "100"; // Qualified Teacher: Assessment Only Route
        var teacher2FinalInductionStatus = dfeta_InductionStatus.FailedinWales;

        var teacher3FirstName = Faker.Name.First();
        var teacher3LastName = Faker.Name.Last();
        var teacher3EmailAddress2 = Faker.Internet.Email();
        var teacher3DateOfBirth = new DateOnly(1997, 4, 24);
        var teacher3QtsDate = new DateOnly(2022, 08, 25);
        var teacher3StatusValue = "71"; // Qualified teacher (trained)
        var teacher3FinalInductionStatus = dfeta_InductionStatus.Pass;

        var teacher4FirstName = Faker.Name.First();
        var teacher4LastName = Faker.Name.Last();
        var teacher4StatedFirstName = Faker.Name.First();
        var teacher4StatedLastName = Faker.Name.Last();
        var teacher4EmailAddress1 = Faker.Internet.Email();
        var teacher4EmailAddress2 = Faker.Internet.Email();
        var teacher4DateOfBirth = new DateOnly(1997, 4, 25);
        var teacher4QtsDate = new DateOnly(2022, 08, 26);
        var teacher4StatusValue = "100"; // Qualified Teacher: Assessment Only Route
        var teacher4InitialInductionStatus = dfeta_InductionStatus.InProgress;
        var teacher4FinalInductionStatus = dfeta_InductionStatus.Pass;

        var teacher5FirstName = Faker.Name.First();
        var teacher5LastName = Faker.Name.Last();
        var teacher5EmailAddress1 = Faker.Internet.Email();
        var teacher5DateOfBirth = new DateOnly(1997, 4, 26);
        var teacher5QtsDate = new DateOnly(2022, 08, 27);
        var teacher5StatusValue = "71"; // Qualified teacher (trained)
        var teacher5FinalInductionStatus = dfeta_InductionStatus.Pass;

        var startDate = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var teacher1Id = await CreateTeacherWithInduction(
            teacher1FirstName,
            teacher1LastName,
            null,
            null,
            teacher1EmailAddress1,
            null,
            teacher1DateOfBirth,
            teacher1QtsDate,
            teacher1StatusValue,
            teacher1InitialInductionStatus,
            teacher1FinalInductionStatus);

        await Task.Delay(3000);

        var teacher2Id = await CreateTeacherWithInduction(
            teacher2FirstName,
            teacher2LastName,
            null,
            null,
            teacher2EmailAddress1,
            null,
            teacher2DateOfBirth,
            teacher2QtsDate,
            teacher2StatusValue,
            null,
            teacher2FinalInductionStatus);

        await Task.Delay(3000);

        var teacher3Id = await CreateTeacherWithInduction(
            teacher3FirstName,
            teacher3LastName,
            null,
            null,
            null,
            teacher3EmailAddress2,
            teacher3DateOfBirth,
            teacher3QtsDate,
            teacher3StatusValue,
            null,
            teacher3FinalInductionStatus);

        await Task.Delay(3000);

        var teacher4Id = await CreateTeacherWithInduction(
            teacher4FirstName,
            teacher4LastName,
            teacher4StatedFirstName,
            teacher4StatedLastName,
            teacher4EmailAddress1,
            teacher4EmailAddress2,
            teacher4DateOfBirth,
            teacher4QtsDate,
            teacher4StatusValue,
            teacher4InitialInductionStatus,
            teacher4FinalInductionStatus);

        // Allow for the fact that we are using "less than" with the end date
        await Task.Delay(1000);

        stopwatch.Stop();
        var endDate = startDate.AddSeconds(Math.Ceiling(stopwatch.Elapsed.TotalSeconds));

        await Task.Delay(2000);

        var teacher5Id = await CreateTeacherWithInduction(
            teacher5FirstName,
            teacher5LastName,
            null,
            null,
            teacher5EmailAddress1,
            null,
            teacher5DateOfBirth,
            teacher5QtsDate,
            teacher5StatusValue,
            null,
            teacher5FinalInductionStatus);

        // Act
        var inductionCompletees = (await _dataverseAdapter.GetInductionCompleteesForDateRangeAsync(startDate, endDate).ToListAsync()).Single();

        // Assert
        Assert.Equal(3, inductionCompletees.Count());
        var teacher1 = inductionCompletees.SingleOrDefault(a => a.TeacherId == teacher1Id);
        Assert.NotNull(teacher1);
        Assert.Equal(teacher1FirstName, teacher1.FirstName);
        Assert.Equal(teacher1LastName, teacher1.LastName);
        Assert.Equal(teacher1EmailAddress1, teacher1.EmailAddress);
        var teacher3 = inductionCompletees.SingleOrDefault(a => a.TeacherId == teacher3Id);
        Assert.NotNull(teacher3);
        Assert.Equal(teacher3Id, teacher3.TeacherId);
        Assert.Equal(teacher3FirstName, teacher3.FirstName);
        Assert.Equal(teacher3LastName, teacher3.LastName);
        Assert.Equal(teacher3EmailAddress2, teacher3.EmailAddress);
        var teacher4 = inductionCompletees.SingleOrDefault(a => a.TeacherId == teacher4Id);
        Assert.NotNull(teacher4);
        Assert.Equal(teacher4Id, teacher4.TeacherId);
        Assert.Equal(teacher4StatedFirstName, teacher4.FirstName);
        Assert.Equal(teacher4StatedLastName, teacher4.LastName);
        Assert.Equal(teacher4EmailAddress1, teacher4.EmailAddress);
    }

    private async Task<Guid> CreateTeacherWithInduction(
        string firstName,
        string lastName,
        string? statedFirstName,
        string? statedLastName,
        string? emailAddress1,
        string? emailAddress2,
        DateOnly dateOfBirth,
        DateOnly qtsDate,
        string teacherStatusValue,
        dfeta_InductionStatus? initialInductionStatus,
        dfeta_InductionStatus finalInductionStatus)
    {
        var contact = new Contact()
        {
            FirstName = firstName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTime(),
            dfeta_QTSDate = qtsDate.ToDateTime()
        };

        if (statedFirstName != null)
        {
            contact.dfeta_StatedFirstName = statedFirstName;
        }

        if (statedLastName != null)
        {
            contact.dfeta_StatedLastName = statedLastName;
        }

        if (emailAddress1 != null)
        {
            contact.EMailAddress1 = emailAddress1;
        }

        if (emailAddress2 != null)
        {
            contact.EMailAddress2 = emailAddress2;
        }

        var teacherId = await _organizationService.CreateAsync(contact);

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

        var teacherStatus = await _dataverseAdapter.GetTeacherStatusAsync(teacherStatusValue, null);
        await _organizationService.CreateAsync(new dfeta_qtsregistration()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id),
            dfeta_QTSDate = qtsDate.ToDateTime()
        });

        if (initialInductionStatus != null)
        {
            var inductionId = await _organizationService.CreateAsync(new dfeta_induction()
            {
                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                dfeta_InductionStatus = initialInductionStatus
            });

            await _organizationService.ExecuteAsync(new UpdateRequest()
            {
                Target = new dfeta_induction()
                {
                    Id = inductionId,
                    dfeta_InductionStatus = finalInductionStatus
                }
            });
        }
        else
        {
            await _organizationService.CreateAsync(new dfeta_induction()
            {
                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                dfeta_InductionStatus = finalInductionStatus
            });
        }

        return teacherId;
    }
}
