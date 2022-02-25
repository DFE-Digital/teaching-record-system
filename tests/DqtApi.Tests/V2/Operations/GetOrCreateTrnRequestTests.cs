using System;
using System.Net.Http;
using System.Net.Http.Json;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql.Models;
using DqtApi.Properties;
using DqtApi.TestCommon;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Requests;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
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
                .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ true, It.IsAny<string[]>()))
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

            // Act
            var response = await HttpClient.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    requestId = requestId,
                    trn = trn,
                    status = "Completed"
                },
                expectedStatusCode: 200);
        }

        [Fact]
        public async Task Given_request_with_id_already_exists_for_client_and_status_is_pending_returns_null_trn()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();

            await WithDbContext(async dbContext =>
            {
                dbContext.Add(new TrnRequest()
                {
                    ClientId = ClientId,
                    RequestId = requestId,
                    TeacherId = null
                });

                await dbContext.SaveChangesAsync();
            });

            // Act
            var response = await HttpClient.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    requestId = requestId,
                    trn = (string)null,
                    status = "Pending"
                },
                expectedStatusCode: 200);
        }

        [Fact]
        public async Task Given_request_with_invalid_id_returns_error()
        {
            // Arrange
            var requestId = "$";

            // Act
            var response = await HttpClient.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
                response,
                propertyName: nameof(GetOrCreateTrnRequest.RequestId),
                expectedError: Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
        }

        [Theory]
        [InlineData("1234567", "Completed")]
        [InlineData(null, "Pending")]
        public async Task Given_request_with_new_id_creates_teacher_and_returns_created(string trn, string expectedStatus)
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var teacherId = Guid.NewGuid();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.CreateTeacher(It.IsAny<CreateTeacherCommand>()))
                .ReturnsAsync(CreateTeacherResult.Success(teacherId, trn))
                .Verifiable();

            // Act
            var response = await HttpClient.PutAsync($"v2/trn-requests/{requestId}", CreateRequest());

            // Assert
            ApiFixture.DataverseAdapter.Verify();

            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    requestId = requestId,
                    trn = trn,
                    status = expectedStatus
                },
                expectedStatusCode: 201);
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
            var response = await HttpClient.PutAsync(
                $"v2/trn-requests/{requestId}",
                CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.PutAsync(
                $"v2/trn-requests/{requestId}",
                CreateRequest(req => req.InitialTeacherTraining.Subject1 = subject));

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.PutAsync(
                $"v2/trn-requests/{requestId}",
                CreateRequest(req => req.InitialTeacherTraining.Subject2 = subject));

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
                response,
                propertyName: $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject2)}",
                expectedError: Properties.StringResources.Errors_10009_Title);
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
            var response = await HttpClient.PutAsync(
                $"v2/trn-requests/{requestId}",
                CreateRequest(req => req.Qualification.CountryCode = country));

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.PutAsync(
                $"v2/trn-requests/{requestId}",
                CreateRequest(req => req.Qualification.Subject = subject));

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
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
            var response = await HttpClient.PutAsync(
                $"v2/trn-requests/{requestId}",
                CreateRequest(req => req.Qualification.ProviderUkprn = ukprn));

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
                response,
                propertyName: $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.ProviderUkprn)}",
                expectedError: Properties.StringResources.Errors_10008_Title);
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
            var response = await HttpClient.PutAsync($"v2/trn-requests/{requestId}", request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
                response,
                expectedErrorPropertyName,
                expectedErrorMessage);
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
                    Subject1 = "Computer Science",
                    Subject2 = "Mathematics",
                    AgeRangeFrom = 5,
                    AgeRangeTo = 11
                },
                Qualification = new()
                {
                    ProviderUkprn = "10044534",
                    CountryCode = "UK",
                    Subject = "Computing",
                    Class = ClassDivision.FirstClassHonours,
                    Date = new(2021, 5, 3)
                }
            };

            configureRequest?.Invoke(request);

            return CreateJsonContent(request);
        }
    }
}
