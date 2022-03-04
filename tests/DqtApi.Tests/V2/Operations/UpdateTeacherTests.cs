using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.Properties;
using DqtApi.TestCommon;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Requests;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
    public class UpdateTeacherTests : ApiTestBase
    {
        public UpdateTeacherTests(ApiFixture apiFixture) : base(apiFixture)
        {
        }

        [Fact]
        public async Task Given_invalid_itt_provider_returns_error()
        {
            // Arrange
            var ukprn = "xxx";
            var contact = new Contact() { Id = Guid.NewGuid() };
            var contactList = new[] { contact };
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingIttRecord));

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(ukprn, dob, /* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(contactList);

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}",
                CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10005, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_invalid_itt_subject1_returns_error()
        {
            // Arrange
            var ukprn = "xxx";
            var subject = "xxx";
            var contact = new Contact() { Id = Guid.NewGuid() };
            var contactList = new[] { contact };
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                    .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject1NotFound));

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(ukprn, dob,/* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                    .ReturnsAsync(contactList);

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}",
                CreateRequest(req => req.InitialTeacherTraining.Subject1 = subject));

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10009, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_invalid_itt_subject2_returns_error()
        {
            // Arrange
            var ukprn = "xxx";
            var subject = "xxx";
            var contact = new Contact() { Id = Guid.NewGuid() };
            var contactList = new[] { contact };
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                    .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject2NotFound));

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(ukprn, dob, /* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                    .ReturnsAsync(contactList);

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}",
                CreateRequest(req => req.InitialTeacherTraining.Subject2 = subject));

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10009, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_invalid_qualification_country_returns_error()
        {
            // Arrange
            var ukprn = "xxx";
            var country = "some non existent country country";
            var contact = new Contact() { Id = Guid.NewGuid() };
            var contactList = new[] { contact };
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(ukprn, dob, /* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                    .ReturnsAsync(contactList);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                    .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationCountryNotFound));

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}",
                CreateRequest(req => req.Qualification.CountryCode = country));

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10010, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_invalid_qualification_subject_returns_error()
        {
            // Arrange
            var ukprn = "xxx";
            var subject = "xxx";
            var contact = new Contact() { Id = Guid.NewGuid() };
            var contactList = new[] { contact };
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(ukprn, dob,/* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                    .ReturnsAsync(contactList);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                    .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubjectNotFound));

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}",
                CreateRequest(req => req.Qualification.Subject = subject));

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10009, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_valid_update_succeeds_return_nocontent()
        {
            // Arrange
            var ukprn = "xxx";
            var subject = "xxx";
            var contact = new Contact() { Id = Guid.NewGuid() };
            var contactList = new[] { contact };
            var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(ukprn, dob, /* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                    .ReturnsAsync(contactList);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                    .ReturnsAsync(result);

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}",
                CreateRequest(req => req.Qualification.Subject = subject));

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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
            var ukprn = "xxx";
            var request = CreateRequest(cmd =>
            {
                cmd.InitialTeacherTraining.AgeRangeFrom = ageRangeFrom;
                cmd.InitialTeacherTraining.AgeRangeTo = ageRangeTo;
            });

            // Act
            var response = await HttpClient.PatchAsync(
                $"v2/teachers/update/{ukprn}", request);

            // Assert
            await AssertEx.ResponseIsValidationErrorForProperty(
                response,
                expectedErrorPropertyName,
                expectedErrorMessage);
        }


        [Fact]
        public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
        {
            // Arrange
            var trn = "1234567";
            var dob = new DateOnly(1987, 01, 01);

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(Array.Empty<Contact>());

            var requestBody = new UpdateTeacherRequest()
            {
                BirthDate = dob,
                InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
                {
                    ProviderUkprn = "123456",
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = IttProgrammeType.EYITTUndergraduate,
                    Subject1 = "Mathematics"
                },
                Qualification = new UpdateTeacherRequestRequestQualification()
                {
                    ProviderUkprn = "123456",
                    CountryCode = "XK",
                    Subject = "Computer Science",
                    Class = ClassDivision.Pass,
                    Date = new DateOnly(2022, 01, 01)
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}")
            {
                Content = CreateJsonContent(requestBody)
            };


            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Given_a_two_teachers_have_the_same_trn_return_conflict()
        {
            // Arrange
            var trn = "1000000";

            var contact1 = new Contact();
            var contact2 = new Contact();
            var contactList = new[] { contact1, contact2 };
            var dob = new DateOnly(1987, 01, 01);

            var requestBody = new UpdateTeacherRequest()
            {
                BirthDate = dob,
                InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
                {
                    ProviderUkprn = "123456",
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = IttProgrammeType.EYITTUndergraduate,
                    Subject1 = "Mathematics",
                    Subject2 = "Computer Science",
                    AgeRangeFrom = 1,
                    AgeRangeTo = 10
                },
                Qualification = new UpdateTeacherRequestRequestQualification()
                {
                    ProviderUkprn = "123456",
                    CountryCode = "XK",
                    Subject = "Computer Science",
                    Class = ClassDivision.Pass,
                    Date = new DateOnly(2022, 01, 01)
                }
            };

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ true, /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(contactList);

            var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}")
            {
                Content = CreateJsonContent(requestBody)
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10002, expectedStatusCode: StatusCodes.Status409Conflict);
        }

        private JsonContent CreateRequest(Action<UpdateTeacherRequest> configureRequest = null)
        {
            var request = new UpdateTeacherRequest()
            {
                BirthDate = new DateOnly(1987, 01, 01),
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
    }
}
