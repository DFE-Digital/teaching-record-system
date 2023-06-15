#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class GetInitialTeacherTrainingByTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public GetInitialTeacherTrainingByTeacherTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Fact]
    public async Task Given_inactive_itt_record_not_returned_by_default()
    {
        // Arrange
        var firstName = "Joe";
        var teacherId = await _organizationService.CreateAsync(new Contact() { FirstName = firstName });
        var ittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId) });
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                StateCode = dfeta_initialteachertrainingState.Inactive
            }
        });

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.FirstName });
        var ittRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(teacherId, columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                dfeta_initialteachertraining.Fields.StateCode
            });

        // Assert
        Assert.Empty(ittRecords);
    }

    [Fact]
    public async Task Given_active_and_inactive_itt_records_are_returned_when_passing_includeInactive()
    {
        // Arrange
        var firstName = "Joe";
        var teacherId = await _organizationService.CreateAsync(new Contact() { FirstName = firstName });
        var aciveittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId) });
        var inActiveittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId) });
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = inActiveittId,
                StateCode = dfeta_initialteachertrainingState.Inactive
            }
        });

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.FirstName });
        var ittRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(teacherId, columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                dfeta_initialteachertraining.Fields.StateCode
            },
            activeOnly: false);

        // Assert
        Assert.Collection(
                    ittRecords,
                    item1 =>
                    {
                        Assert.Equal(aciveittId, item1.Id);
                    },
                    item2 =>
                    {
                        Assert.Equal(inActiveittId, item2.Id);
                    }
                );
    }

    [Fact]
    public async Task Given_IttDataExistsForTeacher_ReturnsExpectedColumnValues()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1997, 4, 23);
        var ittStartDate = new DateOnly(2021, 9, 7);
        var ittEndDate = new DateOnly(2022, 7, 29);
        var ittProgrammeType = dfeta_ITTProgrammeType.EYITTGraduateEntry;
        var ittResult = dfeta_ITTResult.Pass;
        var ittAgeRangeFrom = dfeta_AgeRange._11;
        var ittAgeRangeTo = dfeta_AgeRange._16;
        var ittProviderName = Faker.Company.Name();
        var ittProviderUkprn = "9999999999";
        var ittSubject1Value = "12345";
        var ittSubject1Name = "Subject 1";
        var ittSubject2Value = "23456";
        var ittSubject2Name = "Subject 2";
        var ittSubject3Value = "34567";
        var ittSubject3Name = "Subject 3";
        var ittQualificationName = "My test qualification 123";
        var ittQualificationValue = "MytestIttQual123";

        var teacherId = await _organizationService.CreateAsync(new Contact()
        {
            FirstName = firstName,
            LastName = lastName,
            dfeta_QTSDate = qtsDate.ToDateTime()
        });

        var establishmentId = await _organizationService.CreateAsync(new Account()
        {
            Name = ittProviderName,
            dfeta_UKPRN = ittProviderUkprn
        });

        var qualificationId = await _organizationService.CreateAsync(new dfeta_ittqualification()
        {
            dfeta_name = ittQualificationName,
            dfeta_Value = ittQualificationValue,
            StateCode = dfeta_ittqualificationState.Active
        });

        var subject1Id = await _organizationService.CreateAsync(new dfeta_ittsubject()
        {
            dfeta_name = ittSubject1Name,
            dfeta_Value = ittSubject1Value
        });

        var subject2Id = await _organizationService.CreateAsync(new dfeta_ittsubject()
        {
            dfeta_name = ittSubject2Name,
            dfeta_Value = ittSubject2Value
        });

        var subject3Id = await _organizationService.CreateAsync(new dfeta_ittsubject()
        {
            dfeta_name = ittSubject3Name,
            dfeta_Value = ittSubject3Value
        });

        var ittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType,
            dfeta_Result = ittResult,
            dfeta_AgeRangeFrom = ittAgeRangeFrom,
            dfeta_AgeRangeTo = ittAgeRangeTo,
            StateCode = dfeta_initialteachertrainingState.Active,
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, establishmentId),
            dfeta_Subject1Id = new EntityReference(dfeta_ittsubject.EntityLogicalName, subject1Id),
            dfeta_Subject2Id = new EntityReference(dfeta_ittsubject.EntityLogicalName, subject2Id),
            dfeta_Subject3Id = new EntityReference(dfeta_ittsubject.EntityLogicalName, subject3Id),
            dfeta_ITTQualificationId = new EntityReference(dfeta_ittqualification.EntityLogicalName, qualificationId)
        });

        // Act
        var ittRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
            teacherId,
            columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                dfeta_initialteachertraining.Fields.dfeta_Result,
                dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                dfeta_initialteachertraining.Fields.StateCode
            },
            establishmentColumnNames: new[]
            {
                Account.PrimaryIdAttribute,
                Account.Fields.dfeta_UKPRN,
                Account.Fields.Name
            },
            subjectColumnNames: new[]
            {
                dfeta_ittsubject.PrimaryIdAttribute,
                dfeta_ittsubject.Fields.dfeta_name,
                dfeta_ittsubject.Fields.dfeta_Value
            },
            qualificationColumnNames: new[]
            {
                dfeta_ittqualification.PrimaryIdAttribute,
                dfeta_ittqualification.Fields.dfeta_name
            },
            activeOnly: false);

        // Assert
        Assert.Collection(
                    ittRecords,
                    item1 =>
                    {
                        Assert.Equal(ittId, item1.Id);
                        Assert.Equal(ittProgrammeType, item1.dfeta_ProgrammeType);
                        Assert.Equal(ittResult, item1.dfeta_Result);
                        Assert.Equal(ittAgeRangeFrom, item1.dfeta_AgeRangeFrom);
                        Assert.Equal(ittAgeRangeTo, item1.dfeta_AgeRangeTo);

                        var establishment = item1.Extract<Account>("establishment", Account.PrimaryIdAttribute);
                        Assert.NotNull(establishment);
                        Assert.Equal(ittProviderName, establishment.Name);
                        Assert.Equal(ittProviderUkprn, establishment.dfeta_UKPRN);

                        var qualification = item1.Extract<dfeta_ittqualification>("qualification", dfeta_ittqualification.PrimaryIdAttribute);
                        Assert.NotNull(qualification);
                        Assert.Equal(ittQualificationName, qualification.dfeta_name);

                        var subject1 = item1.Extract<dfeta_ittsubject>("subject1", dfeta_ittsubject.PrimaryIdAttribute);
                        Assert.NotNull(subject1);
                        Assert.Equal(ittSubject1Name, subject1.dfeta_name);
                        Assert.Equal(ittSubject1Value, subject1.dfeta_Value);
                        var subject2 = item1.Extract<dfeta_ittsubject>("subject2", dfeta_ittsubject.PrimaryIdAttribute);
                        Assert.NotNull(subject2);
                        Assert.Equal(ittSubject2Name, subject2.dfeta_name);
                        Assert.Equal(ittSubject2Value, subject2.dfeta_Value);
                        var subject3 = item1.Extract<dfeta_ittsubject>("subject3", dfeta_ittsubject.PrimaryIdAttribute);
                        Assert.NotNull(subject3);
                        Assert.Equal(ittSubject3Name, subject3.dfeta_name);
                        Assert.Equal(ittSubject3Value, subject3.dfeta_Value);
                    }
                );
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
