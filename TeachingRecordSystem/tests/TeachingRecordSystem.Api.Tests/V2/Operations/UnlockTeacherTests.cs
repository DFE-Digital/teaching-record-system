#nullable disable
using System.Net;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

[TestClass]
public class UnlockTeacherTests : ApiTestBase
{
    public UnlockTeacherTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Test]
    public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapter
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync((Contact)null);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task Given_a_teacher_that_does_exist_and_is_locked_returns_ok()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapter
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_ActiveSanctions = false,
                dfeta_loginfailedcounter = 3
            });

        DataverseAdapter
            .Setup(mock => mock.UnlockTeacherRecord(teacherId))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                hasBeenUnlocked = true
            });
    }

    [Test]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Given_a_teacher_that_does_exist_but_is_not_locked_returns_ok(int? loginFailedCounter)
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapter
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_ActiveSanctions = false,
                dfeta_loginfailedcounter = loginFailedCounter
            });

        DataverseAdapter
            .Setup(mock => mock.UnlockTeacherRecord(teacherId))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                hasBeenUnlocked = false
            });
    }

    [Test]
    public async Task Given_a_teacher_that_has_activesanctions_returns_error()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var teacher = new Contact() { dfeta_ActiveSanctions = true };

        DataverseAdapter
            .Setup(mock => mock.GetTeacher(teacherId, It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(teacher);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10014, expectedStatusCode: StatusCodes.Status400BadRequest);
    }
}
