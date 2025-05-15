using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.V20240920.Requests;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240920;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson, ApiRoles.AppropriateBody])]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(GetPersonRequestIncludes.NpqQualifications)]
    [InlineData(GetPersonRequestIncludes.MandatoryQualifications)]
    [InlineData(GetPersonRequestIncludes.PendingDetailChanges)]
    [InlineData(GetPersonRequestIncludes.HigherEducationQualifications)]
    [InlineData(GetPersonRequestIncludes.PreviousNames)]
    [InlineData(GetPersonRequestIncludes._AllowIdSignInWithProhibitions)]
    public async Task Get_AsAppropriateBodyWithNotPermittedInclude_ReturnsForbidden(GetPersonRequestIncludes include)
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include={Uri.EscapeDataString(include.ToString())}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(GetPersonRequestIncludes.Induction)]
    [InlineData(GetPersonRequestIncludes.Alerts)]
    [InlineData(GetPersonRequestIncludes.InitialTeacherTraining)]
    public async Task Get_AsAppropriateBodyWithPermittedInclude_ReturnsOk(GetPersonRequestIncludes include)
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include={Uri.EscapeDataString(include.ToString())}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAppropriateBodyWithoutDateOfBirth_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedAlertsContent()
    {
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => !at.InternalOnly).RandomOne();

        var person = await TestData.CreatePersonAsync(x => x
            .WithTrn()
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Alerts");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    alertId = alert.AlertId,
                    alertType = new
                    {
                        alertTypeId = alert.AlertType!.AlertTypeId,
                        name = alert.AlertType.Name,
                        alertCategory = new
                        {
                            alertCategoryId = alert.AlertType.AlertCategory!.AlertCategoryId,
                            name = alert.AlertType.AlertCategory.Name
                        }
                    },
                    details = alert.Details,
                    startDate = alert.StartDate,
                    endDate = alert.EndDate,
                }
            },
            responseAlerts);
    }

    [Fact]
    public async Task Get_AsAppropriateBodyWithItt_ReturnsIttProviders()
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var ittProviderUkprn = "12345";
        var ittProviderName = Faker.Company.Name();
        var itt = CreateIttEntity(person.ContactId, ittProviderUkprn, ittProviderName);

        DataverseAdapterMock
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacherAsync(
                person.ContactId,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                /*activeOnly: */ true))
            .ReturnsAsync([itt]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include=InitialTeacherTraining");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseItt = jsonResponse.RootElement.GetProperty("initialTeacherTraining");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    provider = new
                    {
                        ukprn = ittProviderUkprn,
                        name = ittProviderName
                    }
                }
            },
            responseItt);
    }

    [Fact]
    public async Task Get_WithItt_ReturnsExpectedItt()
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var ittProviderUkprn = "12345";
        var ittProviderName = Faker.Company.Name();
        var itt = CreateIttEntity(person.ContactId, ittProviderUkprn, ittProviderName);

        DataverseAdapterMock
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacherAsync(
                person.ContactId,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                /*activeOnly: */ true))
            .ReturnsAsync([itt]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=InitialTeacherTraining");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseItt = jsonResponse.RootElement.GetProperty("initialTeacherTraining");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    qualification = new
                    {
                        name = itt.GetAttributeValue<AliasedValue>($"qualification.{dfeta_ittqualification.Fields.dfeta_name}").Value
                    },
                    programmeType = itt.dfeta_ProgrammeType.ToString(),
                    programmeTypeDescription = itt.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>().GetDescription(),
                    startDate = itt.dfeta_ProgrammeStartDate?.ToString("yyyy-MM-dd"),
                    endDate = itt.dfeta_ProgrammeEndDate?.ToString("yyyy-MM-dd"),
                    result = itt.dfeta_Result?.ToString(),
                    ageRange = new
                    {
                        description = "11 to 16 years"
                    },
                    provider = new
                    {
                        ukprn = ittProviderUkprn,
                        name = ittProviderName
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
            responseItt);
    }

    private static dfeta_initialteachertraining CreateIttEntity(Guid contactId, string ittProviderUkprn, string ittProviderName)
    {
        var ittStartDate = new DateOnly(2021, 9, 7);
        var ittEndDate = new DateOnly(2022, 7, 29);
        var ittProgrammeType = dfeta_ITTProgrammeType.Core;
        var ittResult = dfeta_ITTResult.Pass;
        var ittAgeRangeFrom = dfeta_AgeRange._11;
        var ittAgeRangeTo = dfeta_AgeRange._16;
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
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, contactId),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType,
            dfeta_Result = ittResult,
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

        return itt;
    }
}
