#nullable disable
using System.Net;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

public class SetIttOutcomeTests : ApiTestBase
{
    public SetIttOutcomeTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Given_TRN_that_does_not_exist_returns_not_found()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 1, 1);

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(Array.Empty<Contact>());

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = "1001234",
            Outcome = IttOutcome.Pass,
            AssessmentDate = Clock.Today
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10001, expectedStatusCode: StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Given_TRN_that_maps_to_multiple_teachers_return_error()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 1, 1);

        var teacher1 = new Contact() { dfeta_TRN = trn };
        var teacher2 = new Contact() { dfeta_TRN = trn };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(new[] { teacher1, teacher2 });

        var requestBody = new SetIttOutcomeRequest()
        {
            IttProviderUkprn = "1001234",
            Outcome = IttOutcome.Pass,
            AssessmentDate = Clock.Today
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10002, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Given_missing_birthdate_returns_error()
    {
        // Arrange
        var trn = "1234567";

        var requestBody = new SetIttOutcomeRequest()
        {
            IttProviderUkprn = "1001234",
            Outcome = IttOutcome.Pass,
            AssessmentDate = Clock.Today
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(requestBody.BirthDate),
            expectedError: "Birthdate is required.");
    }

    [Fact]
    public async Task Given_missing_IttProviderUkprn_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 1, 1);

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = null,
            Outcome = IttOutcome.Pass,
            AssessmentDate = Clock.Today
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(requestBody.IttProviderUkprn),
            expectedError: "ITT provider UKPRN is required.");
    }

    [Fact]
    public async Task Given_Passed_outcome_and_missing_AssessmentDate_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 1, 1);

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = "1001234",
            Outcome = IttOutcome.Pass,
            AssessmentDate = null
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(requestBody.AssessmentDate),
            expectedError: "Assessment date must be specified when outcome is Pass.");
    }

    [Theory]
    [InlineData(IttOutcome.Fail)]
    [InlineData(IttOutcome.Withdrawn)]
    [InlineData(IttOutcome.Deferred)]
    [InlineData(IttOutcome.DeferredForSkillsTests)]
    public async Task Given_non_Pass_outcome_and_specified_AssessmentDate_returns_error(IttOutcome outcome)
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 1, 1);

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = "1001234",
            Outcome = outcome,
            AssessmentDate = Clock.Today
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(requestBody.AssessmentDate),
            expectedError: "Assessment date cannot be specified unless outcome is Pass.");
    }

    [Fact]
    public async Task Given_AssessmentDate_would_lead_to_QtsDate_in_future_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 1, 1);

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = "1001234",
            Outcome = IttOutcome.Pass,
            AssessmentDate = Clock.Today.AddDays(1)
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(requestBody.AssessmentDate),
            expectedError: "QTS date cannot be in the future.");
    }

    [Fact]
    public async Task Given_teacher_already_has_different_QTS_date_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var ittProviderUkprn = "1001234";
        var outcome = IttOutcome.Pass;
        var assessmentDate = Clock.Today;
        var dob = new DateOnly(1987, 1, 1);

        var contact = new Contact() { dfeta_TRN = trn, Id = Guid.NewGuid() };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(new[] { contact });

        DataverseAdapterMock
            .Setup(mock => mock.SetIttResultForTeacher(contact.Id, ittProviderUkprn, outcome.ConvertToITTResult(), assessmentDate, It.IsAny<string>()))
            .ReturnsAsync(SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.QtsDateMismatch));

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = ittProviderUkprn,
            Outcome = outcome,
            AssessmentDate = assessmentDate
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, 10003, expectedStatusCode: StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Given_teacher_passing_withdrawn_outcome_for_teacher_that_is_withdrawn_do_nothing_without_error()
    {
        // Arrange
        var trn = "1234567";
        var ittProviderUkprn = "1001234";
        var outcome = IttOutcome.Withdrawn;
        var assessmentDate = Clock.Today;
        var dob = new DateOnly(1987, 1, 1);

        var contact = new Contact() { dfeta_TRN = trn, Id = Guid.NewGuid() };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(new[] { contact });

        DataverseAdapterMock
            .Setup(mock => mock.SetIttResultForTeacher(contact.Id, ittProviderUkprn, outcome.ConvertToITTResult(), null, It.IsAny<string>()))
            .ReturnsAsync(SetIttResultForTeacherResult.Success(null));

        var requestBody = new SetIttOutcomeRequest()
        {
            BirthDate = dob,
            IttProviderUkprn = ittProviderUkprn,
            Outcome = outcome
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(SetIttResultForTeacherFailedReason.EytsDateMismatch, 10003)]
    [InlineData(SetIttResultForTeacherFailedReason.QtsDateMismatch, 10003)]
    [InlineData(SetIttResultForTeacherFailedReason.MultipleIttRecords, 10004)]
    [InlineData(SetIttResultForTeacherFailedReason.NoMatchingIttRecord, 10005)]
    [InlineData(SetIttResultForTeacherFailedReason.NoMatchingQtsRecord, 10006)]
    [InlineData(SetIttResultForTeacherFailedReason.MultipleQtsRecords, 10007)]
    public async Task Given_teacher_state_is_invalid_returns_error(
        SetIttResultForTeacherFailedReason failedReason,
        int expectedErrorCode)
    {
        // Arrange
        var trn = "1234567";
        var ittProviderUkprn = "1001234";
        var outcome = IttOutcome.Pass;
        var assessmentDate = Clock.Today;
        var dob = new DateOnly(1987, 1, 1);

        var contact = new Contact() { dfeta_TRN = trn, Id = Guid.NewGuid() };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(new[] { contact });

        DataverseAdapterMock
            .Setup(mock => mock.SetIttResultForTeacher(contact.Id, ittProviderUkprn, outcome.ConvertToITTResult(), assessmentDate, It.IsAny<string>()))
            .ReturnsAsync(SetIttResultForTeacherResult.Failed(failedReason));

        var requestBody = new SetIttOutcomeRequest()
        {
            IttProviderUkprn = ittProviderUkprn,
            Outcome = outcome,
            AssessmentDate = assessmentDate
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode, expectedStatusCode: StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Given_teacher_is_fetched_using_fallback_if_notfound_using_slugid()
    {
        // Arrange
        var trn = "1234567";
        var ittProviderUkprn = "1001234";
        var outcome = IttOutcome.Pass;
        var assessmentDate = Clock.Today;
        var dob = new DateOnly(1987, 1, 1);
        var slugId = Guid.NewGuid().ToString();

        var contact = new Contact() { dfeta_TRN = trn, Id = Guid.NewGuid() };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(Array.Empty<Contact>());

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersBySlugIdAndTrn(slugId, trn,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(Array.Empty<Contact>());

        var requestBody = new SetIttOutcomeRequest()
        {
            IttProviderUkprn = ittProviderUkprn,
            Outcome = outcome,
            AssessmentDate = assessmentDate
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}&slugid={slugId}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        DataverseAdapterMock.Verify(x => x.GetTeachersBySlugIdAndTrn(slugId, trn,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true), Times.Once);
        DataverseAdapterMock.Verify(x => x.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true), Times.Once);
    }

    [Fact]
    public async Task Given_teacher_is_fetched_using_correct_method_without_passing_slugid()
    {
        // Arrange
        var trn = "1234567";
        var ittProviderUkprn = "1001234";
        var outcome = IttOutcome.Pass;
        var assessmentDate = Clock.Today;
        var dob = new DateOnly(1987, 1, 1);
        var slugId = Guid.NewGuid().ToString();

        var contact = new Contact() { dfeta_TRN = trn, Id = Guid.NewGuid() };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(Array.Empty<Contact>());

        DataverseAdapterMock
            .Setup(mock => mock.GetTeachersBySlugIdAndTrn(slugId, trn,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(Array.Empty<Contact>());

        var requestBody = new SetIttOutcomeRequest()
        {
            IttProviderUkprn = ittProviderUkprn,
            Outcome = outcome,
            AssessmentDate = assessmentDate
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/{trn}/itt-outcome?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        DataverseAdapterMock.Verify(x => x.GetTeachersBySlugIdAndTrn(slugId, trn,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true), Times.Never);
        DataverseAdapterMock.Verify(x => x.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true), Times.Once);
    }
}
