#nullable disable
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Api.IntegrationTests.V2.Operations;

public class GetOrCreateTrnRequestTests : TestBase
{
    public GetOrCreateTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: [ApiRoles.UpdatePerson])]
    public async Task Put_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestId = Guid.NewGuid().ToString();

        var request = CreateRequest();

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_ValidRequestInDbWithResolvedTrn_ReturnsOkWithCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn());

        ConfigureDataverseAdapterGetTeacher(person.Contact);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = person.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        var request = CreateRequest(req => req.RequestId = requestId);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                trn = person.Trn,
                status = "Completed",
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = (string)null
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Put_ValidRequestInCrmWithResolvedTrn_ReturnsOkWithCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithSlugId(slugId));

        ConfigureDataverseAdapterGetTeacher(person.Contact);

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(person.ContactId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = person.ContactId,
                dfeta_TRN = person.Trn,
                dfeta_SlugId = slugId
            });

        var request = CreateRequest(req => req.RequestId = requestId);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                trn = person.Trn,
                status = "Completed",
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = (string)null
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Put_ValidRequestInDbWithUnresolvedTrn_ReturnsOkWithPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithoutTrn());

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = person.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        var request = CreateRequest(req => req.RequestId = requestId);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                trn = (string)null,
                status = "Pending",
                qtsDate = (DateOnly?)null,
                potentialDuplicate = true,
                slugId = (string)null
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Put_ValidRequestInCrmWithUnresolvedTrn_ReturnsOkWithPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithoutTrn()
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithSlugId(slugId));

        ConfigureDataverseAdapterGetTeacher(person.Contact);

        var request = CreateRequest(req => req.RequestId = requestId);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                trn = (string)null,
                status = "Pending",
                qtsDate = (DateOnly?)null,
                potentialDuplicate = true,
                slugId = (string)null
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Put_InvalidRequestId_ReturnsError()
    {
        // Arrange
        var requestId = "$";

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.RequestId),
            expectedError: Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes);
    }

    [Fact]
    public async Task Put_RequestIdTooLong_ReturnsError()
    {
        // Arrange
        var requestId = new string('x', 101);  // Limit is 100

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.RequestId),
            expectedError: Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
    }

    [Theory]
    [InlineData("1234567", "Completed", false)]
    [InlineData(null, "Pending", true)]
    public async Task Put_ValidRequestWithNewId_CreatesContactRecordAndReturnsCreated(
        string trn,
        string expectedStatus,
        bool expectedPotentialDuplicate)
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, /* trnToken: */ null))
            .Verifiable();
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        var slugId = Guid.NewGuid().ToString();
        var request = CreateRequest(req => req.SlugId = slugId);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        DataverseAdapterMock.Verify();

        await AssertEx.JsonResponseEqualsAsync(
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
    public async Task Put_ValidRequestWithNullQualification_Succeeds()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, /* trnToken: */ null))
            .Verifiable();
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        var request = CreateRequest(req => req.Qualification = null);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Put_ValidRequestWithNullQualificationSubject2_Succeeds()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, /* trnToken: */ null))
            .Verifiable();
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        var request = CreateRequest(req => req.Qualification.Subject2 = null);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Put_ValidRequestWithNullQualificationSubject3_Succeeds()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, /* trnToken: */ null))
            .Verifiable();
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        var request = CreateRequest(req => req.Qualification.Subject3 = null);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Put_RequestWithInvalidQualificationSubject2_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationSubject2NotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.Subject2 = "some invalid subject"));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject2)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidQualificationSubject3_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationSubject3NotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.Subject3 = "some invalid subject"));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject3)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidIttProvider_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var ukprn = "xxx";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.IttProviderNotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));
        CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.ProviderUkprn)}",
            expectedError: Properties.StringResources.Errors_10008_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidIttSubject1_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var subject = "xxx";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.Subject1NotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.Subject1 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject1)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidIttSubject2_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var subject = "xxx";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.Subject2NotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.Subject2 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject2)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidIttQualification_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var ittQualificationType = (IttQualificationType)(-1);

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.IttQualificationNotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.InitialTeacherTraining.IttQualificationType = ittQualificationType));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_RequestWithInvalidIttCountry_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var country = "xxx";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationCountryNotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.CountryCode = country));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.CountryCode)}",
            expectedError: Properties.StringResources.Errors_10010_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidQualificationSbject_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var subject = "xxx";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationSubjectNotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject)}",
            expectedError: Properties.StringResources.Errors_10009_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidQualificationProvider_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var ukprn = "xxx";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationProviderNotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.ProviderUkprn = ukprn));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.ProviderUkprn)}",
            expectedError: Properties.StringResources.Errors_10008_Title);
    }

    [Fact]
    public async Task Put_RequestWithInvalidQualificationType_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var heQualificationType = (HeQualificationType)(-1);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.HeQualificationType = heQualificationType));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_RequestWithNotFoundQualificationType_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var heQualificationType = HeQualificationType.BachelorOfCommerce;

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Failed(CreateTeacherFailedReasons.QualificationNotFound));

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"v2/trn-requests/{requestId}",
            CreateRequest(req => req.Qualification.HeQualificationType = heQualificationType));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.HeQualificationType)}",
            expectedError: Properties.StringResources.Errors_10013_Title);
    }

    [Theory]
    [InlineData(1900, 1, 1)]
    public async Task Put_RequestWithDateOfBirthBefore01011940_ReturnsError(int year, int month, int day)
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
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            "Birthdate",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [InlineData(2022, 1, 1)]
    [InlineData(2023, 1, 1)]
    public async Task Put_RequestWithDateOfBirthInFuture_ReturnsError(int year, int month, int day)
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
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            "Birthdate",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [MemberData(nameof(InvalidAgeCombinationsData))]
    public async Task Put_RequestWithInvalidAgeRange_ReturnsError(
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
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            expectedErrorPropertyName,
            expectedErrorMessage);
    }

    [Theory]
    [InlineData("Joe Xavier", "Andre", "Joe", "Xavier Andre")]
    [InlineData("Joe Xavier", "", "Joe", "Xavier")]
    public async Task Put_ValidRequestWithMultiWordFirstName_PopulatesContactMiddlenameField(
        string firstName,
        string middleName,
        string expectedFirstName,
        string expectedMiddleName)
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, /* trnToken: */ null));
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        var request = CreateRequest(cmd =>
        {
            cmd.FirstName = firstName;
            cmd.MiddleName = middleName;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        DataverseAdapterMock
            .Verify(mock => mock.CreateTeacherAsync(It.Is<CreateTeacherCommand>(cmd => cmd.FirstName == expectedFirstName && cmd.MiddleName == expectedMiddleName)));
    }

    [Fact]
    public async Task Put_OverseasQualifiedTeacherAndEarlyYearsProgrammeType_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();

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
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_ValidRequestForOverseasQualifiedTeacher_ExecutesSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var trnToken = "ABCDEFG1234567";
        var qtsDate = new DateOnly(2020, 10, 10);

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, trnToken))
            .Verifiable();
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync(new CreateTrnTokenResponse()
            {
                Email = Faker.Internet.Email(),
                Trn = trn,
                TrnToken = trnToken,
                ExpiresUtc = DateTime.UtcNow.AddDays(1),
            });

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
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        DataverseAdapterMock.Verify();

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                trn = trn,
                status = "Completed",
                qtsDate = qtsDate,
                potentialDuplicate = false,
                slugId = (string)null,
                accessYourTeachingQualificationsLink = $"https://aytq.com/qualifications/start?trn_token={trnToken}"
            },
            expectedStatusCode: 201);
    }

    [Fact]
    public async Task Put_ValidRequestWithInternationalQualifiedTeacherStatus_ExecutesSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";

        DataverseAdapterMock
            .Setup(mock => mock.CreateTeacherAsync(It.IsAny<CreateTeacherCommand>()))
            .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn, /* trnToken: */ null))
            .Verifiable();
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(teacherId, Array.Empty<string>(), false))
            .ReturnsAsync(new Contact() { Id = teacherId });

        var request = CreateRequest(req =>
        {
            req.TeacherType = TeachingRecordSystem.Api.V2.Requests.CreateTeacherType.TraineeTeacher;
            req.InitialTeacherTraining.ProgrammeType = IttProgrammeType.InternationalQualifiedTeacherStatus;
            req.InitialTeacherTraining.IttQualificationType = IttQualificationType.InternationalQualifiedTeacherStatus;
            req.InitialTeacherTraining.TrainingCountryCode = "SC";
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        DataverseAdapterMock.Verify();
    }

    [Fact]
    public async Task Put_SlugIdTooLong_ReturnsError()
    {
        // Arrange
        var slugId = new string('x', 155);  // Limit is 150
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.SlugId = slugId;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: nameof(GetOrCreateTrnRequest.SlugId),
            expectedError: Properties.StringResources.ErrorMessages_SlugIdMustBe150CharactersOrFewer);
    }

    [Fact]
    public async Task Put_RequestForOverseasQualifiedTeacherWithSlugId_ReturnsError()
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
        var response = await GetHttpClientWithApiKey().PutAsync($"v2/trn-requests/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
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
            SlugId = null,
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
            HusId = "1234567890123"
        };

        configureRequest?.Invoke(request);

        return CreateJsonContent(request);
    }

    private void ConfigureDataverseAdapterGetTeacher(Contact contact)
    {
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherAsync(contact.Id, It.IsAny<string[]>(), /* resolveMerges: */ true))
            .ReturnsAsync(contact);
    }
}
