using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

            ApiFixture.DataverseAdaptor
                .Setup(mock => mock.UnlockTeacherRecordAsync(teacherId))
                .ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Given_a_teacher_that_does_exist_returns_nocontent()
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            ApiFixture.DataverseAdaptor
                .Setup(mock => mock.UnlockTeacherRecordAsync(teacherId))
                .ReturnsAsync(true)
                .Verifiable();

            var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            ApiFixture.DataverseAdaptor.Verify();
        }
    }
}
