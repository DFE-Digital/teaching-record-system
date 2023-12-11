#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class GetQualificationsForTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public GetQualificationsForTeacherTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    public async Task Given_QualificationsExistForTeacher_ReturnsExpectedColumnValues(
        bool setHeQualificationColumnNames,
        bool setHeSubjectColumnNames,
        bool setSpecialismColumnNames)
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var teacherStatusValue = "100"; // Qualified Teacher: Assessment Only Route 
        var qtsDate = new DateOnly(1997, 4, 23);
        var dateOfBirth = new DateOnly(1975, 4, 5);
        var qualification1Type = dfeta_qualification_dfeta_Type.NPQEYL;
        var qualification1AwardDate = new DateOnly(2022, 3, 4);
        var qualification1Status = dfeta_qualificationState.Active;
        var qualification2Type = dfeta_qualification_dfeta_Type.HigherEducation;
        var qualification2Status = dfeta_qualificationState.Active;
        var qualification3Type = dfeta_qualification_dfeta_Type.NPQSL;
        var qualification3AwardDate = new DateOnly(2021, 5, 6);
        var qualification3Status = dfeta_qualificationState.Inactive;
        var heQualificationName = "The big D";
        var heSubject1Value = "12345";
        var heSubject1Name = "Subject 1";
        var heSubject2Value = "23456";
        var heSubject2Name = "Subject 2";
        var heSubject3Value = "34567";
        var heSubject3Name = "Subject 3";
        var mqSpecialismName = "My MQ Specialism Name";
        var mqSpecialismValue = "My MQ Specialism Value";

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

        // Need QTS to be able to get MQ quals into CRM due to plugin validation
        var teacherStatus = await _dataverseAdapter.GetTeacherStatus(teacherStatusValue, null);
        await _organizationService.CreateAsync(new dfeta_qtsregistration()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id),
            dfeta_QTSDate = qtsDate.ToDateTime()
        });

        var qualification1Id = await _organizationService.CreateAsync(new dfeta_qualification()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_Type = qualification1Type,
            StateCode = qualification1Status
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = qualification1Id,
                dfeta_CompletionorAwardDate = qualification1AwardDate.ToDateTime()
            }
        });

        var heQualificationId = await _organizationService.CreateAsync(new dfeta_hequalification()
        {
            dfeta_name = heQualificationName
        });

        var heSubject1Id = await _organizationService.CreateAsync(new dfeta_hesubject()
        {
            dfeta_name = heSubject1Name,
            dfeta_Value = heSubject1Value
        });

        var heSubject2Id = await _organizationService.CreateAsync(new dfeta_hesubject()
        {
            dfeta_name = heSubject2Name,
            dfeta_Value = heSubject2Value
        });

        var heSubject3Id = await _organizationService.CreateAsync(new dfeta_hesubject()
        {
            dfeta_name = heSubject3Name,
            dfeta_Value = heSubject3Value
        });

        var qualification2Id = await _organizationService.CreateAsync(new dfeta_qualification()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_Type = qualification2Type,
            dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, heQualificationId),
            dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject1Id),
            dfeta_HE_HESubject2Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject2Id),
            dfeta_HE_HESubject3Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject3Id)
        });

        var qualification3Id = await _organizationService.CreateAsync(new dfeta_qualification()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_Type = qualification3Type,
            dfeta_CompletionorAwardDate = qualification3AwardDate.ToDateTime(),
            StateCode = qualification3Status
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = qualification3Id,
                StateCode = qualification3Status
            }
        });

        var mqSpecialismId = await _organizationService.CreateAsync(new dfeta_specialism()
        {
            dfeta_name = mqSpecialismName,
            dfeta_Value = mqSpecialismValue
        });

        var mqQualificationId = await _organizationService.CreateAsync(new dfeta_qualification()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            StateCode = dfeta_qualificationState.Active,
            dfeta_MQ_SpecialismId = new EntityReference(dfeta_specialism.EntityLogicalName, mqSpecialismId)
        });

        // Act
        var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
            teacherId,
            columnNames: new[]
            {
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.StateCode
            },
            setHeQualificationColumnNames ?
                new[]
                {
                    dfeta_hequalification.PrimaryIdAttribute,
                    dfeta_hequalification.Fields.dfeta_name
                }
                : null,
            setHeSubjectColumnNames ?
                new[]
                {
                    dfeta_hesubject.PrimaryIdAttribute,
                    dfeta_hesubject.Fields.dfeta_name,
                    dfeta_hesubject.Fields.dfeta_Value
                } :
                null,
            setSpecialismColumnNames ?
                new[]
                {
                    dfeta_specialism.PrimaryIdAttribute,
                    dfeta_specialism.Fields.dfeta_name
                }
                : null);

        // Assert
        Assert.Collection(
            qualifications,
            item1 =>
            {
                Assert.Equal(qualification1Id, item1.Id);
                Assert.Equal(qualification1Type, item1.dfeta_Type);
                Assert.Equal(qualification1AwardDate.ToDateTime(), item1.dfeta_CompletionorAwardDate);
                Assert.Equal(qualification1Status, item1.StateCode);
            },
            item2 =>
            {
                Assert.Equal(qualification2Id, item2.Id);
                Assert.Equal(qualification2Type, item2.dfeta_Type);
                Assert.Equal(qualification2Status, item2.StateCode);

                var heQualification = item2.Extract<dfeta_hequalification>("dfeta_hequalification", dfeta_hequalification.PrimaryIdAttribute);
                if (setHeQualificationColumnNames)
                {
                    Assert.NotNull(heQualification);
                    Assert.Equal(heQualificationName, heQualification.dfeta_name);
                }
                else
                {
                    Assert.Null(heQualification);
                }

                var heSubject1 = item2.Extract<dfeta_hesubject>("dfeta_hesubject1", dfeta_hesubject.PrimaryIdAttribute);
                var heSubject2 = item2.Extract<dfeta_hesubject>("dfeta_hesubject2", dfeta_hesubject.PrimaryIdAttribute);
                var heSubject3 = item2.Extract<dfeta_hesubject>("dfeta_hesubject3", dfeta_hesubject.PrimaryIdAttribute);
                if (setHeSubjectColumnNames)
                {
                    Assert.NotNull(heSubject1);
                    Assert.Equal(heSubject1Name, heSubject1.dfeta_name);
                    Assert.Equal(heSubject1Value, heSubject1.dfeta_Value);
                    Assert.NotNull(heSubject2);
                    Assert.Equal(heSubject2Name, heSubject2.dfeta_name);
                    Assert.Equal(heSubject2Value, heSubject2.dfeta_Value);
                    Assert.NotNull(heSubject3);
                    Assert.Equal(heSubject3Name, heSubject3.dfeta_name);
                    Assert.Equal(heSubject3Value, heSubject3.dfeta_Value);
                }
                else
                {
                    Assert.Null(heSubject1);
                    Assert.Null(heSubject2);
                    Assert.Null(heSubject3);
                }
            },
            item3 =>
            {
                Assert.Equal(mqQualificationId, item3.Id);
                Assert.Equal(dfeta_qualification_dfeta_Type.MandatoryQualification, item3.dfeta_Type);
                Assert.Equal(dfeta_qualificationState.Active, item3.StateCode);

                var mqSpecialism = item3.Extract<dfeta_specialism>(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute);
                if (setSpecialismColumnNames)
                {
                    Assert.NotNull(mqSpecialism);
                }
                else
                {
                    Assert.Null(mqSpecialism);
                }
            }
        );
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
