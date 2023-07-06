#nullable disable
using Moq;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.TestCommon;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

public class GetOrCreateTrnRequestTests : ApiTestBase
{
    public GetOrCreateTrnRequestTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Given_request_with_id_already_exists_for_client_and_status_is_completed_returns_existing_trn()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_TRN = trn
            });

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });
        var slugId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.SlugId = slugId;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                trn = trn,
                status = "Completed",
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = slugId
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Given_request_with_id_already_exists_for_client_and_status_is_pending_returns_null_trn()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });
        var slugId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.SlugId = slugId;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                trn = (string)null,
                status = "Pending",
                qtsDate = (DateOnly?)null,
                potentialDuplicate = true,
                slugId = slugId
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Given_request_with_invalid_id_returns_error()
    {
        // Arrange
        var requestId = "$";

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.RequestId),
            expectedError: Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes);
    }

    [Fact]
    public async Task Given_request_with_too_long_invalid_id_returns_error()
    {
        // Arrange
        var requestId = new string('x', 101);  // Limit is 100

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.RequestId),
            expectedError: Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
    }

    [Theory]
    [InlineData("1234567", "Completed", false)]
    [InlineData(null, "Pending", true)]
    public async Task Given_request_with_new_id_creates_teacher_and_returns_created(
        string trn,
        string expectedStatus,
        bool expectedPotentialDuplicate)
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();
        var slugId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.SlugId = slugId;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        ApiFixture.DataverseAdapter.Verify();

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                trn = trn,
                status = expectedStatus,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = expectedPotentialDuplicate,
                slugId = slugId
            },
            expectedStatusCode: 201);
    }

    [Fact]
    public async Task Given_request_with_null_qualification_passes_request_to_DataverseAdapter_successfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        var request = CreateRequest(req => req.Qualification = null);

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Given_request_with_non_existent_identityuser_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        ApiFixture.IdentityApiClient
            .Setup(mock => mock.GetUserById(It.IsAny<Guid>()))
            .ReturnsAsync(default(User))
            .Verifiable();

        var request = CreateRequest(req =>
        {
            req.IdentityUserId = Guid.NewGuid();
            req.Qualification = null;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.IdentityUserId),
            expectedError: Properties.StringResources.Errors_10022_Title);
    }

    [Fact]
    public async Task Given_request_with_valid_existent_identityuser_returns_success()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var identityUserId = Guid.NewGuid();

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        ApiFixture.IdentityApiClient
            .Setup(mock => mock.GetUserById(It.IsAny<Guid>()))
            .ReturnsAsync(new User() { UserId = identityUserId, FirstName = Faker.Name.First(), LastName = Faker.Name.Last() })
            .Verifiable();

        var request = CreateRequest(req =>
        {
            req.IdentityUserId = identityUserId;
            req.Qualification = null;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Given_request_with_null_qualification_subject2_request_to_DataverseAdapter_successfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        var request = CreateRequest(req => req.Qualification.Subject2 = null);

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }


    [Fact]
    public async Task Given_request_with_null_qualification_subject3_request_to_DataverseAdapter_successfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        var request = CreateRequest(req => req.Qualification.Subject3 = null);

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Given_invalid_qualification_subject2_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationSubject2NotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.Subject2 = "some invalid subject"));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject2)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_subject3_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationSubject3NotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.Subject3 = "some invalid subject"));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject3)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Given_invalid_itt_provider_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var ukprn = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.IttProviderNotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));
        CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.ProviderUkprn)}",
            expectedError: Properties.StringResources.Errors_10008_Title);
    }

    [Fact]
    public async Task Given_invalid_itt_subject1_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var subject = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.Subject1NotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.Subject1 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject1)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Given_invalid_itt_subject2_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var subject = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.Subject2NotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.Subject2 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject2)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Given_invalid_itt_qualification_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var ittQualificationType = (IttQualificationType)(-1);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.IttQualificationNotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.IttQualificationType = ittQualificationType));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_invalid_qualification_country_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var country = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationCountryNotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.CountryCode = country));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.CountryCode)}",
            expectedError: Properties.StringResources.Errors_10010_Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_subject_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var subject = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationSubjectNotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_provider_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var ukprn = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationProviderNotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.ProviderUkprn = ukprn));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.ProviderUkprn)}",
            expectedError: Properties.StringResources.Errors_10008_Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var heQualificationType = (HeQualificationType)(-1);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationNotFound));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.HeQualificationType = heQualificationType));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(1900, 1, 1)]
    public async Task Given_dob_before_1_1_1940_returns_error(int year, int month, int day)
    {
        // Arrange
        var dob = new DateOnly(year, month, day);
        var requestId = Guid.NewGuid().ToString();
        Clock.UtcNow = new DateTime(2022, 1, 1);

        var request = CreateRequest(cmd =>
        {
            cmd.BirthDate = dob;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Birthdate",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [InlineData(2022, 1, 1)]
    [InlineData(2023, 1, 1)]
    public async Task Given_dob_equal_or_after_today_returns_error(int year, int month, int day)
    {
        // Arrange
        var dob = new DateOnly(year, month, day);
        var requestId = Guid.NewGuid().ToString();
        Clock.UtcNow = new DateTime(2022, 1, 1);
        var request = CreateRequest(cmd =>
        {
            cmd.BirthDate = dob;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Birthdate",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [MemberData(nameof(InvalidAgeCombinationsData))]
    public async Task Given_invalid_age_combination_returns_error(
        int? ageRangeFrom,
        int? ageRangeTo,
        string expectedErrorPropertyName,
        string expectedErrorMessage)
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        var request = CreateRequest(cmd =>
        {
            cmd.InitialTeacherTraining.AgeRangeFrom = ageRangeFrom;
            cmd.InitialTeacherTraining.AgeRangeTo = ageRangeTo;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            expectedErrorPropertyName,
            expectedErrorMessage);
    }

    [Theory]
    [InlineData("Joe Xavier", "Andre", "Joe", "Xavier Andre")]
    [InlineData("Joe Xavier", "", "Joe", "Xavier")]
    public async Task Given_trainee_with_multiple_first_names_populates_middlename_field(
        string firstName,
        string middleName,
        string expectedFirstName,
        string expectedMiddleName)
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn));

        var request = CreateRequest(cmd =>
        {
            cmd.FirstName = firstName;
            cmd.MiddleName = middleName;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        ApiFixture.DataverseAdapter
            .Verify(mock => mock.CreateTeacher(It.Is<CreateTeacherCommand>(cmd => cmd.FirstName == expectedFirstName && cmd.MiddleName == expectedMiddleName)));
    }

    [Fact]
    public async Task Given_OverseasQualifiedTeacher_and_EarlyYears_ProgrammeType_returns_error()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        var request = CreateRequest(req =>
        {
            req.TeacherType = TeachingRecordSystem.Api.V2.Requests.CreateTeacherType.OverseasQualifiedTeacher;
            req.InitialTeacherTraining.ProviderUkprn = null;
            req.InitialTeacherTraining.TrainingCountryCode = "SC";
            req.InitialTeacherTraining.ProgrammeType = IttProgrammeType.EYITTAssessmentOnly;
            req.QtsDate = new DateOnly(2020, 10, 10);
            req.RecognitionRoute = TeachingRecordSystem.Api.V2.Requests.CreateTeacherRecognitionRoute.Scotland;
            req.InductionRequired = false;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_valid_OverseasQualifiedTeacher_request_passes_request_to_DataverseAdapter_successfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        var request = CreateRequest(req =>
        {
            req.TeacherType = TeachingRecordSystem.Api.V2.Requests.CreateTeacherType.OverseasQualifiedTeacher;
            req.InitialTeacherTraining.ProviderUkprn = null;
            req.InitialTeacherTraining.TrainingCountryCode = "XH";
            req.QtsDate = new DateOnly(2020, 10, 10);
            req.RecognitionRoute = TeachingRecordSystem.Api.V2.Requests.CreateTeacherRecognitionRoute.Scotland;
            req.InductionRequired = false;
            req.SlugId = null;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        ApiFixture.DataverseAdapter.Verify();
    }

    [Fact]
    public async Task Given_valid_InternationalQualifiedTeacherStatus_request_passes_request_to_DataverseAdapter_successfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
            .Verifiable();

        var request = CreateRequest(req =>
        {
            req.TeacherType = TeachingRecordSystem.Api.V2.Requests.CreateTeacherType.TraineeTeacher;
            req.InitialTeacherTraining.ProgrammeType = IttProgrammeType.InternationalQualifiedTeacherStatus;
            req.InitialTeacherTraining.IttQualificationType = IttQualificationType.InternationalQualifiedTeacherStatus;
            req.InitialTeacherTraining.TrainingCountryCode = "SC";
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        ApiFixture.DataverseAdapter.Verify();
    }

    [Fact]
    public async Task Given_request_with_too_long_invalid_slugid_returns_error()
    {
        // Arrange
        var slugId = new string('x', 155);  // Limit is 150
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.SlugId = slugId;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.SlugId),
            expectedError: Properties.StringResources.ErrorMessages_SlugIdMustBe150CharactersOrFewer);
    }

    [Fact]
    public async Task Given_request_for_non_traineeteacher_with_slugid_returns_error()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.SlugId = slugId;
            req.TeacherType = TeachingRecordSystem.Api.V2.Requests.CreateTeacherType.OverseasQualifiedTeacher;
        });

        // Act
        var response = await HttpClientWithApiKey.PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.SlugId),
            expectedError: Properties.StringResources.ErrorMessages_SlugIdCanOnlyBeProvidedForTraineeTeachers);
    }

    public static TheoryData<int?, int?, string, string> InvalidAgeCombinationsData { get; } = new()
    {
        {
            -1,
            1,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.AgeRangeFrom)}",
            StringResources.ErrorMessages_AgeMustBe0To19Inclusive
        },
        {
            1,
            -1,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.AgeRangeTo)}",
            StringResources.ErrorMessages_AgeMustBe0To19Inclusive
        },
        {
            5,
            4,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.AgeRangeTo)}",
            StringResources.ErrorMessages_AgeToCannotBeLessThanAgeFrom
        }
    };

    private JsonContent CreateRequest(Action<GetOrCreateTrnRequest> configureRequest = null)
    {
        var request = new GetOrCreateTrnRequest()
        {
            FirstName = "Minnie",
            MiddleName = "Van",
            LastName = "Ryder",
            BirthDate = new(1990, 5, 23),
            SlugId = Guid.NewGuid().ToString(),
            EmailAddress = "minnie.van.ryder@example.com",
            Address = new()
            {
                AddressLine1 = "52 Quernmore Road",
                City = "Liverpool",
                PostalCode = "L33 6UZ",
                Country = "United Kingdom"
            },
            GenderCode = Gender.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = IttProgrammeType.GraduateTeacherProgramme,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = 5,
                AgeRangeTo = 11
            },
            Qualification = new()
            {
                ProviderUkprn = "10044534",
                CountryCode = "UK",
                Subject = "100366",  // computer science
                Class = ClassDivision.FirstClassHonours,
                Date = new(2021, 5, 3),
                Subject2 = "X300", // Academic Studies in Education
                Subject3 = "N400"  // Accounting
            },
            HusId = "1234567890123",
            IdentityUserId = null
        };

        configureRequest?.Invoke(request);

        return CreateJsonContent(request);
    }
}
