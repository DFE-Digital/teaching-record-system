#nullable disable
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Xrm.Sdk;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.Properties;
using QualifiedTeachersApi.TestCommon;
using QualifiedTeachersApi.V2.ApiModels;
using Xunit;

namespace QualifiedTeachersApi.Tests.V2.Operations;

public class GetTeacherTests : ApiTestBase
{
    public GetTeacherTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("xxx")]
    public async Task Given_invalid_trn_returns_error(string trn)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
    }

    [Fact]
    public async Task Given_no_match_found_returns_notfound()
    {
        // Arrange
        var trn = "1234567";


        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, It.IsAny<string[]>(), true))
            .ReturnsAsync((Contact)null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_match_returns_ok_and_returns_active_and_inactive_itt_records()
    {
        // Arrange
        var trn = "1234567";
        var birthDate = "1990-04-01";
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var middleName = Faker.Name.Middle();
        var nino = Faker.Identification.UkNationalInsuranceNumber();
        var qtsDate = (DateOnly?)null;
        var eytsDate = (DateOnly?)new DateOnly(2022, 7, 7);
        var teacherId = Guid.NewGuid();
        var earlyYearsStatusId = Guid.NewGuid();
        var earlyYearsStatusName = "Early Years Teacher Status";
        var earlyYearsStatusValue = "221";
        var ittStartDate = new DateOnly(2021, 9, 7);
        var ittEndDate = new DateOnly(2022, 7, 29);
        var ittProgrammeType = IttProgrammeType.EYITTGraduateEntry;
        var ittResult = IttOutcome.Pass;
        var ittProviderUkprn = "12345";
        var ittTraineeId = "54321";
        var inactiveittTraineeId = "444444";
        var husId = "987654";

        var contact = new Contact()
        {
            Id = teacherId,
            BirthDate = DateTime.Parse(birthDate),
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            dfeta_TRN = trn,
            dfeta_NINumber = nino,
            StateCode = ContactState.Active,
            dfeta_EYTSDate = eytsDate.ToDateTime(),
            dfeta_HUSID = husId
        };

        var qtsRegistration = new dfeta_qtsregistration()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_EarlyYearsStatusId = new Microsoft.Xrm.Sdk.EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, earlyYearsStatusId),
            dfeta_EYTSDate = eytsDate.ToDateTime()
        };

        var earlyYearsStatus = new dfeta_earlyyearsstatus()
        {
            Id = earlyYearsStatusId,
            dfeta_name = earlyYearsStatusName,
            dfeta_Value = earlyYearsStatusValue
        };

        var itt = new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType.ConvertToIttProgrammeType(),
            dfeta_Result = ittResult.ConvertToITTResult(),
            dfeta_TraineeID = ittTraineeId,
            StateCode = dfeta_initialteachertrainingState.Active
        };
        var inActiveItt = new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType.ConvertToIttProgrammeType(),
            dfeta_Result = ittResult.ConvertToITTResult(),
            dfeta_TraineeID = inactiveittTraineeId,
            StateCode = dfeta_initialteachertrainingState.Inactive
        };
        itt.Attributes.Add($"establishment.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"establishment.{Account.Fields.dfeta_UKPRN}", new AliasedValue(Account.EntityLogicalName, Account.Fields.dfeta_UKPRN, ittProviderUkprn));

        inActiveItt.Attributes.Add($"establishment.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        inActiveItt.Attributes.Add($"establishment.{Account.Fields.dfeta_UKPRN}", new AliasedValue(Account.EntityLogicalName, Account.Fields.dfeta_UKPRN, ittProviderUkprn));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(contact);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetQtsRegistrationsByTeacher(teacherId, /* columnNames: */ It.IsAny<string[]>()))
            .ReturnsAsync(new[] { qtsRegistration });

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetEarlyYearsStatus(earlyYearsStatusId))
            .ReturnsAsync(earlyYearsStatus);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(
                teacherId,
                /* columnNames: */ It.IsAny<string[]>(),
                /*establishmentColumnNames: */It.IsAny<string[]>(),
                /*subjectColumnNames: */It.IsAny<string[]>(),
                /*qualificationColumnNames: */It.IsAny<string[]>(),
                /*activeOnly */ false))
            .ReturnsAsync(new[] { itt, inActiveItt });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}?includeInactive=true");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                trn = trn,
                firstName = firstName,
                lastName = lastName,
                middleName = middleName,
                dateOfBirth = birthDate,
                nationalInsuranceNumber = nino,
                hasActiveSanctions = false,
                qtsDate = qtsDate?.ToString("yyyy-MM-dd"),
                eytsDate = eytsDate?.ToString("yyyy-MM-dd"),
                husId = husId,
                earlyYearsStatus = new
                {
                    name = earlyYearsStatusName,
                    value = earlyYearsStatusValue
                },
                initialTeacherTraining = new[]
                {

                    new
                    {
                        programmeStartDate = ittStartDate.ToString("yyyy-MM-dd"),
                        programmeEndDate = ittEndDate.ToString("yyyy-MM-dd"),
                        programmeType = ittProgrammeType.ToString(),
                        result = ittResult.ToString(),
                        provider = new
                        {
                            ukprn = ittProviderUkprn
                        },
                        husId = ittTraineeId,
                        active = true
                    },
                    new
                    {
                        programmeStartDate = ittStartDate.ToString("yyyy-MM-dd"),
                        programmeEndDate = ittEndDate.ToString("yyyy-MM-dd"),
                        programmeType = ittProgrammeType.ToString(),
                        result = ittResult.ToString(),
                        provider = new
                        {
                            ukprn = ittProviderUkprn
                        },
                        husId = inactiveittTraineeId,
                        active = false
                    },
                }
            });
    }

    [Fact]
    public async Task Given_match_returns_ok()
    {
        // Arrange
        var trn = "1234567";
        var birthDate = "1990-04-01";
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var middleName = Faker.Name.Middle();
        var nino = Faker.Identification.UkNationalInsuranceNumber();
        var qtsDate = (DateOnly?)null;
        var eytsDate = (DateOnly?)new DateOnly(2022, 7, 7);
        var teacherId = Guid.NewGuid();
        var earlyYearsStatusId = Guid.NewGuid();
        var earlyYearsStatusName = "Early Years Teacher Status";
        var earlyYearsStatusValue = "221";
        var ittStartDate = new DateOnly(2021, 9, 7);
        var ittEndDate = new DateOnly(2022, 7, 29);
        var ittProgrammeType = IttProgrammeType.EYITTGraduateEntry;
        var ittResult = IttOutcome.Pass;
        var ittProviderUkprn = "12345";
        var ittTraineeId = "54321";
        var husId = "987654";

        var contact = new Contact()
        {
            Id = teacherId,
            BirthDate = DateTime.Parse(birthDate),
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            dfeta_TRN = trn,
            dfeta_NINumber = nino,
            StateCode = ContactState.Active,
            dfeta_EYTSDate = eytsDate.ToDateTime(),
            dfeta_HUSID = husId
        };

        var qtsRegistration = new dfeta_qtsregistration()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_EarlyYearsStatusId = new Microsoft.Xrm.Sdk.EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, earlyYearsStatusId),
            dfeta_EYTSDate = eytsDate.ToDateTime()
        };

        var earlyYearsStatus = new dfeta_earlyyearsstatus()
        {
            Id = earlyYearsStatusId,
            dfeta_name = earlyYearsStatusName,
            dfeta_Value = earlyYearsStatusValue
        };

        var itt = new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType.ConvertToIttProgrammeType(),
            dfeta_Result = ittResult.ConvertToITTResult(),
            dfeta_TraineeID = ittTraineeId,
            StateCode = dfeta_initialteachertrainingState.Active
        };
        itt.Attributes.Add($"establishment.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"establishment.{Account.Fields.dfeta_UKPRN}", new AliasedValue(Account.EntityLogicalName, Account.Fields.dfeta_UKPRN, ittProviderUkprn));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(contact);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetQtsRegistrationsByTeacher(teacherId, /* columnNames: */ It.IsAny<string[]>()))
            .ReturnsAsync(new[] { qtsRegistration });

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetEarlyYearsStatus(earlyYearsStatusId))
            .ReturnsAsync(earlyYearsStatus);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(
                teacherId,
                /* columnNames: */ It.IsAny<string[]>(),
                /*establishmentColumnNames: */It.IsAny<string[]>(),
                /*subjectColumnNames: */It.IsAny<string[]>(),
                /*qualificationColumnNames: */It.IsAny<string[]>(),
                /*includeinactive */ It.IsAny<bool>()))
            .ReturnsAsync(new[] { itt });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                trn = trn,
                firstName = firstName,
                lastName = lastName,
                middleName = middleName,
                dateOfBirth = birthDate,
                nationalInsuranceNumber = nino,
                hasActiveSanctions = false,
                qtsDate = qtsDate?.ToString("yyyy-MM-dd"),
                eytsDate = eytsDate?.ToString("yyyy-MM-dd"),
                husId = husId,
                earlyYearsStatus = new
                {
                    name = earlyYearsStatusName,
                    value = earlyYearsStatusValue
                },
                initialTeacherTraining = new[]
                {
                    new
                    {
                        programmeStartDate = ittStartDate.ToString("yyyy-MM-dd"),
                        programmeEndDate = ittEndDate.ToString("yyyy-MM-dd"),
                        programmeType = ittProgrammeType.ToString(),
                        result = ittResult.ToString(),
                        provider = new
                        {
                            ukprn = ittProviderUkprn
                        },
                        husId = ittTraineeId,
                        active = true
                    }
                }
            });
    }
}
