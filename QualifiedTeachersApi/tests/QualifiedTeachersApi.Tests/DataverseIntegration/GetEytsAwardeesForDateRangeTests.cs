using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public class GetEytsAwardeesForDateRangeTests : IAsyncLifetime
{
    private CrmClientFixture.TestDataScope _dataScope;
    private DataverseAdapter _dataverseAdapter;
    private ITrackedEntityOrganizationService _organizationService;

    public GetEytsAwardeesForDateRangeTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task GetEytsAwardeesForDateRange_WhenCalledForFixedDataInBuildEnvironment_ReturnsExpectedQtsAwardees()
    {
        // Arrange
        var startDate = new DateTime(2023, 06, 2, 0, 0, 00, DateTimeKind.Utc);
        var endDate = new DateTime(2023, 06, 3, 0, 0, 00, DateTimeKind.Utc);
        var expectedCount = 18;

        // Act
        var eytsAwardees = await _dataverseAdapter.GetEytsAwardeesForDateRange(startDate, endDate);

        // Assert
        Assert.Equal(expectedCount, eytsAwardees.Count());
    }

    [Fact]
    public async Task GetEytsAwardeesForDateRange_WhenCalledForDataGeneratedInTest_ReturnsExpectedQtsAwardees()
    {
        // Arrange
        var teacher1FirstName = Faker.Name.First();
        var teacher1LastName = Faker.Name.Last();
        var teacher1EmailAddress1 = Faker.Internet.Email();
        var teacher1DateOfBirth = new DateOnly(1997, 4, 22);
        var teacher1EytsDate = new DateOnly(2022, 08, 23);
        var teacher1EarlyYearsStatusValue = "221"; // Early Years Teacher Status

        var teacher2FirstName = Faker.Name.First();
        var teacher2LastName = Faker.Name.Last();
        var teacher2EmailAddress1 = Faker.Internet.Email();
        var teacher2DateOfBirth = new DateOnly(1997, 4, 23);
        var teacher2EarlyYearsStatusValue = "220"; // Early Years Trainee

        var teacher3FirstName = Faker.Name.First();
        var teacher3LastName = Faker.Name.Last();
        var teacher3EmailAddress2 = Faker.Internet.Email();
        var teacher3DateOfBirth = new DateOnly(1997, 4, 24);
        var teacher3EytsDate = new DateOnly(2022, 08, 25);
        var teacher3EarlyYearsStatusValue = "222"; // Early Years Professional Status

        var teacher4FirstName = Faker.Name.First();
        var teacher4LastName = Faker.Name.Last();
        var teacher4StatedFirstName = Faker.Name.First();
        var teacher4StatedLastName = Faker.Name.Last();
        var teacher4EmailAddress1 = Faker.Internet.Email();
        var teacher4EmailAddress2 = Faker.Internet.Email();
        var teacher4DateOfBirth = new DateOnly(1997, 4, 25);
        var teacher4EytsDate = new DateOnly(2022, 08, 26);
        var teacher4EarlyYearsStatusValue = "221"; // Early Years Teacher Status

        var teacher5FirstName = Faker.Name.First();
        var teacher5LastName = Faker.Name.Last();
        var teacher5EmailAddress1 = Faker.Internet.Email();
        var teacher5DateOfBirth = new DateOnly(1997, 4, 26);
        var teacher5EytsDate = new DateOnly(2022, 08, 27);
        var teacher5EarlyYearsStatusValue = "222"; // Early Years Professional Status

        var startDate = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var teacher1Id = await CreateTeacherWithEyts(
            teacher1FirstName,
            teacher1LastName,
            null,
            null,
            teacher1EmailAddress1,
            null,
            teacher1DateOfBirth,
            teacher1EytsDate,
            teacher1EarlyYearsStatusValue);

        await Task.Delay(3000);

        var teacher2Id = await CreateTeacherWithEyts(
            teacher2FirstName,
            teacher2LastName,
            null,
            null,
            teacher2EmailAddress1,
            null,
            teacher2DateOfBirth,
            null,
            teacher2EarlyYearsStatusValue);

        await Task.Delay(3000);

        var teacher3Id = await CreateTeacherWithEyts(
            teacher3FirstName,
            teacher3LastName,
            null,
            null,
            null,
            teacher3EmailAddress2,
            teacher3DateOfBirth,
            teacher3EytsDate,
            teacher3EarlyYearsStatusValue);

        await Task.Delay(3000);

        var teacher4Id = await CreateTeacherWithEyts(
            teacher4FirstName,
            teacher4LastName,
            teacher4StatedFirstName,
            teacher4StatedLastName,
            teacher4EmailAddress1,
            teacher4EmailAddress2,
            teacher4DateOfBirth,
            teacher4EytsDate,
            teacher4EarlyYearsStatusValue);

        // Allow for the fact that we are using "less than" with the end date
        await Task.Delay(1000);

        stopwatch.Stop();
        var endDate = startDate.AddSeconds(Math.Ceiling(stopwatch.Elapsed.TotalSeconds));

        await Task.Delay(2000);

        var teacher5Id = await CreateTeacherWithEyts(
            teacher5FirstName,
            teacher5LastName,
            null,
            null,
            teacher5EmailAddress1,
            null,
            teacher5DateOfBirth,
            teacher5EytsDate,
            teacher5EarlyYearsStatusValue);

        // Act
        var eytsAwardees = await _dataverseAdapter.GetEytsAwardeesForDateRange(startDate, endDate);

        // Assert
        Assert.Equal(3, eytsAwardees.Count());
        var teacher1 = eytsAwardees.SingleOrDefault(a => a.TeacherId == teacher1Id);
        Assert.NotNull(teacher1);
        Assert.Equal(teacher1FirstName, teacher1.FirstName);
        Assert.Equal(teacher1LastName, teacher1.LastName);
        Assert.Equal(teacher1EmailAddress1, teacher1.EmailAddress);
        var teacher3 = eytsAwardees.SingleOrDefault(a => a.TeacherId == teacher3Id);
        Assert.NotNull(teacher3);
        Assert.Equal(teacher3Id, teacher3.TeacherId);
        Assert.Equal(teacher3FirstName, teacher3.FirstName);
        Assert.Equal(teacher3LastName, teacher3.LastName);
        Assert.Equal(teacher3EmailAddress2, teacher3.EmailAddress);
        var teacher4 = eytsAwardees.SingleOrDefault(a => a.TeacherId == teacher4Id);
        Assert.NotNull(teacher4);
        Assert.Equal(teacher4Id, teacher4.TeacherId);
        Assert.Equal(teacher4StatedFirstName, teacher4.FirstName);
        Assert.Equal(teacher4StatedLastName, teacher4.LastName);
        Assert.Equal(teacher4EmailAddress1, teacher4.EmailAddress);
    }

    private async Task<Guid> CreateTeacherWithEyts(
        string firstName,
        string lastName,
        string? statedFirstName,
        string? statedLastName,
        string? emailAddress1,
        string? emailAddress2,
        DateOnly dateOfBirth,
        DateOnly? eytsDate,
        string earlyYearsStatusValue)
    {
        var contact = new Contact()
        {
            FirstName = firstName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTime(),
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

        if (eytsDate != null)
        {
            contact.dfeta_EYTSDate = eytsDate.ToDateTime();
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

        var earlyYearsTeacherStatus = await _dataverseAdapter.GetEarlyYearsStatus(earlyYearsStatusValue, null);
        var qtsRegistration = new dfeta_qtsregistration()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_EarlyYearsStatusId = new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, earlyYearsTeacherStatus.Id)
        };

        if (eytsDate != null)
        {
            qtsRegistration.dfeta_EYTSDate = eytsDate.ToDateTime();
        }

        await _organizationService.CreateAsync(qtsRegistration);

        return teacherId;
    }
}
