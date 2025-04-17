using Microsoft.Xrm.Sdk;
using Optional;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using ProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.ProfessionalStatusStatus;
using TrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.TrainingAgeSpecialismType;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

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

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn());
        var slugId = 1;

        var request = CreateJsonContent(CreateRequest());

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonDoesNotExistForTrn_ReturnsNotFound()
    {
        // Arrange
        var invalidTrn = "0000001";
        var slugId = 1;
        var request = CreateJsonContent(CreateRequest());

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{invalidTrn}/professional-statuses/{slugId}", request);

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
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.ApplyforQtsId,
                Status = status
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status must be 'Approved' when route type is '{RouteToProfessionalStatus.ApplyforQtsId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotOverseasAndStatusIsApproved_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.Approved
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status cannot be 'Approved' when route type is '{RouteToProfessionalStatus.HeiProgrammeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsAssessmentOnlyRouteAndStatusIsInTraining_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.AssessmentOnlyRouteId,
                Status = ProfessionalStatusStatus.InTraining
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status cannot be 'InTraining' when route type is '{RouteToProfessionalStatus.AssessmentOnlyRouteId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotAssessmentOnlyRouteAndStatusIsUnderAssessment_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.UnderAssessment
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.Status)}", $"Status cannot be 'UnderAssessment' when route type is '{RouteToProfessionalStatus.HeiProgrammeTypeId}'.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Put_StatusIsApprovedAndAwardedDateIsNotSpecified_ReturnsBadRequest(bool isOverseasRouteType)
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var status = isOverseasRouteType ? ProfessionalStatusStatus.Approved : ProfessionalStatusStatus.Awarded;
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = isOverseasRouteType ? RouteToProfessionalStatus.ApplyforQtsId : RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = status
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

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
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = status,
                AwardedDate = Clock.Today
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.AwardedDate)}", $"Awarded date cannot be specified when status is '{status}'.");
    }

    [Fact]
    public async Task Put_AwardedDateIsInTheFuture_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.Awarded,
                AwardedDate = Clock.Today.AddDays(1)
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.AwardedDate)}", "Awarded date cannot be in the future.");
    }

    [Fact]
    public async Task Put_WithoutTrainingStartDate_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingStartDate = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingStartDate)}", "Training start date must be specified.");
    }

    [Fact]
    public async Task Put_WithoutTrainingEndDate_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingEndDate = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingEndDate)}", "Training end date must be specified.");
    }

    [Fact]
    public async Task Put_WithTrainingEndDateBeforeTrainingStartDate_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingStartDate = Clock.Today.AddMonths(-1),
                TrainingEndDate = Clock.Today.AddMonths(-2)
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingEndDate)}", "Training end date cannot be before training start date.");
    }

    [Fact]
    public async Task Put_WithMoreThanThreeTrainingSubjectReferences_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingSubjectReferences = Option.Some(new[] { "100343", "100300", "100301", "100302" })
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingSubjectReferences)}", "A maximum of 3 training subject references are allowed.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithoutFromAge_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = null,
                    To = 7
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.From)}", $"From age must be specified for specialism type '{TrainingAgeSpecialismType.Range}'.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithoutToAge_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = 3,
                    To = null
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.To)}", $"To age must be specified for specialism type '{TrainingAgeSpecialismType.Range}'.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithToAgeLessThanFromAge_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = 7,
                    To = 3
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.To)}", $"To age cannot be less than From age for specialism type '{TrainingAgeSpecialismType.Range}'.");
    }

    [Fact]
    public async Task Put_TrainingAgeSpecialismTypeIsRangeWithToOrFromAgeNotBetween0And19_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingAgeSpecialism = new SetProfessionalStatusRequestTrainingAgeSpecialism
                {
                    Type = TrainingAgeSpecialismType.Range,
                    From = 20,
                    To = 21
                }
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorsForPropertiesAsync(
            response,
            new Dictionary<string, string>()
            {
                { $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.From)}", $"From age must be 0-19 inclusive for specialism type '{TrainingAgeSpecialismType.Range}'."},
                { $"{nameof(SetProfessionalStatusRequest.TrainingAgeSpecialism)}.{nameof(SetProfessionalStatusRequestTrainingAgeSpecialism.To)}", $"To age must be 0-19 inclusive for specialism type '{TrainingAgeSpecialismType.Range}'." }
            });
    }

    [Fact]
    public async Task Put_RouteTypeIsNotOverseasOrInternationalQualifiedTeacherStatusWithNonGBTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingCountryReference = "FR"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB' when route type is '{RouteToProfessionalStatus.HeiProgrammeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeData))]
    public async Task Put_RouteTypeIsOverseasWithoutTrainingCountryReference_ReturnsBadRequest(Guid routeTypeId)
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be specified when route type is '{routeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsInternationalQualifiedTeacherStatusWithoutTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var routeTypeId = RouteToProfessionalStatus.InternationalQualifiedTeacherStatusId;
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingCountryReference = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be specified when route type is '{routeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeData))]
    public async Task Put_RouteTypeIsOverseasWithGBTrainingCountryReference_ReturnsBadRequest(Guid routeTypeId)
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "GB"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be 'GB' when route type is '{routeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsScotlandWithNonScotlandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.ScotlandRId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB-SCT' when route type is '{RouteToProfessionalStatus.ScotlandRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotScotlandWithScotlandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.ApplyforQtsId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "GB-SCT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be 'GB-SCT' when route type is not '{RouteToProfessionalStatus.ScotlandRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNorthernIrelandWithNonNorthernIrelandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.NiRId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB-NIR' when route type is '{RouteToProfessionalStatus.NiRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotNorthernIrelandWithNorthernIrelandTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.ApplyforQtsId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "GB-NIR"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be 'GB-NIR' when route type is not '{RouteToProfessionalStatus.NiRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsWalesWithNonWalesTrainingCountryReference_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.WelshRId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference must be 'GB-WLS' or 'GB-CYM' when route type is '{RouteToProfessionalStatus.WelshRId}'.");
    }

    [Theory]
    [InlineData("GB-WLS")]
    [InlineData("GB-CYM")]
    public async Task Put_RouteTypeIsNotWalesWithWalesTrainingCountryReference_ReturnsBadRequest(string trainingCountryReference)
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingCountryReference = trainingCountryReference
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingCountryReference)}", $"Training country reference cannot be '{trainingCountryReference}' when route type is not '{RouteToProfessionalStatus.WelshRId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeIsNotOverseasWithoutTrainingProviderUkprn_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingProviderUkprn = null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingProviderUkprn)}", $"Training provider UKPRN must be specified when route type is '{RouteToProfessionalStatus.HeiProgrammeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeData))]
    public async Task Put_RouteTypeIsOverseasWithTrainingProviderUkprn_ReturnsBadRequest(Guid routeTypeId)
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = "PT"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.TrainingProviderUkprn)}", $"Training provider UKPRN cannot be specified when route type is '{routeTypeId}'.");
    }

    [Theory]
    [MemberData(nameof(RouteTypeWhichCanHaveInductionExemptionData))]
    public async Task Put_RouteTypeWhichCanHaveInductionExemptionWithoutIsExemptFromInduction_ReturnsBadRequest(Guid routeTypeId, string? trainingCountryReference)
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingCountryReference = trainingCountryReference,
                TrainingProviderUkprn = routeTypeId == RouteToProfessionalStatus.QtlsAndSetMembershipId ? "10044534" : null
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.IsExemptFromInduction)}", $"Is exempt from induction must be specified when route type is '{routeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeWhichCannotHaveInductionExemptionWithIsExemptFromInduction_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                IsExemptFromInduction = true
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, $"{nameof(SetProfessionalStatusRequest.IsExemptFromInduction)}", $"Is exempt from induction cannot be specified when route type is '{RouteToProfessionalStatus.HeiProgrammeTypeId}'.");
    }

    [Fact]
    public async Task Put_RouteTypeDoesNotMapToIttProgrammeType_ReturnsBadRequest()
    {
        // Arrange
        var legacyIttRouteTypeId = Guid.Parse("4514EC65-20B0-4465-B66F-4718963C5B80");
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = legacyIttRouteTypeId
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

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
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                TrainingSubjectReferences = Option.Some(new[] { subject1, subject2, subject3 })
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidTrainingSubjectReference, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_TrainingCountryReferenceDoesNotMapToDqtCountry_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "123456789";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.ApplyforQtsId,
                Status = ProfessionalStatusStatus.Approved,
                AwardedDate = Clock.Today,
                TrainingProviderUkprn = null,
                TrainingCountryReference = "9999",
                IsExemptFromInduction = true
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidTrainingCountryReference, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_TrainingProviderUkprnDoesNotMapToIttProvider_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = ProfessionalStatusStatus.InTraining,
                TrainingProviderUkprn = "12345678"
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidTrainingProviderUkprn, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_DegreeTypeDoesNotMapToIttQualification_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));
        var request = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
                DegreeTypeId = Guid.NewGuid()
            });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidDegreeType, StatusCodes.Status400BadRequest);
    }

    [Theory]
    [MemberData(nameof(OverseasRouteTypeCreateData))]
    public async Task Put_ValidRequestWithOverseasRouteType_ReturnsNoContent(Guid routeTypeId, string trainingCountryReference, bool? isExemptFromInduction, string expectedTrainingCountryName, string expectedIttProviderName, string expectedTeacherStatusValue, Guid? expectedInductionExemptionReasonId)
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var request = new SetProfessionalStatusRequest
        {
            RouteTypeId = routeTypeId,
            Status = ProfessionalStatusStatus.Approved,
            AwardedDate = Clock.Today,
            TrainingStartDate = Clock.Today.AddMonths(-1),
            TrainingEndDate = Clock.Today.AddMonths(9),
            TrainingSubjectReferences = Option.Some<string[]>(["100343", "100300", "100078"]),
            TrainingAgeSpecialism = new()
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 7
            },
            TrainingCountryReference = trainingCountryReference,
            TrainingProviderUkprn = null,
            IsExemptFromInduction = isExemptFromInduction
        };
        var requestJson = CreateJsonContent(request);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        // Check ITT record 
        var itt = ctx.dfeta_initialteachertrainingSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person.PersonId);
        Assert.NotNull(itt);
        Assert.Equal(dfeta_ITTResult.Approved, itt.dfeta_Result);
        Assert.Equal(request.TrainingStartDate, itt.dfeta_ProgrammeStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        Assert.Equal(request.TrainingEndDate, itt.dfeta_ProgrammeEndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        Assert.Equal(dfeta_AgeRange._03, itt.dfeta_AgeRangeFrom);
        Assert.Equal(dfeta_AgeRange._07, itt.dfeta_AgeRangeTo);
        var expectedIttProvider = ctx.AccountSet.Single(a => a.Name == expectedIttProviderName);
        Assert.Equal(expectedIttProvider.Id, itt.dfeta_EstablishmentId?.Id);
        var expectedTrainingCountry = ctx.dfeta_countrySet.Single(c => c.dfeta_name == expectedTrainingCountryName);
        Assert.Equal(expectedTrainingCountry.Id, itt.dfeta_CountryId.Id);
        // Check QTS record
        var qts = ctx.dfeta_qtsregistrationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person.PersonId);
        Assert.NotNull(qts);
        Assert.Equal(qts.Id, itt.dfeta_qtsregistration.Id);
        var expectedTeacherStatus = ctx.dfeta_teacherstatusSet.Single(t => t.dfeta_Value == expectedTeacherStatusValue);
        Assert.Equal(expectedTeacherStatus.Id, qts.dfeta_TeacherStatusId.Id);
        Assert.Equal(request.AwardedDate, qts.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        // Check outbox message
        var (crmQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<SetProfessionalStatusQuery, bool>();
        var outboxMessage = crmQuery?.InductionOutboxMessage;
        Assert.NotNull(outboxMessage);
        if (isExemptFromInduction ?? false)
        {
            Assert.Equal(nameof(AddInductionExemptionMessage), outboxMessage.dfeta_MessageName);
            var messageSerializer = HostFixture.Services.GetRequiredService<MessageSerializer>();
            var message = Assert.IsType<AddInductionExemptionMessage>(messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName));
            Assert.Equal(person.PersonId, message.PersonId);
            Assert.Equal(expectedInductionExemptionReasonId, message.ExemptionReasonId);
            Assert.Equal(ApplicationUserId, message.TrsUserId);
        }
        else
        {
            Assert.Equal(nameof(SetInductionRequiredToCompleteMessage), outboxMessage.dfeta_MessageName);
            var messageSerializer = HostFixture.Services.GetRequiredService<MessageSerializer>();
            var message = Assert.IsType<SetInductionRequiredToCompleteMessage>(messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName));
            Assert.Equal(person.PersonId, message.PersonId);
            Assert.Equal(ApplicationUserId, message.TrsUserId);
        }
    }

    [Theory]
    [MemberData(nameof(NonOverseasRouteTypeCreateData))]
    public async Task Put_ValidRequestWithNonOverseasRouteTypeWithNoExistingIttRecords_ReturnsNoContent(
        Guid routeTypeId,
        ProfessionalStatusStatus status,
        SetProfessionalStatusRequestTrainingAgeSpecialism? trainingAgeSpecialism,
        dfeta_ITTResult expectedIttResult,
        string? expectedTeacherStatusValue,
        string? expectedEarlyYearsTeacherStatusValue,
        dfeta_AgeRange? expectedAgeRangeFrom,
        dfeta_AgeRange? expectedAgeRangeTo)
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var trainingProviderUkprn = "10044534";
        var expectedIttProviderName = "ARK Teacher Training";
        var trainingCountryReference = "GB";
        var request = new SetProfessionalStatusRequest
        {
            RouteTypeId = routeTypeId,
            Status = status,
            AwardedDate = status == ProfessionalStatusStatus.Awarded ? Clock.Today : null,
            TrainingStartDate = Clock.Today.AddMonths(-1),
            TrainingEndDate = Clock.Today.AddMonths(9),
            TrainingSubjectReferences = Option.Some<string[]>(["100343", "100300", "100078"]),
            TrainingAgeSpecialism = trainingAgeSpecialism,
            TrainingProviderUkprn = trainingProviderUkprn,
            TrainingCountryReference = trainingCountryReference,
        };
        var requestJson = CreateJsonContent(request);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);

        // Check ITT record 
        var itt = ctx.dfeta_initialteachertrainingSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person.PersonId);
        Assert.NotNull(itt);
        Assert.Equal(expectedIttResult, itt.dfeta_Result);
        Assert.Equal(request.TrainingStartDate, itt.dfeta_ProgrammeStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        Assert.Equal(request.TrainingEndDate, itt.dfeta_ProgrammeEndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        Assert.Equal(expectedAgeRangeFrom, itt.dfeta_AgeRangeFrom);
        Assert.Equal(expectedAgeRangeTo, itt.dfeta_AgeRangeTo);
        var expectedIttProvider = ctx.AccountSet.Single(a => a.Name == expectedIttProviderName);
        Assert.Equal(expectedIttProvider.Id, itt.dfeta_EstablishmentId?.Id);
        var country = ctx.dfeta_countrySet.SingleOrDefault(c => c.dfeta_Value == trainingCountryReference);
        var defaultCountry = ctx.dfeta_countrySet.SingleOrDefault(c => c.dfeta_Value == "XK");
        if (trainingCountryReference is null)
        {
            Assert.Equal(defaultCountry!.Id, itt.dfeta_CountryId.Id);
        }
        else
        {
            Assert.Equal(country!.Id, itt.dfeta_CountryId.Id);
        }

        // Check QTS record
        var qts = ctx.dfeta_qtsregistrationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person.PersonId);
        Assert.NotNull(qts);
        Assert.Equal(qts.Id, itt.dfeta_qtsregistration.Id);
        if (expectedTeacherStatusValue is not null)
        {
            var expectedTeacherStatus = ctx.dfeta_teacherstatusSet.Single(t => t.dfeta_Value == expectedTeacherStatusValue);
            Assert.Equal(expectedTeacherStatus.Id, qts.dfeta_TeacherStatusId.Id);
            Assert.Equal(request.AwardedDate, qts.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        }
        else
        {
            Assert.Null(qts.dfeta_TeacherStatusId);
        }
        if (expectedEarlyYearsTeacherStatusValue is not null)
        {
            var expectedEarlyYearsTeacherStatus = ctx.dfeta_earlyyearsstatusSet.Single(t => t.dfeta_Value == expectedEarlyYearsTeacherStatusValue);
            Assert.Equal(expectedEarlyYearsTeacherStatus.Id, qts.dfeta_EarlyYearsStatusId.Id);
            Assert.Equal(request.AwardedDate, qts.dfeta_EYTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        }
        else
        {
            Assert.Null(qts.dfeta_EarlyYearsStatusId);
        }

        if (status == ProfessionalStatusStatus.Awarded)
        {
            // Check outbox message
            var (crmQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<SetProfessionalStatusQuery, bool>();
            var outboxMessage = crmQuery?.InductionOutboxMessage;
            Assert.NotNull(outboxMessage);
            Assert.Equal(nameof(SetInductionRequiredToCompleteMessage), outboxMessage.dfeta_MessageName);
            Assert.Equal(nameof(SetInductionRequiredToCompleteMessage), outboxMessage.dfeta_MessageName);
            var messageSerializer = HostFixture.Services.GetRequiredService<MessageSerializer>();
            var message = Assert.IsType<SetInductionRequiredToCompleteMessage>(messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName));
            Assert.Equal(person.PersonId, message.PersonId);
            Assert.Equal(ApplicationUserId, message.TrsUserId);
        }
    }

    [Fact]
    public async Task Put_EarlyYearsRouteTypeWithExistingNonEarlyYearsItt_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = RouteToProfessionalStatus.EarlyYearsIttGraduateEntryId
            });

        // Create existing non early years ITT record
        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.UnableToChangeRouteType, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_NonEarlyYearsRouteTypeWithExistingEarlyYearsItt_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest());

        // Create existing early years ITT record
        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_ProgrammeType = dfeta_ITTProgrammeType.EYITTGraduateEntry;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.UnableToChangeRouteType, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_WithExistingIttWithPassResult_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest());

        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_Result = dfeta_ITTResult.Pass;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.RouteToProfessionalStatusAlreadyAwarded, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_FailedStatusWithExistingIttWithFailResult_ReturnsNoContent()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest() with
            {
                Status = ProfessionalStatusStatus.Failed
            });

        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_Result = dfeta_ITTResult.Fail;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_WithdrawnStatusWithExistingIttWithWithdrawnResult_ReturnsNoContent()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest() with
            {
                Status = ProfessionalStatusStatus.Withdrawn
            });

        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_Result = dfeta_ITTResult.Withdrawn;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(ProfessionalStatusStatus.Deferred)]
    [InlineData(ProfessionalStatusStatus.InTraining)]
    [InlineData(ProfessionalStatusStatus.UnderAssessment)]
    public async Task Put_InvalidStatusWithExistingIttWithFailResult_ReturnsBadRequest(ProfessionalStatusStatus status)
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = status == ProfessionalStatusStatus.UnderAssessment ? RouteToProfessionalStatus.AssessmentOnlyRouteId : RouteToProfessionalStatus.HeiProgrammeTypeId,
                Status = status
            });

        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_Result = dfeta_ITTResult.Fail;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.UnableToChangeFailProfessionalStatusStatus, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_DeferredStatusWithExistingIttWithWithdrawnResult_ReturnsBadRequest()
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest() with
            {
                Status = ProfessionalStatusStatus.Deferred
            });

        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_Result = dfeta_ITTResult.Withdrawn;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.UnableToChangeWithdrawnProfessionalStatusStatus, StatusCodes.Status400BadRequest);
    }

    [Theory]
    [MemberData(nameof(ExistingQtsData))]
    public async Task Put_WithMultipleExistingPotentialMatchingQtsRegistrations_ReturnsBadRequest(Guid routeTypeId, string? teacherStatusValue, string? earlyYearsStatusValue)
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var requestJson = CreateJsonContent(
            CreateRequest() with
            {
                RouteTypeId = routeTypeId,
                Status = ProfessionalStatusStatus.Awarded,
                AwardedDate = Clock.Today
            });

        var teacherStatusId = teacherStatusValue is not null ? new EntityReference(dfeta_teacherstatus.EntityLogicalName, (await TestData.ReferenceDataCache.GetTeacherStatusByValueAsync(teacherStatusValue)).Id) : null;
        var earlyYearsStatusId = earlyYearsStatusValue is not null ? new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, (await TestData.ReferenceDataCache.GetEarlyYearsStatusByValueAsync(earlyYearsStatusValue)).Id) : null;

        var qts1 = CreateQtsRegistration(person.PersonId);
        qts1.dfeta_TeacherStatusId = teacherStatusId;
        qts1.dfeta_EarlyYearsStatusId = earlyYearsStatusId;
        var qts2 = CreateQtsRegistration(person.PersonId);
        qts2.dfeta_TeacherStatusId = teacherStatusId;
        qts2.dfeta_EarlyYearsStatusId = earlyYearsStatusId;
        await TestData.OrganizationService.CreateAsync(qts1);
        await TestData.OrganizationService.CreateAsync(qts2);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.MultipleQtsRecords, StatusCodes.Status400BadRequest);
    }

    [Theory]
    [MemberData(nameof(UpdatedRouteData))]
    public async Task Put_WithExistingQtsRegistration_UpdatesIttAndQtsAndReturnsNoContent(
        dfeta_ITTProgrammeType existingIttProgrammeType,
        string? existingTeacherStatusValue,
        string? existingEarlyYearsStatusValue,
        dfeta_ITTResult existingIttResult,
        Guid routeTypeId,
        dfeta_ITTProgrammeType expectedIttProgrammeType,
        ProfessionalStatusStatus status,
        dfeta_ITTResult expectedIttResult,
        string? expectedTeacherStatusValue,
        string? expectedEarlyYearsStatusValue)
    {
        // Arrange
        var slugId = "12345678";
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId));

        var request = CreateRequest() with
        {
            RouteTypeId = routeTypeId,
            Status = status,
            AwardedDate = status == ProfessionalStatusStatus.Awarded ? Clock.Today : null
        };

        var requestJson = CreateJsonContent(request);

        var existingTeacherStatusId = existingTeacherStatusValue is not null ? new EntityReference(dfeta_teacherstatus.EntityLogicalName, (await TestData.ReferenceDataCache.GetTeacherStatusByValueAsync(existingTeacherStatusValue)).Id) : null;
        var existingEarlyYearsStatusId = existingEarlyYearsStatusValue is not null ? new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, (await TestData.ReferenceDataCache.GetEarlyYearsStatusByValueAsync(existingEarlyYearsStatusValue)).Id) : null;

        var itt = await CreateInitialTeacherTrainingAsync(person.PersonId, slugId);
        itt.dfeta_ProgrammeType = existingIttProgrammeType;
        itt.dfeta_Result = existingIttResult;
        var ittId = await TestData.OrganizationService.CreateAsync(itt);

        var qts = CreateQtsRegistration(person.PersonId);
        qts.dfeta_TeacherStatusId = existingTeacherStatusId;
        qts.dfeta_EarlyYearsStatusId = existingEarlyYearsStatusId;
        var qtsId = await TestData.OrganizationService.CreateAsync(qts);

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/professional-statuses/{slugId}", requestJson);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);

        // Check ITT record
        var updatedItt = ctx.dfeta_initialteachertrainingSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person.PersonId);
        Assert.NotNull(updatedItt);
        Assert.Equal(expectedIttProgrammeType, updatedItt.dfeta_ProgrammeType);
        Assert.Equal(expectedIttResult, updatedItt.dfeta_Result);
        Assert.Equal(request.TrainingStartDate, updatedItt.dfeta_ProgrammeStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        Assert.Equal(request.TrainingEndDate, updatedItt.dfeta_ProgrammeEndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        Assert.Equal(dfeta_AgeRange._03, updatedItt.dfeta_AgeRangeFrom);
        Assert.Equal(dfeta_AgeRange._07, updatedItt.dfeta_AgeRangeTo);
        var expectedIttProvider = ctx.AccountSet.Single(a => a.dfeta_UKPRN == request.TrainingProviderUkprn);
        Assert.Equal(expectedIttProvider.Id, updatedItt.dfeta_EstablishmentId?.Id);
        // Check QTS record
        var updatedQts = ctx.dfeta_qtsregistrationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person.PersonId);
        Assert.NotNull(qts);
        Assert.Equal(qts.Id, updatedItt.dfeta_qtsregistration.Id);
        if (expectedTeacherStatusValue is not null)
        {
            var expectedTeacherStatus = ctx.dfeta_teacherstatusSet.Single(t => t.dfeta_Value == expectedTeacherStatusValue);
            Assert.Equal(expectedTeacherStatus.Id, updatedQts!.dfeta_TeacherStatusId.Id);
            Assert.Equal(request.AwardedDate, updatedQts.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        }
        else
        {
            Assert.Null(updatedQts!.dfeta_TeacherStatusId);
        }

        if (expectedEarlyYearsStatusValue is not null)
        {
            var expectedEarlyYearsStatus = ctx.dfeta_earlyyearsstatusSet.Single(t => t.dfeta_Value == expectedEarlyYearsStatusValue);
            Assert.Equal(expectedEarlyYearsStatus.Id, updatedQts.dfeta_EarlyYearsStatusId.Id);
            Assert.Equal(request.AwardedDate, updatedQts.dfeta_EYTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true));
        }
        else
        {
            Assert.Null(updatedQts.dfeta_EarlyYearsStatusId);
        }
    }

    private SetProfessionalStatusRequest CreateRequest() =>
        new SetProfessionalStatusRequest
        {
            RouteTypeId = RouteToProfessionalStatus.HeiProgrammeTypeId,
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
            TrainingProviderUkprn = "10007799"
        };

    private async Task<dfeta_initialteachertraining> CreateInitialTeacherTrainingAsync(Guid personId, string slugId)
    {
        var ittProvider = await TestData.ReferenceDataCache.GetIttProviderByUkPrnAsync("10044534");
        var subject1 = await TestData.ReferenceDataCache.GetIttSubjectBySubjectCodeAsync("100078");

        return new dfeta_initialteachertraining
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, personId),
            dfeta_SlugId = slugId,
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.HEI,
            dfeta_Result = dfeta_ITTResult.InTraining,
            dfeta_ProgrammeStartDate = Clock.Today.AddMonths(-1).ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_ProgrammeEndDate = Clock.Today.AddMonths(9).ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_Subject1Id = new EntityReference(dfeta_ittsubject.EntityLogicalName, subject1!.Id),
            dfeta_AgeRangeFrom = dfeta_AgeRange.KeyStage1,
            dfeta_AgeRangeTo = dfeta_AgeRange.KeyStage1,
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, ittProvider!.Id)
        };
    }

    private dfeta_qtsregistration CreateQtsRegistration(Guid personId) =>
        new dfeta_qtsregistration
        {
            Id = Guid.NewGuid(),
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, personId)
        };

    public static TheoryData<Guid, ProfessionalStatusStatus, SetProfessionalStatusRequestTrainingAgeSpecialism?, dfeta_ITTResult, string?, string?, dfeta_AgeRange?, dfeta_AgeRange?> NonOverseasRouteTypeCreateData { get; } = new()
    {
        {
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            ProfessionalStatusStatus.InTraining,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 7
            },
            dfeta_ITTResult.InTraining,
            "211", // Trainee Teacher
            null,
            dfeta_AgeRange._03,
            dfeta_AgeRange._07
        },
        {
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            ProfessionalStatusStatus.Awarded,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 7
            },
            dfeta_ITTResult.Pass,
            "71", // Qualified teacher (trained)
            null,
            dfeta_AgeRange._03,
            dfeta_AgeRange._07
        },
        {
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            ProfessionalStatusStatus.Withdrawn,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage4,
            },
            dfeta_ITTResult.Withdrawn,
            null,
            null,
            dfeta_AgeRange.KeyStage4,
            dfeta_AgeRange.KeyStage4
        },
        {
            RouteToProfessionalStatus.InternationalQualifiedTeacherStatusId,
            ProfessionalStatusStatus.InTraining,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage2,
            },
            dfeta_ITTResult.InTraining,
            "211", // Trainee Teacher
            null,
            dfeta_AgeRange.KeyStage2,
            dfeta_AgeRange.KeyStage2
        },
        {
            RouteToProfessionalStatus.InternationalQualifiedTeacherStatusId,
            ProfessionalStatusStatus.Awarded,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage2,
            },
            dfeta_ITTResult.Pass,
            "90", // Qualified teacher: by virtue of achieving international qualified teacher status
            null,
            dfeta_AgeRange.KeyStage2,
            dfeta_AgeRange.KeyStage2
        },
        {
            RouteToProfessionalStatus.InternationalQualifiedTeacherStatusId,
            ProfessionalStatusStatus.Withdrawn,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage2,
            },
            dfeta_ITTResult.Withdrawn,
            null,
            null,
            dfeta_AgeRange.KeyStage2,
            dfeta_AgeRange.KeyStage2
        },
        {
            RouteToProfessionalStatus.AssessmentOnlyRouteId,
            ProfessionalStatusStatus.UnderAssessment,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage3,
            },
            dfeta_ITTResult.UnderAssessment,
            "212", // AOR Candidate
            null,
            dfeta_AgeRange.KeyStage3,
            dfeta_AgeRange.KeyStage3
        },
        {
            RouteToProfessionalStatus.AssessmentOnlyRouteId,
            ProfessionalStatusStatus.Awarded,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage4,
            },
            dfeta_ITTResult.Pass,
            "100", // Qualified Teacher: Assessment Only Route
            null,
            dfeta_AgeRange.KeyStage4,
            dfeta_AgeRange.KeyStage4
        },
        {
            RouteToProfessionalStatus.AssessmentOnlyRouteId,
            ProfessionalStatusStatus.Withdrawn,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.KeyStage4,
            },
            dfeta_ITTResult.Withdrawn,
            null,
            null,
            dfeta_AgeRange.KeyStage4,
            dfeta_AgeRange.KeyStage4
        },
        {
            RouteToProfessionalStatus.EarlyYearsIttGraduateEntryId,
            ProfessionalStatusStatus.InTraining,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 5
            },
            dfeta_ITTResult.InTraining,
            null,
            "220", // Early Years Trainee
            dfeta_AgeRange._03,
            dfeta_AgeRange._05
        },
        {
            RouteToProfessionalStatus.EarlyYearsIttGraduateEntryId,
            ProfessionalStatusStatus.Awarded,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 5
            },
            dfeta_ITTResult.Pass,
            null,
            "221", // Early Years Teacher Status
            dfeta_AgeRange._03,
            dfeta_AgeRange._05
        },
        {
            RouteToProfessionalStatus.EarlyYearsIttGraduateEntryId,
            ProfessionalStatusStatus.Withdrawn,
            new SetProfessionalStatusRequestTrainingAgeSpecialism
            {
                Type = TrainingAgeSpecialismType.Range,
                From = 3,
                To = 5
            },
            dfeta_ITTResult.Withdrawn,
            null,
            null,
            dfeta_AgeRange._03,
            dfeta_AgeRange._05
        }
    };

    public static TheoryData<Guid, string, bool?, string, string, string, Guid?> OverseasRouteTypeCreateData { get; } = new()
    {
        {
            RouteToProfessionalStatus.ApplyforQtsId,            // RouteTypeId
            "PT",                                               // TrainingCountryReference
            true,                                               // IsExemptFromInduction
            "Portugal",                                         // ExpectedTrainingCountryName
            "Non-UK establishment",                             // ExpectedIttProviderName
            "104",                                              // ExpectedTeacherStatusValue
            InductionExemptionReason.OverseasTrainedTeacherId   // ExpectedInductionExemptionReasonId
        },
        {
            RouteToProfessionalStatus.EuropeanRecognitionId,
            "FR",
            null,
            "France",
            "Non-UK establishment",
            "223",
            null
        },
        {
            RouteToProfessionalStatus.OverseasTrainedTeacherRecognitionId,
            "ES",
            null,
            "Spain",
            "Non-UK establishment",
            "103",
            null
        },
        {
            RouteToProfessionalStatus.NiRId,
            "GB-NIR",
            true,
            "Northern Ireland",
            "UK establishment (Scotland/Northern Ireland)",
            "69",
            InductionExemptionReason.PassedInductionInNorthernIrelandId
        },
        {
            RouteToProfessionalStatus.ScotlandRId,
            "GB-SCT",
            true,
            "Scotland",
            "UK establishment (Scotland/Northern Ireland)",
            "68",
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId
        }
    };

    public static TheoryData<Guid> OverseasRouteTypeData { get; } = new()
    {
        {
            RouteToProfessionalStatus.ApplyforQtsId
        },
        {
            RouteToProfessionalStatus.EuropeanRecognitionId
        },
        {
            RouteToProfessionalStatus.OverseasTrainedTeacherRecognitionId
        },
        {
            RouteToProfessionalStatus.NiRId
        },
        {
            RouteToProfessionalStatus.ScotlandRId
        }
    };

    public static TheoryData<Guid, string?> RouteTypeWhichCanHaveInductionExemptionData { get; } = new()
    {
        {
            RouteToProfessionalStatus.ApplyforQtsId,
            "PT"
        },
        {
            RouteToProfessionalStatus.QtlsAndSetMembershipId,
            null
        },
        {
            RouteToProfessionalStatus.ScotlandRId,
            "GB-SCT"
        },
        {
            RouteToProfessionalStatus.NiRId,
            "GB-NIR"
        }
    };

    public static TheoryData<Guid, string?, string?> ExistingQtsData { get; } = new()
    {
        {
            RouteToProfessionalStatus.EarlyYearsIttGraduateEntryId,
            null,
            "220"
        },
        {
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            "211",
            null
        },
        {
            RouteToProfessionalStatus.AssessmentOnlyRouteId,
            "212",
            null
        },
        {
            RouteToProfessionalStatus.EarlyYearsIttGraduateEntryId,
            null,
            null
        },
        {
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            null,
            null
        }
    };

    public static TheoryData<dfeta_ITTProgrammeType, string?, string?, dfeta_ITTResult, Guid, dfeta_ITTProgrammeType, ProfessionalStatusStatus, dfeta_ITTResult, string?, string?> UpdatedRouteData { get; } = new()
    {
        {
            dfeta_ITTProgrammeType.EYITTGraduateEntry,                  // ExistingIttProgrammeType
            null,                                                       // ExistingTeacherStatusValue
            "220",                                                      // ExistingEarlyYearsStatusValue
            dfeta_ITTResult.InTraining,                                 // ExistingIttResult
            RouteToProfessionalStatus.EarlyYearsIttUndergraduateId,     // RouteTypeId
            dfeta_ITTProgrammeType.EYITTUndergraduate,                  // ExpectedIttProgrammeType
            ProfessionalStatusStatus.Awarded,                           // Status
            dfeta_ITTResult.Pass,                                       // ExpectedIttResult
            null,                                                       // ExpectedTeacherStatusValue
            "221"                                                       // ExpectedEarlyYearsStatusValue
        },
        {
            dfeta_ITTProgrammeType.HEI,
            "211",
            null,
            dfeta_ITTResult.InTraining,
            RouteToProfessionalStatus.SchoolDirectTrainingProgrammeId,
            dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme,
            ProfessionalStatusStatus.Awarded,
            dfeta_ITTResult.Pass,
            "71",
            null
        },
        {
            dfeta_ITTProgrammeType.AssessmentOnlyRoute,
            "212",
            null,
            dfeta_ITTResult.UnderAssessment,
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            dfeta_ITTProgrammeType.HEI,
            ProfessionalStatusStatus.Awarded,
            dfeta_ITTResult.Pass,
            "71",
            null
        },
        {
            dfeta_ITTProgrammeType.AssessmentOnlyRoute,
            "212",
            null,
            dfeta_ITTResult.UnderAssessment,
            RouteToProfessionalStatus.HeiProgrammeTypeId,
            dfeta_ITTProgrammeType.HEI,
            ProfessionalStatusStatus.InTraining,
            dfeta_ITTResult.InTraining,
            "211",
            null
        },
        {
            dfeta_ITTProgrammeType.HEI,
            "211",
            null,
            dfeta_ITTResult.InTraining,
            RouteToProfessionalStatus.AssessmentOnlyRouteId,
            dfeta_ITTProgrammeType.AssessmentOnlyRoute,
            ProfessionalStatusStatus.UnderAssessment,
            dfeta_ITTResult.UnderAssessment,
            "212",
            null
        },
        {
            dfeta_ITTProgrammeType.EYITTGraduateEntry,
            null,
            null,
            dfeta_ITTResult.InTraining,
            RouteToProfessionalStatus.EarlyYearsIttUndergraduateId,
            dfeta_ITTProgrammeType.EYITTUndergraduate,
            ProfessionalStatusStatus.Awarded,
            dfeta_ITTResult.Pass,
            null,
            "221"
        },
        {
            dfeta_ITTProgrammeType.HEI,
            null,
            null,
            dfeta_ITTResult.Withdrawn,
            RouteToProfessionalStatus.SchoolDirectTrainingProgrammeId,
            dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme,
            ProfessionalStatusStatus.Awarded,
            dfeta_ITTResult.Pass,
            "71",
            null
        },
        {
            dfeta_ITTProgrammeType.HEI,
            null,
            null,
            dfeta_ITTResult.Withdrawn,
            RouteToProfessionalStatus.SchoolDirectTrainingProgrammeId,
            dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme,
            ProfessionalStatusStatus.Withdrawn,
            dfeta_ITTResult.Withdrawn,
            null,
            null
        },
        {
            dfeta_ITTProgrammeType.HEI,
            "211",
            null,
            dfeta_ITTResult.Fail,
            RouteToProfessionalStatus.AssessmentOnlyRouteId,
            dfeta_ITTProgrammeType.AssessmentOnlyRoute,
            ProfessionalStatusStatus.Failed,
            dfeta_ITTResult.Fail,
            "212",
            null
        }
    };
}
