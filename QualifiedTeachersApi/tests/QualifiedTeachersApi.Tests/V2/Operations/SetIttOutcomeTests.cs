using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.TestCommon;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using Xunit;

namespace QualifiedTeachersApi.Tests.V2.Operations
{
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

            ApiFixture.DataverseAdapter
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10001, expectedStatusCode: StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task Given_TRN_that_maps_to_multiple_teachers_return_error()
        {
            // Arrange
            var trn = "1234567";
            var dob = new DateOnly(1987, 1, 1);

            var teacher1 = new Contact() { dfeta_TRN = trn };
            var teacher2 = new Contact() { dfeta_TRN = trn };

            ApiFixture.DataverseAdapter
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10002, expectedStatusCode: StatusCodes.Status409Conflict);
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
                response,
                propertyName: nameof(requestBody.AssessmentDate),
                expectedError: "QTS date cannot be in the future.");
        }

        [Fact]
        public async Task Given_teacher_already_has_QTS_date_returns_error()
        {
            // Arrange
            var trn = "1234567";
            var ittProviderUkprn = "1001234";
            var outcome = IttOutcome.Pass;
            var assessmentDate = Clock.Today;
            var dob = new DateOnly(1987, 1, 1);

            var contact = new Contact() { dfeta_TRN = trn, Id = Guid.NewGuid() };

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(new[] { contact });

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.SetIttResultForTeacher(contact.Id, ittProviderUkprn, outcome.ConvertToITTResult(), assessmentDate))
                .ReturnsAsync(SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.AlreadyHaveQtsDate));

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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, 10003, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Theory]
        [InlineData(SetIttResultForTeacherFailedReason.AlreadyHaveEytsDate, 10003)]
        [InlineData(SetIttResultForTeacherFailedReason.AlreadyHaveQtsDate, 10003)]
        [InlineData(SetIttResultForTeacherFailedReason.MultipleInTrainingIttRecords, 10004)]
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

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(new[] { contact });

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.SetIttResultForTeacher(contact.Id, ittProviderUkprn, outcome.ConvertToITTResult(), assessmentDate))
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
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, expectedErrorCode, expectedStatusCode: StatusCodes.Status400BadRequest);
        }
    }
}
