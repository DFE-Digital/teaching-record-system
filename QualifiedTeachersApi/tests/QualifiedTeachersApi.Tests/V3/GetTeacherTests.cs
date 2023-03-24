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
        var middleName = Faker.Name.Middle();

        var qtsDate = new DateOnly(1997, 4, 23);
        var eytsDate = new DateOnly(1995, 5, 14);
        var inductionStartDate = new DateOnly(1996, 2, 3);
        var inductionEndDate = new DateOnly(1996, 6, 7);
        var inductionStatus = dfeta_InductionStatus.Pass;
        var inductionPeriodStartDate = new DateOnly(1996, 2, 3);
        var inductionPeriodEndDate = new DateOnly(1996, 6, 7);
        var inductionPeriodTerms = 3;
        var inductionPeriodAppropriateBodyName = "My appropriate body";
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
        var mandatoryQualificationValidSpecialismName = "Hearing";
        var mandatoryQualificationNoAwardDateSpecialismName = "Visual Impairment";
        var mandatoryQualificationInactiveSpecialismName = "Mutli Sensory Impairment";

        var contact = new Contact()
        {
            Id = teacherId,
            dfeta_TRN = trn,
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            dfeta_QTSDate = qtsDate.ToDateTime(),
            dfeta_EYTSDate = eytsDate.ToDateTime(),
        };

        var inductionPeriod = new dfeta_inductionperiod()
        {
            dfeta_StartDate = inductionPeriodStartDate.ToDateTime(),
            dfeta_EndDate = inductionPeriodEndDate.ToDateTime(),
            dfeta_Numberofterms = inductionPeriodTerms
        };

        inductionPeriod.Attributes.Add($"appropriatebody.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        inductionPeriod.Attributes.Add($"appropriatebody.{Account.Fields.Name}", new AliasedValue(Account.EntityLogicalName, Account.Fields.Name, inductionPeriodAppropriateBodyName));

        var induction = new dfeta_induction()
        {
            dfeta_StartDate = inductionStartDate.ToDateTime(),
            dfeta_CompletionDate = inductionEndDate.ToDateTime(),
            dfeta_InductionStatus = inductionStatus
        };

        var inductionPeriods = new[]
        {
            inductionPeriod
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

        var npqQualificationNoAwardDate = new dfeta_qualification()
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.NPQLL,
            dfeta_CompletionorAwardDate = null,
            StateCode = dfeta_qualificationState.Active
        };

        var npqQualificationInactive = new dfeta_qualification()
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.NPQSL,
            dfeta_CompletionorAwardDate = new DateTime(2022, 5, 6),
            StateCode = dfeta_qualificationState.Inactive
        };

        var npqQualificationValid = new dfeta_qualification()
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.NPQEYL,
            dfeta_CompletionorAwardDate = new DateTime(2022, 3, 4),
            StateCode = dfeta_qualificationState.Active
        };

        var mandatoryQualificationNoAwardDate = new dfeta_qualification
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_MQ_Date = null,
            StateCode = dfeta_qualificationState.Active
        };

        mandatoryQualificationNoAwardDate.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.PrimaryIdAttribute}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute, Guid.NewGuid()));
        mandatoryQualificationNoAwardDate.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.Fields.dfeta_name, mandatoryQualificationNoAwardDateSpecialismName));

        var mandatoryQualificationNoSpecialism = new dfeta_qualification
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_MQ_Date = new DateTime(2023, 2, 3),
            StateCode = dfeta_qualificationState.Active
        };

        var mandatoryQualificationValid = new dfeta_qualification
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_MQ_Date = new DateTime(2022, 4, 6),
            StateCode = dfeta_qualificationState.Active
        };

        mandatoryQualificationValid.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.PrimaryIdAttribute}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute, Guid.NewGuid()));
        mandatoryQualificationValid.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.Fields.dfeta_name, mandatoryQualificationValidSpecialismName));

        var mandatoryQualificationInactive = new dfeta_qualification
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_MQ_Date = new DateTime(2022, 4, 8),
            StateCode = dfeta_qualificationState.Inactive
        };

        mandatoryQualificationInactive.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.PrimaryIdAttribute}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute, Guid.NewGuid()));
        mandatoryQualificationInactive.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.Fields.dfeta_name, mandatoryQualificationInactiveSpecialismName));

        var qualifications = new dfeta_qualification[]
        {
            npqQualificationNoAwardDate,
            npqQualificationInactive,
            npqQualificationValid,
            mandatoryQualificationNoAwardDate,
            mandatoryQualificationNoSpecialism,
            mandatoryQualificationValid,
            mandatoryQualificationInactive
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
            .Setup(mock => mock.GetInductionByTeacher(
                teacherId,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>()))
            .ReturnsAsync((induction, inductionPeriods));

        ApiFixture.DataverseAdapter
             .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(
                 teacherId,
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 false))
             .ReturnsAsync(new[] { itt });

        ApiFixture.DataverseAdapter
             .Setup(mock => mock.GetQualificationsForTeacher(
                 teacherId,
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>()))
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
                middleName = middleName,
                trn = trn,
                qts = new
                {
                    awarded = qtsDate.ToString("yyyy-MM-dd"),
                    certificateUrl = "/v3/certificates/qts"
                },
                eyts = new
                {
                    awarded = eytsDate.ToString("yyyy-MM-dd"),
                    certificateUrl = "/v3/certificates/eyts"
                },
                induction = new
                {
                    startDate = inductionStartDate.ToString("yyyy-MM-dd"),
                    endDate = inductionEndDate.ToString("yyyy-MM-dd"),
                    status = inductionStatus.ToString(),
                    certificateUrl = "/v3/certificates/induction",
                    periods = new[]
                    {
                        new
                        {
                            startDate = inductionPeriodStartDate.ToString("yyyy-MM-dd"),
                            endDate = inductionPeriodEndDate.ToString("yyyy-MM-dd"),
                            terms = inductionPeriodTerms,
                            appropriateBody = new
                            {
                                name = inductionPeriodAppropriateBodyName
                            }
                        }
                    }
                },
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
                        awarded = npqQualificationValid.dfeta_CompletionorAwardDate.Value.ToString("yyyy-MM-dd"),
                        type = new
                        {
                            code = npqQualificationValid.dfeta_Type.ToString(),
                            name = npqQualificationValid.dfeta_Type.Value.GetName()
                        },
                        certificateUrl = $"/v3/certificates/npq/{npqQualificationValid.Id}"
                    }
                },
                mandatoryQualifications = new[]
                {
                    new
                    {
                        awarded = mandatoryQualificationValid.dfeta_MQ_Date.Value.ToString("yyyy-MM-dd"),
                        specialism = mandatoryQualificationValidSpecialismName
                    }
                }
            },
            StatusCodes.Status200OK);
    }
}
