using System;
using System.Net.Http;
using System.Net.Http.Json;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql.Models;
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
                .Setup(mock => mock.GetTeacherAsync(teacherId, /* resolveMerges: */ true, It.IsAny<string[]>()))
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
                propertyName: nameof(TrnRequest.RequestId),
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
                propertyName: nameof(TrnRequest.RequestId),
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
                    Subject2 = "Mathematics"
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
