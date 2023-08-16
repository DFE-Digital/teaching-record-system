#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class UpdateTeacherIdentityInfoTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public UpdateTeacherIdentityInfoTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Fact]
    public async Task Given_UpdatingExistingTeacher_SetsFieldValuesAsExpected()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var originalEmailAddress = Faker.Internet.Email();
        var originalMobileNumber = Faker.Phone.Number();
        var updatedEmailAddress = Faker.Internet.Email();
        var updatedMobileNumber = Faker.Phone.Number();
        var identityUserId = Guid.NewGuid();

        var teacherId = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            LastName = lastName,
            EMailAddress1 = originalEmailAddress,
            MobilePhone = originalMobileNumber,
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = teacherId,
                dfeta_TRNAllocateRequest = DateTime.UtcNow
            }
        });

        // Act
        var updateTime = DateTime.UtcNow;
        await _dataverseAdapter.UpdateTeacherIdentityInfo(new UpdateTeacherIdentityInfoCommand()
        {
            TeacherId = teacherId,
            IdentityUserId = identityUserId,
            EmailAddress = updatedEmailAddress,
            MobilePhone = updatedMobileNumber,
            UpdateTimeUtc = updateTime
        });

        // Assert
        var updatedTeacher = await _dataverseAdapter.GetTeacher(
            teacherId,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_TSPersonID,
                Contact.Fields.EMailAddress1,
                Contact.Fields.MobilePhone,
                Contact.Fields.dfeta_LastIdentityUpdate
            });

        Assert.Equal(identityUserId.ToString(), updatedTeacher.dfeta_TSPersonID);
        Assert.Equal(updatedEmailAddress, updatedTeacher.EMailAddress1);
        Assert.Equal(updatedMobileNumber, updatedTeacher.MobilePhone);
        Assert.Equal(updateTime, updatedTeacher.dfeta_LastIdentityUpdate.Value, TimeSpan.FromMilliseconds(1000));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
