using System;
using System.Net.Http;
using DqtApi.DataStore.Crm.Models;
using DqtApi.TestCommon;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
    public class SetTeacherIdentityInfoTests : ApiTestBase
    {
        public SetTeacherIdentityInfoTests(ApiFixture apiFixture)
            : base(apiFixture)
        {
        }

        [Fact]
        public async Task Given_teacher_does_not_exist_returns_NotFound()
        {
            // Arrange
            var trn = "1234567";
            var teacherId = Guid.NewGuid();
            var tsPersonId = Guid.NewGuid().ToString();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrn(trn, /* activeOnly: */ It.IsAny<bool>(), /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(Array.Empty<Contact>());

            var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/teacher-identity/{trn}")
            {
                Content = CreateJsonContent(new { tsPersonId = tsPersonId })
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10001, expectedStatusCode: StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task Given_teacher_already_has_non_matching_tspersonid_returns_error()
        {
            // Arrange
            var trn = "1234567";
            var teacherId = Guid.NewGuid();
            var tsPersonId = Guid.NewGuid().ToString();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrn(trn, /* activeOnly: */ It.IsAny<bool>(), /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(new Contact[]
                {
                    new Contact()
                    {
                        Id = teacherId,
                        dfeta_TSPersonID = Guid.NewGuid().ToString()
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/teacher-identity/{trn}")
            {
                Content = CreateJsonContent(new { tsPersonId = tsPersonId })
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10017, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_another_teacher_already_has_tspersonid_assigned_returns_error()
        {
            // Arrange
            var trn = "1234567";
            var teacherId = Guid.NewGuid();
            var tsPersonId = Guid.NewGuid().ToString();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrn(trn, /* activeOnly: */ It.IsAny<bool>(), /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(new Contact[]
                {
                    new Contact()
                    {
                        Id = teacherId,
                        dfeta_TSPersonID = null
                    }
                });

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacherByTsPersonId(tsPersonId))
                .ReturnsAsync(new Contact()
                {
                    Id = Guid.NewGuid(),
                    dfeta_TSPersonID = tsPersonId
                });

            var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/teacher-identity/{trn}")
            {
                Content = CreateJsonContent(new { tsPersonId = tsPersonId })
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10016, expectedStatusCode: StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Given_teacher_does_not_have_tspersonid_updates_field_and_returns_ok()
        {
            // Arrange
            var trn = "1234567";
            var teacherId = Guid.NewGuid();
            var tsPersonId = Guid.NewGuid().ToString();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrn(trn, /* activeOnly: */ It.IsAny<bool>(), /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(new Contact[]
                {
                    new Contact()
                    {
                        Id = teacherId,
                        dfeta_TSPersonID = null
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/teacher-identity/{trn}")
            {
                Content = CreateJsonContent(new { tsPersonId = tsPersonId })
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    trn = trn,
                    tsPersonId = tsPersonId
                },
                expectedStatusCode: StatusCodes.Status200OK);

            ApiFixture.DataverseAdapter
                .Verify(mock => mock.SetTsPersonId(teacherId, tsPersonId));
        }

        [Fact]
        public async Task Given_teacher_already_has_matching_tspersonid_returns_ok()
        {
            // Arrange
            var trn = "1234567";
            var teacherId = Guid.NewGuid();
            var tsPersonId = Guid.NewGuid().ToString();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeachersByTrn(trn, /* activeOnly: */ It.IsAny<bool>(), /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(new Contact[]
                {
                    new Contact()
                    {
                        Id = teacherId,
                        dfeta_TSPersonID = tsPersonId
                    }
                });

            var request = new HttpRequestMessage(HttpMethod.Put, $"/v2/teachers/teacher-identity/{trn}")
            {
                Content = CreateJsonContent(new { tsPersonId = tsPersonId })
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    trn = trn,
                    tsPersonId = tsPersonId
                },
                expectedStatusCode: StatusCodes.Status200OK);
        }
    }
}
