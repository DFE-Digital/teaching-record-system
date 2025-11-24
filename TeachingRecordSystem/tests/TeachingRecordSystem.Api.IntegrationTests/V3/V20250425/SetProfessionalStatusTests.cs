using Optional;
using TeachingRecordSystem.Api.V3.V20250425.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using ProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.ProfessionalStatusStatus;
using TrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250425;

public class SetProfessionalStatusTests : TestBase
{
    public SetProfessionalStatusTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.SetProfessionalStatus]);
    }

    [Theory, RoleNamesData(except: ApiRoles.SetProfessionalStatus)]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(CreateRequest());

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonDoesNotExistForTrn_ReturnsNotFound()
    {
        // Arrange
        var trn = "0000000";
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(CreateRequest());

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [Theory]
    [InlineData(ProfessionalStatusStatus.InTraining)]
    [InlineData(ProfessionalStatusStatus.Awarded)]
    [InlineData(ProfessionalStatusStatus.Deferred)]
    [InlineData(ProfessionalStatusStatus.DeferredForSkillsTest)]
    [InlineData(ProfessionalStatusStatus.Failed)]
    [InlineData(ProfessionalStatusStatus.Withdrawn)]
    [InlineData(ProfessionalStatusStatus.UnderAssessment)]
    public async Task Put_RouteTypeIsOverseasAndStatusIsNotApproved_ReturnsBadRequest(ProfessionalStatusStatus status)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.ApplyForQtsId,
                Status = status
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status must be 'Approved' when route type is '{RouteToProfessionalStatusType.ApplyForQtsId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotOverseasAndStatusIsApproved_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.Approved
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status cannot be 'Approved' when route type is '{RouteToProfessionalStatusType.HeiProgrammeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsAssessmentOnlyRouteAndStatusIsInTraining_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.AssessmentOnlyRouteId,
                Status = ProfessionalStatusStatus.InTraining
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status cannot be 'InTraining' when route type is '{RouteToProfessionalStatusType.AssessmentOnlyRouteId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotAssessmentOnlyRouteAndStatusIsUnderAssessment_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.UnderAssessment
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status cannot be 'UnderAssessment' when route type is '{RouteToProfessionalStatusType.HeiProgrammeTypeId}'.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Put_StatusIsApprovedAndAwardedDateIsNotSpecified_ReturnsBadRequest(bool isOverseasRouteType)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var status = isOverseasRouteType ? ProfessionalStatusStatus.Approved : ProfessionalStatusStatus.Awarded;
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = isOverseasRouteType ? RouteToProfessionalStatusType.ApplyForQtsId : RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = status
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.AwardedDate)}", $"Awarded date must be specified when status is '{status}'.");
    }

    [Theory]
    [InlineData(ProfessionalStatusStatus.InTraining)]
    [InlineData(ProfessionalStatusStatus.Deferred)]
    [InlineData(ProfessionalStatusStatus.DeferredForSkillsTest)]
    [InlineData(ProfessionalStatusStatus.Failed)]
    [InlineData(ProfessionalStatusStatus.Withdrawn)]
    [InlineData(ProfessionalStatusStatus.UnderAssessment)]
    public async Task Put_StatusIsNotApprovedAndAwardedDateIsSpecified_ReturnsBadRequest(ProfessionalStatusStatus status)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = status,
                AwardedDate = Clock.Today
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.AwardedDate)}", $"Awarded date cannot be specified when status is '{status}'.");
    }

    [Fact]
    public async Task Put_AwardedDateIsInTheFuture_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.Awarded,
                AwardedDate = Clock.Today.AddDays(1)
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.AwardedDate)}", "Awarded date cannot be in the future.");
    }

    [Fact]
    public async Task Put_WithoutTrainingStartDate_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingStartDate = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingStartDate)}", "Training start date must be specified.");
    }

    [Fact]
    public async Task Put_WithoutTrainingEndDate_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingEndDate = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingEndDate)}", "Training end date must be specified.");
    }

    [Fact]
    public async Task Put_WithTrainingEndDateBeforeTrainingStartDate_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingStartDate = Clock.Today.AddMonths(-1),
                TrainingEndDate = Clock.Today.AddMonths(-2)
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingEndDate)}", "Training end date cannot be before training start date.");
    }

    [Fact]
    public async Task Put_WithMoreThanThreeTrainingSubjectReferences_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingSubjectReferences = Option.Some(new[] { "100343", "100300", "100301", "100302" })
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingSubjectReferences)}", "A maximum of 3 training subject references are allowed.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithoutFromAge_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = null,
                    To = 7
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.From)}", $"From age must be specified for specialism type '{TrainingAgeSpecialismType.Range}'.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithoutToAge_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = 3,
                    To = null
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.To)}", $"To age must be specified for specialism type '{TrainingAgeSpecialismType.Range}'.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithToAgeLessThanFromAge_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = 7,
                    To = 3
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.To)}", $"To age cannot be less than From age for specialism type '{TrainingAgeSpecialismType.Range}'.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithToOrFromAgeNotBetween0And19_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = 20,
                    To = 21
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorsForPropertiesAsync(
            response,
            new Dictionary<string, string>
            {
                { $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.From)}", $"From age must be 0-19 inclusive for specialism type '{TrainingAgeSpecialismType.Range}'."},
                { $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.To)}", $"To age must be 0-19 inclusive for specialism type '{TrainingAgeSpecialismType.Range}'." }
            });
    }

    [Fact]
    public async Task Put_RouteTypeIsNotOverseasOrInternationalQualifiedTeacherStatusWithNonGBTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingCountryReference = "FR"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB' when route type is '{RouteToProfessionalStatusType.HeiProgrammeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeData))]
    public async Task Put_RouteTypeIsOverseasWithoutTrainingCountryReference_ReturnsBadRequest(Guid routeTypeId)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be specified when route type is '{routeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsInternationalQualifiedTeacherStatusWithoutTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();
        var routeTypeId = RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId;

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingCountryReference = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be specified when route type is '{routeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeData))]
    public async Task Put_RouteTypeIsOverseasWithGBTrainingCountryReference_ReturnsBadRequest(Guid routeTypeId)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "GB"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be 'GB' when route type is '{routeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsScotlandWithNonScotlandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.ScotlandRId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB-SCT' when route type is '{RouteToProfessionalStatusType.ScotlandRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotScotlandWithScotlandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.ApplyForQtsId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "GB-SCT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be 'GB-SCT' when route type is not '{RouteToProfessionalStatusType.ScotlandRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNorthernIrelandWithNonNorthernIrelandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.NiRId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB-NIR' when route type is '{RouteToProfessionalStatusType.NiRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotNorthernIrelandWithNorthernIrelandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.ApplyForQtsId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "GB-NIR"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be 'GB-NIR' when route type is not '{RouteToProfessionalStatusType.NiRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsWalesWithNonWalesTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.WelshRId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB-WLS' or 'GB-CYM' when route type is '{RouteToProfessionalStatusType.WelshRId}'.");
    }

    [Theory]
    [InlineData("GB-WLS")]
    [InlineData("GB-CYM")]
    public async Task Put_RouteTypeIsNotWalesWithWalesTrainingCountryReference_ReturnsBadRequest(string trainingCountryReference)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingCountryReference = trainingCountryReference
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be '{trainingCountryReference}' when route type is not '{RouteToProfessionalStatusType.WelshRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotOverseasWithoutTrainingProviderUkprn_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingProviderUkprn = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingProviderUkprn)}", $"Training provider UKPRN must be specified when route type is '{RouteToProfessionalStatusType.HeiProgrammeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeData))]
    public async Task Put_RouteTypeIsOverseasWithTrainingProviderUkprn_ReturnsBadRequest(Guid routeTypeId)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingProviderUkprn)}", $"Training provider UKPRN cannot be specified when route type is '{routeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(RouteTypeWhichCanHaveInductionExemptionData))]
    public async Task Put_RouteTypeWhichCanHaveInductionExemptionWithoutIsExemptFromInduction_ReturnsBadRequest(Guid routeTypeId, string? trainingCountryReference)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = trainingCountryReference,
                TrainingProviderUkprn = routeTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId ? "11111111" : null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.IsExemptFromInduction)}", $"Is exempt from induction must be specified when route type is '{routeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeWhichCannotHaveInductionExemptionWithIsExemptFromInduction_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                IsExemptFromInduction = true
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.IsExemptFromInduction)}", $"Is exempt from induction cannot be specified when route type is '{RouteToProfessionalStatusType.HeiProgrammeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeDoesNotMapToIttProgrammeType_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();
        var legacyIttRouteTypeId = Guid.Parse("4514EC65-20B0-4465-B66F-4718963C5B80");

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = legacyIttRouteTypeId
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidRouteType, StatusCodes.Status400BadRequest);
    }

    [Theory]
    [InlineData("AAAAAA", "100300", "100343")]
    [InlineData("100343", "BBBBBB", "100300")]
    [InlineData("100343", "100300", "CCCCCC")]
    public async Task Put_TrainingSubjectReferenceDoesNotMapToIttSubject_ReturnsBadRequest(string subject1, string subject2, string subject3)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                TrainingSubjectReferences = Option.Some(new[] { subject1, subject2, subject3 })
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidTrainingSubjectReference, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_TrainingCountryReferenceDoesNotMapToDqtCountry_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.ApplyForQtsId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingProviderUkprn = null,
                TrainingCountryReference = "9999",
                IsExemptFromInduction = true
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidTrainingCountryReference, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_TrainingProviderUkprnDoesNotMapToIttProvider_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingProviderUkprn = "12345678"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidTrainingProviderUkprn, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_DegreeTypeDoesNotMapToIttQualification_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var requestId = Guid.NewGuid().ToString();

        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
                DegreeTypeId = Guid.NewGuid()
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{requestId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidDegreeType, StatusCodes.Status400BadRequest);
    }

    private SetProfessionalStatusRequest CreateRequest() =>
        new SetProfessionalStatusRequest()
        {
            RouteTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
            Status = ProfessionalStatusStatus.InTraining,
            AwardedDate = null,
            TrainingStartDate = Clock.Today.AddMonths(-1),
            TrainingEndDate = Clock.Today.AddMonths(9),
            TrainingSubjectReferences = Option.Some<string[]>(["100343", "100300", "100079"]),
            TrainingAgeSpecialism = new()
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 7
            },
            TrainingCountryReference = "GB",
            TrainingProviderUkprn = "11111111"
        };

    public static TheoryData<Guid, string, bool?, string, string, string, Guid?> OverseasRouteTypeCreateData { get; } = new()
    {
        {
            RouteToProfessionalStatusType.ApplyForQtsId,        // RouteTypeId
            "PT",                                               // TrainingCountryReference
            true,                                               // IsExemptFromInduction
            "Portugal",                                         // ExpectedTrainingCountryName
            "Non-UK establishment",                             // ExpectedIttProviderName
            "104",                                              // ExpectedTeacherStatusValue
            InductionExemptionReason.OverseasTrainedTeacherId   // ExpectedInductionExemptionReasonId
        },
        {
            RouteToProfessionalStatusType.EuropeanRecognitionId,
            "FR",
            null,
            "France",
            "Non-UK establishment",
            "223",
            null
        },
        {
            RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId,
            "ES",
            null,
            "Spain",
            "Non-UK establishment",
            "103",
            null
        },
        {
            RouteToProfessionalStatusType.NiRId,
            "GB-NIR",
            true,
            "Northern Ireland",
            "UK establishment (Scotland/Northern Ireland)",
            "69",
            InductionExemptionReason.PassedInductionInNorthernIrelandId
        },
        {
            RouteToProfessionalStatusType.ScotlandRId,
            "GB-SCT",
            true,
            "Scotland",
            "UK establishment (Scotland/Northern Ireland)",
            "68",
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId
        }
    };

    public static TheoryData<Guid> OverseasRouteTypeData { get; } =
    [
        RouteToProfessionalStatusType.ApplyForQtsId,
        RouteToProfessionalStatusType.EuropeanRecognitionId,
        RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId,
        RouteToProfessionalStatusType.NiRId,
        RouteToProfessionalStatusType.ScotlandRId
    ];

    public static TheoryData<Guid, string?> RouteTypeWhichCanHaveInductionExemptionData { get; } = new()
    {
        {
            RouteToProfessionalStatusType.ApplyForQtsId,
            "PT"
        },
        {
            RouteToProfessionalStatusType.QtlsAndSetMembershipId,
            null
        },
        {
            RouteToProfessionalStatusType.ScotlandRId,
            "GB-SCT"
        },
        {
            RouteToProfessionalStatusType.NiRId,
            "GB-NIR"
        }
    };

    public static TheoryData<Guid, string?, string?> ExistingQtsData { get; } = new()
    {
        {
            RouteToProfessionalStatusType.EarlyYearsIttGraduateEntryId,
            null,
            "220"
        },
        {
            RouteToProfessionalStatusType.HeiProgrammeTypeId,
            "211",
            null
        },
        {
            RouteToProfessionalStatusType.AssessmentOnlyRouteId,
            "212",
            null
        },
        {
            RouteToProfessionalStatusType.EarlyYearsIttGraduateEntryId,
            null,
            null
        },
        {
            RouteToProfessionalStatusType.HeiProgrammeTypeId,
            null,
            null
        }
    };
}
