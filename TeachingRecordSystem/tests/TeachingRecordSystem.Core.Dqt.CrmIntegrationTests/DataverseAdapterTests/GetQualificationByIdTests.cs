#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class GetQualificationByIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public GetQualificationByIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_QualificationExistForId_ReturnsExpectedColumnValues(
        bool setContactColumnNames
        )
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var qualificationType = dfeta_qualification_dfeta_Type.NPQEYL;
        var qualificationAwardDate = new DateOnly(2022, 3, 4);
        var qualificationStatus = dfeta_qualificationState.Active;

        var teacherId = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName
        });

        var qualificationId = await _organizationService.CreateAsync(new dfeta_qualification()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_Type = qualificationType,
            StateCode = qualificationStatus
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = qualificationId,
                dfeta_CompletionorAwardDate = qualificationAwardDate.ToDateTime()
            }
        });

        // Act
        var qualification = await _dataverseAdapter.GetQualificationByIdAsync(
            qualificationId,
            columnNames: new[]
            {
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.StateCode,
                dfeta_qualification.Fields.dfeta_PersonId
            },
            setContactColumnNames
            ? new[]
            {
                Contact.PrimaryIdAttribute,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName
            }
            : null);

        // Assert
        Assert.NotNull(qualification);
        Assert.Equal(qualificationType, qualification.dfeta_Type);
        Assert.Equal(qualificationAwardDate.ToDateTime(), qualification.dfeta_CompletionorAwardDate);
        Assert.Equal(qualificationStatus, qualification.StateCode);

        var contact = qualification.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute);
        if (setContactColumnNames)
        {
            Assert.NotNull(contact);
            Assert.Equal(firstName, contact.FirstName);
            Assert.Equal(middleName, contact.MiddleName);
            Assert.Equal(lastName, contact.LastName);
        }
        else
        {
            Assert.Null(contact);
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
