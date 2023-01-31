using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using DqtApi.TestCommon;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
    public class UnlockTeacherTests : ApiTestBase
    {
        public UnlockTeacherTests(ApiFixture apiFixture) : base(apiFixture)
        {
        }

        [Fact]
        public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
                .ReturnsAsync((Contact)null);

            var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Given_a_teacher_that_does_exist_and_is_locked_returns_ok()
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
                .ReturnsAsync(new Contact()
                {
                    Id = teacherId,
                    dfeta_ActiveSanctions = false,
                    dfeta_loginfailedcounter = 3
                });

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UnlockTeacherRecord(teacherId))
                .ReturnsAsync(true)
                .Verifiable();

            var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                new
                {
                    hasBeenUnlocked = true
                });
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task Given_a_teacher_that_does_exist_but_is_not_locked_returns_ok(int? loginFailedCounter)
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
                .ReturnsAsync(new Contact()
                {
                    Id = teacherId,
                    dfeta_ActiveSanctions = false,
                    dfeta_loginfailedcounter = loginFailedCounter
                });

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.UnlockTeacherRecord(teacherId))
                .ReturnsAsync(true)
                .Verifiable();

            var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                new
                {
                    hasBeenUnlocked = false
                });
        }

        [Fact]
        public async Task Given_a_teacher_that_has_activesanctions_returns_error()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var teacher = new Contact() { dfeta_ActiveSanctions = true };

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacher(teacherId, It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(teacher);

            var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            await AssertEx.ResponseIsError(response, errorCode: 10014, expectedStatusCode: StatusCodes.Status400BadRequest);
        }
    }
}
