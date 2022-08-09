using System;
using System.Net.Http;
using DqtApi.DataStore.Crm.Models;
using DqtApi.TestCommon;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
    public class GetTeacherIdentityInfoByTsPersonIdTests : ApiTestBase
    {
        public GetTeacherIdentityInfoByTsPersonIdTests(ApiFixture apiFixture)
            : base(apiFixture)
        {
        }

        [Fact]
        public async Task Given_teacher_exists_with_specified_tspersonid_returns_trn()
        {
            // Arrange
            var tsPersonId = Guid.NewGuid().ToString();
            var trn = "1234567";

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacherByTsPersonId(tsPersonId, /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync(new Contact()
                {
                    Id = Guid.NewGuid(),
                    dfeta_TSPersonID = tsPersonId,
                    dfeta_TRN = trn
                });

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/teacher-identity?tsPersonId={tsPersonId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                new
                {
                    trn = trn,
                    tsPersonId = tsPersonId
                });
        }

        [Fact]
        public async Task Given_teacher_does_not_exist_with_specified_tspersonid_returns_NotFound()
        {
            // Arrange
            var tsPersonId = Guid.NewGuid().ToString();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacherByTsPersonId(tsPersonId, /* columnNames: */ It.IsAny<string[]>()))
                .ReturnsAsync((Contact)null);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/teacher-identity?tsPersonId={tsPersonId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, 10015, expectedStatusCode: StatusCodes.Status404NotFound);
        }
    }
}
