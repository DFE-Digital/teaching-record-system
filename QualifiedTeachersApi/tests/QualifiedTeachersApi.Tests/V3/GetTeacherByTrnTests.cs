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

public class GetTeacherByTrnTests : ApiTestBase
{
    public GetTeacherByTrnTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_Returns401()
    {
        // Arrange
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await ApiFixture.CreateClient().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_InvalidTrn_Returns400()
    {
        // Arrange
        var trn = "invalid";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnNotFound_Returns404()
    {
        // Arrange
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }


    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var trn = "1234567";

        var teacher = GenerateTeacher(trn);
        var itt = GenerateItt(teacher);

        var induction = GenerateInduction();
        var inductionPeriods = GenerateInductionPeriods();

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetInductionByTeacher(
                teacher.Id,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>()))
            .ReturnsAsync((induction, inductionPeriods));

        var npqQualificationNoAwardDate = GenerateQualification(dfeta_qualification_dfeta_Type.NPQLL, null, dfeta_qualificationState.Active, null);
        var npqQualificationInactive = GenerateQualification(dfeta_qualification_dfeta_Type.NPQSL, new DateTime(2022, 5, 6), dfeta_qualificationState.Inactive, null);
        var npqQualificationValid = GenerateQualification(dfeta_qualification_dfeta_Type.NPQEYL, new DateTime(2022, 3, 4), dfeta_qualificationState.Active, null);
        var mandatoryQualificationNoAwardDate = GenerateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, null, dfeta_qualificationState.Active, "Visual Impairment");
        var mandatoryQualificationNoSpecialism = GenerateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, new DateTime(2022, 2, 3), dfeta_qualificationState.Active, null);
        var mandatoryQualificationValid = GenerateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, new DateTime(2022, 4, 6), dfeta_qualificationState.Active, "Hearing");
        var mandatoryQualificationInactive = GenerateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, new DateTime(2022, 4, 8), dfeta_qualificationState.Inactive, "Mutli Sensory Impairment");

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

        ApiFixture.DataverseAdapter
             .Setup(mock => mock.GetQualificationsForTeacher(
                 teacher.Id,
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>()))
             .ReturnsAsync(qualifications);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                firstName = teacher.FirstName,
                lastName = teacher.LastName,
                middleName = teacher.MiddleName,
                trn = trn,
                dateOfBirth = teacher.BirthDate?.ToString("yyyy-MM-dd"),
                nationalInsuranceNumber = teacher.dfeta_NINumber,
                qts = new
                {
                    awarded = teacher.dfeta_QTSDate?.ToString("yyyy-MM-dd"),
                    certificateUrl = "/v3/certificates/qts"
                },
                eyts = new
                {
                    awarded = teacher.dfeta_EYTSDate?.ToString("yyyy-MM-dd"),
                    certificateUrl = "/v3/certificates/eyts"
                },
                induction = new
                {
                    startDate = induction.dfeta_StartDate?.ToString("yyyy-MM-dd"),
                    endDate = induction.dfeta_CompletionDate?.ToString("yyyy-MM-dd"),
                    status = induction.dfeta_InductionStatus.ToString(),
                    certificateUrl = "/v3/certificates/induction",
                    periods = new[]
                    {
                        new
                        {
                            startDate = inductionPeriods[0].dfeta_StartDate?.ToString("yyyy-MM-dd"),
                            endDate = inductionPeriods[0].dfeta_EndDate?.ToString("yyyy-MM-dd"),
                            terms = inductionPeriods[0].dfeta_Numberofterms,
                            appropriateBody = new
                            {
                                name = inductionPeriods[0].GetAttributeValue<AliasedValue>($"appropriatebody.{Account.Fields.Name}").Value
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
                            name = itt.GetAttributeValue<AliasedValue>($"qualification.{dfeta_ittqualification.Fields.dfeta_name}").Value
                        },
                        programmeType = itt.dfeta_ProgrammeType.ToString(),
                        startDate = itt.dfeta_ProgrammeStartDate?.ToString("yyyy-MM-dd"),
                        endDate = itt.dfeta_ProgrammeEndDate?.ToString("yyyy-MM-dd"),
                        result = itt.dfeta_Result?.ToString(),
                        ageRange = new
                        {
                            description = "11 to 16 years"
                        },
                        provider = new
                        {
                            name = itt.GetAttributeValue<AliasedValue>($"establishment.{Account.Fields.Name}").Value,
                            ukprn = itt.GetAttributeValue<AliasedValue>($"establishment.{Account.Fields.dfeta_UKPRN}").Value
                        },
                        subjects = new[]
                        {
                            new
                            {
                                code = itt.GetAttributeValue<AliasedValue>($"subject1.{dfeta_ittsubject.Fields.dfeta_Value}").Value,
                                name = itt.GetAttributeValue<AliasedValue>($"subject1.{dfeta_ittsubject.Fields.dfeta_name}").Value
                            },
                            new
                            {
                                code = itt.GetAttributeValue<AliasedValue>($"subject2.{dfeta_ittsubject.Fields.dfeta_Value}").Value,
                                name = itt.GetAttributeValue<AliasedValue>($"subject2.{dfeta_ittsubject.Fields.dfeta_name}").Value
                            },
                            new
                            {
                                code = itt.GetAttributeValue<AliasedValue>($"subject3.{dfeta_ittsubject.Fields.dfeta_Value}").Value,
                                name = itt.GetAttributeValue<AliasedValue>($"subject3.{dfeta_ittsubject.Fields.dfeta_name}").Value
                            }
                        }
                    }
                },
                npqQualifications = new[]
                {
                    new
                    {
                        awarded = npqQualificationValid.dfeta_CompletionorAwardDate?.ToString("yyyy-MM-dd"),
                        type = new
                        {
                            code = npqQualificationValid.dfeta_Type.ToString(),
                            name = npqQualificationValid.dfeta_Type?.GetName()
                        },
                        certificateUrl = $"/v3/certificates/npq/{npqQualificationValid.Id}"
                    }
                },
                mandatoryQualifications = new[]
                {
                    new
                    {
                        awarded = mandatoryQualificationValid.dfeta_MQ_Date?.ToString("yyyy-MM-dd"),
                        specialism = mandatoryQualificationValid.GetAttributeValue<AliasedValue>($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}").Value
                    }
                }
            },
            StatusCodes.Status200OK);
    }

    private Contact GenerateTeacher(string trn)
    {
        var teacherId = Guid.NewGuid();
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var middleName = Faker.Name.Middle();
        var dateOfBirth = Faker.Identification.DateOfBirth().ToDateOnly();
        var nino = Faker.Identification.UkNationalInsuranceNumber();

        var qtsDate = new DateOnly(1997, 4, 23);
        var eytsDate = new DateOnly(1995, 5, 14);

        var teacher = new Contact()
        {
            Id = teacherId,
            dfeta_TRN = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTime(),
            dfeta_NINumber = nino,
            dfeta_QTSDate = qtsDate.ToDateTime(),
            dfeta_EYTSDate = eytsDate.ToDateTime(),
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(teacher);

        return teacher;
    }

    private dfeta_initialteachertraining GenerateItt(Contact teacher)
    {
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

        var itt = new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacher.Id),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType.ConvertToIttProgrammeType(),
            dfeta_Result = ittResult.ConvertToITTResult(),
            dfeta_AgeRangeFrom = ittAgeRangeFrom,
            dfeta_AgeRangeTo = ittAgeRangeTo,
            dfeta_TraineeID = ittTraineeId,
            StateCode = dfeta_initialteachertrainingState.Active
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
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(
                teacher.Id,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                false))
            .ReturnsAsync(new[] { itt });

        return itt;
    }

    private dfeta_induction GenerateInduction()
    {
        var inductionStartDate = new DateOnly(1996, 2, 3);
        var inductionEndDate = new DateOnly(1996, 6, 7);
        var inductionStatus = dfeta_InductionStatus.Pass;

        return new dfeta_induction()
        {
            dfeta_StartDate = inductionStartDate.ToDateTime(),
            dfeta_CompletionDate = inductionEndDate.ToDateTime(),
            dfeta_InductionStatus = inductionStatus
        };
    }

    private dfeta_inductionperiod[] GenerateInductionPeriods()
    {
        var inductionPeriodStartDate = new DateOnly(1996, 2, 3);
        var inductionPeriodEndDate = new DateOnly(1996, 6, 7);
        var inductionPeriodTerms = 3;
        var inductionPeriodAppropriateBodyName = "My appropriate body";

        var inductionPeriod = new dfeta_inductionperiod()
        {
            dfeta_StartDate = inductionPeriodStartDate.ToDateTime(),
            dfeta_EndDate = inductionPeriodEndDate.ToDateTime(),
            dfeta_Numberofterms = inductionPeriodTerms
        };

        inductionPeriod.Attributes.Add($"appropriatebody.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        inductionPeriod.Attributes.Add($"appropriatebody.{Account.Fields.Name}", new AliasedValue(Account.EntityLogicalName, Account.Fields.Name, inductionPeriodAppropriateBodyName));

        return new[]
        {
            inductionPeriod
        };
    }

    private dfeta_qualification GenerateQualification(
        dfeta_qualification_dfeta_Type type,
        DateTime? date,
        dfeta_qualificationState state,
        string specialismName)
    {
        var qualification = new dfeta_qualification
        {
            Id = Guid.NewGuid(),
            dfeta_Type = type,
            StateCode = state
        };

        if (type == dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            qualification.dfeta_MQ_Date = date;
        }
        else
        {
            qualification.dfeta_CompletionorAwardDate = date;
        }

        if (specialismName is not null)
        {
            qualification.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.PrimaryIdAttribute}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute, Guid.NewGuid()));
            qualification.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.Fields.dfeta_name, specialismName));
        }

        return qualification;
    }
}
