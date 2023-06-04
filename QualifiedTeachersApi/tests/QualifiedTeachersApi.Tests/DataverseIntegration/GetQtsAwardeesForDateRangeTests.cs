using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public class GetQtsAwardeesForDateRangeTests : IAsyncLifetime
{
    private CrmClientFixture.TestDataScope _dataScope;
    private DataverseAdapter _dataverseAdapter;
    private ITrackedEntityOrganizationService _organizationService;

    public GetQtsAwardeesForDateRangeTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task GetQtsAwardeesForDateRange_WhenCalledForFixedDataInBuildEnvironment_ReturnsExpectedQtsAwardees()
    {
        // Arrange
        var startDate = new DateTime(2022, 04, 13, 09, 53, 00, DateTimeKind.Utc);
        var endDate = new DateTime(2022, 04, 13, 11, 07, 59, DateTimeKind.Utc);
        var expectedCount = 8;

        // Act
        var qtsAwardees = await _dataverseAdapter.GetQtsAwardeesForDateRange(startDate, endDate);

        // Assert
        Assert.Equal(expectedCount, qtsAwardees.Count());
    }

    [Fact]
    public async Task GetQtsAwardeesForDateRange_WhenCalledForDataGeneratedInTest_ReturnsExpectedQtsAwardees()
    {
        // Arrange
        var teacher1FirstName = Faker.Name.First();
        var teacher1LastName = Faker.Name.Last();
        var teacher1EmailAddress1 = Faker.Internet.Email();
        var teacher1DateOfBirth = new DateOnly(1997, 4, 22);
        var teacher1QtsDate = new DateOnly(2022, 08, 23);
        var teacher1StatusValue = "100"; // Qualified Teacher: Assessment Only Route

        var teacher2FirstName = Faker.Name.First();
        var teacher2LastName = Faker.Name.Last();
        var teacher2EmailAddress1 = Faker.Internet.Email();
        var teacher2DateOfBirth = new DateOnly(1997, 4, 23);
        var teacher2QtsDate = new DateOnly(2022, 08, 24);
        var teacher2StatusValue = "103"; // Qualified Teacher: By virtue of overseas qualifications

        var teacher3FirstName = Faker.Name.First();
        var teacher3LastName = Faker.Name.Last();
        var teacher3EmailAddress2 = Faker.Internet.Email();
        var teacher3DateOfBirth = new DateOnly(1997, 4, 24);
        var teacher3QtsDate = new DateOnly(2022, 08, 25);
        var teacher3StatusValue = "71"; // Qualified teacher (trained)

        var teacher4FirstName = Faker.Name.First();
        var teacher4LastName = Faker.Name.Last();
        var teacher4StatedFirstName = Faker.Name.First();
        var teacher4StatedLastName = Faker.Name.Last();
        var teacher4EmailAddress1 = Faker.Internet.Email();
        var teacher4EmailAddress2 = Faker.Internet.Email();
        var teacher4DateOfBirth = new DateOnly(1997, 4, 25);
        var teacher4QtsDate = new DateOnly(2022, 08, 26);
        var teacher4StatusValue = "100"; // Qualified Teacher: Assessment Only Route

        var teacher5FirstName = Faker.Name.First();
        var teacher5LastName = Faker.Name.Last();
        var teacher5EmailAddress1 = Faker.Internet.Email();
        var teacher5DateOfBirth = new DateOnly(1997, 4, 26);
        var teacher5QtsDate = new DateOnly(2022, 08, 27);
        var teacher5StatusValue = "71"; // Qualified Teacher: Assessment Only Route

        var startDate = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var teacher1Id = await CreateTeacherWithQts(
            teacher1FirstName,
            teacher1LastName,
            null,
            null,
            teacher1EmailAddress1,
            null,
            teacher1DateOfBirth,
            teacher1QtsDate,
            teacher1StatusValue);

        await Task.Delay(2000);

        var teacher2Id = await CreateTeacherWithQts(
            teacher2FirstName,
            teacher2LastName,
            null,
            null,
            teacher2EmailAddress1,
            null,
            teacher2DateOfBirth,
            teacher2QtsDate,
            teacher2StatusValue);

        await Task.Delay(2000);

        var teacher3Id = await CreateTeacherWithQts(
            teacher3FirstName,
            teacher3LastName,
            null,
            null,
            null,
            teacher3EmailAddress2,
            teacher3DateOfBirth,
            teacher3QtsDate,
            teacher3StatusValue);

        await Task.Delay(2000);

        var teacher4Id = await CreateTeacherWithQts(
            teacher4FirstName,
            teacher4LastName,
            teacher4StatedFirstName,
            teacher4StatedLastName,
            teacher4EmailAddress1,
            teacher4EmailAddress2,
            teacher4DateOfBirth,
            teacher4QtsDate,
            teacher4StatusValue);

        stopwatch.Stop();
        var endDate = startDate.AddSeconds(Math.Ceiling(stopwatch.Elapsed.TotalSeconds));

        await Task.Delay(2000);

        var teacher5Id = await CreateTeacherWithQts(
            teacher5FirstName,
            teacher5LastName,
            null,
            null,
            teacher5EmailAddress1,
            null,
            teacher5DateOfBirth,
            teacher5QtsDate,
            teacher5StatusValue);

        // Act
        var qtsAwardees = await _dataverseAdapter.GetQtsAwardeesForDateRange(startDate, endDate);

        // Assert
        Assert.Equal(3, qtsAwardees.Count());
        Assert.Equal(teacher1Id, qtsAwardees[0].TeacherId);
        Assert.Equal(teacher1FirstName, qtsAwardees[0].FirstName);
        Assert.Equal(teacher1LastName, qtsAwardees[0].LastName);
        Assert.Equal(teacher1EmailAddress1, qtsAwardees[0].EmailAddress);
        Assert.Equal(teacher3Id, qtsAwardees[1].TeacherId);
        Assert.Equal(teacher3FirstName, qtsAwardees[1].FirstName);
        Assert.Equal(teacher3LastName, qtsAwardees[1].LastName);
        Assert.Equal(teacher3EmailAddress2, qtsAwardees[1].EmailAddress);
        Assert.Equal(teacher4Id, qtsAwardees[2].TeacherId);
        Assert.Equal(teacher4StatedFirstName, qtsAwardees[2].FirstName);
        Assert.Equal(teacher4StatedLastName, qtsAwardees[2].LastName);
        Assert.Equal(teacher4EmailAddress1, qtsAwardees[2].EmailAddress);
    }

    private async Task<Guid> CreateTeacherWithQts(
        string firstName,
        string lastName,
        string? statedFirstName,
        string? statedLastName,
        string? emailAddress1,
        string? emailAddress2,
        DateOnly dateOfBirth,
        DateOnly qtsDate,
        string teacherStatusValue)
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

        var teacherStatus = await _dataverseAdapter.GetTeacherStatus(teacherStatusValue, null);
        await _organizationService.CreateAsync(new dfeta_qtsregistration()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id),
            dfeta_QTSDate = qtsDate.ToDateTime()
        });

        return teacherId;
    }
}
