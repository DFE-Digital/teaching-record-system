using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Xrm.Sdk;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.TestCommon;
using QualifiedTeachersApi.V3.ApiModels;
using Xunit;

namespace QualifiedTeachersApi.Tests.V3;

public class GetTeacherTests : ApiTestBase
{
    public GetTeacherTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_TeacherWithTrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/teacher");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var teacherId = Guid.NewGuid();
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1997, 4, 23);
        var ittStartDate = new DateOnly(2021, 9, 7);
        var ittEndDate = new DateOnly(2022, 7, 29);
        var ittProgrammeType = IttProgrammeType.EYITTGraduateEntry;
        var ittResult = IttOutcome.Pass;
        var ittAgeRangeFrom = dfeta_AgeRange._11;
        var ittAgeRangeTo = dfeta_AgeRange._16;
        var ittProviderName = Faker.Company.Name();
        var ittProviderUkprn = "12345";
        var ittTraineeId = "54321";
        var ittSubject1Value = "12345";
        var ittSubject1Name = "Subject 1";
        var ittSubject2Value = "23456";
        var ittSubject2Name = "Subject 2";
        var ittSubject3Value = "34567";
        var ittSubject3Name = "Subject 3";
        var ittQualificationName = "My test qualification 123";
        var npqQualification1Type = dfeta_qualification_dfeta_Type.NPQEYL;
        var npqQualification1AwardDate = new DateOnly(2022, 3, 4);
        var npqQualification1Status = dfeta_qualificationState.Active;
        var npqQualification2Type = dfeta_qualification_dfeta_Type.NPQLL;
        DateOnly? npqQualification2AwardDate = null;
        var npqQualification2Status = dfeta_qualificationState.Active;
        var npqQualification3Type = dfeta_qualification_dfeta_Type.NPQSL;
        var npqQualification3AwardDate = new DateOnly(2022, 3, 4);
        var npqQualification3Status = dfeta_qualificationState.Inactive;

        var contact = new Contact()
        {
            Id = teacherId,
            dfeta_TRN = trn,
            FirstName = firstName,
            LastName = lastName,
            dfeta_QTSDate = qtsDate.ToDateTime()
        };

        var itt = new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType.ConvertToIttProgrammeType(),
            dfeta_Result = ittResult.ConvertToITTResult(),
            dfeta_AgeRangeFrom = ittAgeRangeFrom,
            dfeta_AgeRangeTo = ittAgeRangeTo,
            dfeta_TraineeID = ittTraineeId,
            StateCode = dfeta_initialteachertrainingState.Active
        };

        var qualifications = new dfeta_qualification[]
        {
            new dfeta_qualification()
            {
                dfeta_Type = npqQualification1Type,
                dfeta_CompletionorAwardDate = npqQualification1AwardDate.ToDateTime(),
                StateCode = npqQualification1Status                
            },
            new dfeta_qualification()
            {
                dfeta_Type = npqQualification2Type,
                dfeta_CompletionorAwardDate = npqQualification2AwardDate?.ToDateTime(),
                StateCode = npqQualification2Status
            },
            new dfeta_qualification()
            {
                dfeta_Type = npqQualification3Type,
                dfeta_CompletionorAwardDate = npqQualification3AwardDate.ToDateTime(),
                StateCode = npqQualification3Status
            }
        };

        itt.Attributes.Add($"qualification.{dfeta_ittqualification.PrimaryIdAttribute}", new AliasedValue(dfeta_ittqualification.EntityLogicalName, dfeta_ittqualification.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"qualification.{dfeta_ittqualification.Fields.dfeta_name}", new AliasedValue(dfeta_ittqualification.EntityLogicalName, dfeta_ittqualification.Fields.dfeta_name, ittQualificationName));
        itt.Attributes.Add($"establishment.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"establishment.{Account.Fields.Name}", new AliasedValue(Account.EntityLogicalName, Account.Fields.Name, ittProviderName));
        itt.Attributes.Add($"establishment.{Account.Fields.dfeta_UKPRN}", new AliasedValue(Account.EntityLogicalName, Account.Fields.dfeta_UKPRN, ittProviderUkprn));
        itt.Attributes.Add($"subject1.{dfeta_ittsubject.PrimaryIdAttribute}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"subject1.{dfeta_ittsubject.Fields.dfeta_Value}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, ittSubject1Value));
        itt.Attributes.Add($"subject1.{dfeta_ittsubject.Fields.dfeta_name}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_name, ittSubject1Name));
        itt.Attributes.Add($"subject2.{dfeta_ittsubject.PrimaryIdAttribute}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"subject2.{dfeta_ittsubject.Fields.dfeta_Value}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, ittSubject2Value));
        itt.Attributes.Add($"subject2.{dfeta_ittsubject.Fields.dfeta_name}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_name, ittSubject2Name));
        itt.Attributes.Add($"subject3.{dfeta_ittsubject.PrimaryIdAttribute}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"subject3.{dfeta_ittsubject.Fields.dfeta_Value}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, ittSubject3Value));
        itt.Attributes.Add($"subject3.{dfeta_ittsubject.Fields.dfeta_name}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_name, ittSubject3Name));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(contact);

        ApiFixture.DataverseAdapter
             .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(
                 teacherId,
                 /* columnNames: */ It.IsAny<string[]>(),
                 /*establishmentColumnNames: */It.IsAny<string[]>(),
                 /*subjectColumnNames: */It.IsAny<string[]>(),
                 /*qualificationColumnNames: */It.IsAny<string[]>(),
                 /*activeOnly */ false))
             .ReturnsAsync(new[] { itt });

        ApiFixture.DataverseAdapter
             .Setup(mock => mock.GetQualificationsForTeacher(
                 teacherId,
                 /* columnNames: */ It.IsAny<string[]>(),
                 /*heQualificationColumnNames: */It.IsAny<string[]>(),
                 /*subjectColumnNames: */It.IsAny<string[]>()))
             .ReturnsAsync(qualifications);

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/teacher");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                firstName = firstName,
                lastName = lastName,
                trn = trn,
                qtsDate = qtsDate.ToString("yyyy-MM-dd"),
                initialTeacherTraining = new[]
                {
                    new
                    {
                        qualification = new
                        {
                            name = ittQualificationName
                        },
                        programmeType = ittProgrammeType.ToString(),
                        startDate = ittStartDate.ToString("yyyy-MM-dd"),
                        endDate = ittEndDate.ToString("yyyy-MM-dd"),
                        result = ittResult.ToString(),
                        ageRange = new
                        {
                            description = "11 to 16 years"
                        },
                        provider = new
                        {
                            name = ittProviderName,
                            ukprn = ittProviderUkprn
                        },
                        subjects = new[]
                        {
                            new
                            {
                                code = ittSubject1Value,
                                name = ittSubject1Name
                            },
                            new
                            {
                                code = ittSubject2Value,
                                name = ittSubject2Name
                            },
                            new
                            {
                                code = ittSubject3Value,
                                name = ittSubject3Name
                            }
                        }
                    }
                },
                npqQualifications = new[]
                {
                    new
                    {
                        awarded = npqQualification1AwardDate.ToString("yyyy-MM-dd"),
                        type = new
                        {
                            code = npqQualification1Type.ToString(),
                            name = npqQualification1Type.GetName()
                        }
                    }
                }
            },
            StatusCodes.Status200OK);
    }
}
