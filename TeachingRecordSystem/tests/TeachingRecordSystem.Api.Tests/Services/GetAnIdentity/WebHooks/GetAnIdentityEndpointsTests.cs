using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Moq;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using TeachingRecordSystem.Api.Services.GetAnIdentity.WebHooks;
using TeachingRecordSystem.Api.Services.GetAnIdentityApi;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.Services.GetAnIdentity.WebHooks;

public class GetAnIdentityEndpointsTests : ApiTestBase
{
    public GetAnIdentityEndpointsTests(ApiFixture apiFixture)
       : base(apiFixture)
    {
    }

    [Fact]
    public async Task Post_WithNoSignatureInHeader_ReturnsUnauthorised()
    {
        // Arrange
        var httpClient = ApiFixture.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidSignatureInHeader_ReturnsUnauthorised()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var signature = "InvalidSignature";
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidContent_ThrowsJsonException()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            UnexpectedProperty = "blah blah"
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act / Assert
        await Assert.ThrowsAsync<JsonException>(() => httpClient.SendAsync(request));
    }

    [Fact]
    public async Task Post_WithMessageTypeWeAreNotInterestedIn_ThrowsJsonException()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = "AMessageTypeWeAreNotInterestedIn",
            Message = new
            {
                Stuff = "Blah Blah"
            }
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act / Assert
        await Assert.ThrowsAsync<JsonException>(() => httpClient.SendAsync(request));
    }

    [Fact]
    public async Task Post_WithNonJsonMessage_ThrowsJsonException()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = (string?)null
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act / Assert
        await Assert.ThrowsAsync<JsonException>(() => httpClient.SendAsync(request));
    }

    [Fact]
    public async Task Post_WithInvalidMessageFormat_ThrowsJsonException()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = new
            {
                UnexpectedProperty = "blah blah"
            }
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act / Assert
        await Assert.ThrowsAsync<JsonException>(() => httpClient.SendAsync(request));
    }

    [Fact]
    public async Task Post_WithValidUserUpdatedMessage_ReturnsNoContent()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = new
            {
                User = new
                {
                    UserId = Guid.NewGuid(),
                    EmailAddress = Faker.Internet.Email(),
                    Trn = "7654321",
                    MobileNumber = "07968987654"
                },
                Changes = new { }
            }
        };

        UpdateTeacherIdentityInfoCommand? actualCommand = null;
        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());
        ApiFixture.DataverseAdapter
            .Setup(d => d.UpdateTeacherIdentityInfo(It.IsAny<UpdateTeacherIdentityInfoCommand>()))
            .Returns(Task.CompletedTask)
            .Callback<UpdateTeacherIdentityInfoCommand>(c => actualCommand = c);

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        Assert.Equal(content.Message.User.UserId, actualCommand?.IdentityUserId);
        Assert.Equal(content.Message.User.EmailAddress, actualCommand?.EmailAddress);
        Assert.Equal(content.Message.User.MobileNumber, actualCommand?.MobilePhone);
        Assert.Equal(content.TimeUtc, actualCommand?.UpdateTimeUtc);
    }

    [Fact]
    public async Task Post_WithUserWithoutTrn_DoesNotCallDqt()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = new
            {
                User = new
                {
                    UserId = Guid.NewGuid(),
                    EmailAddress = Faker.Internet.Email(),
                    Trn = (string?)null,
                    MobileNumber = "07968987654"
                },
                Changes = new { }
            }
        };

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(content)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        ApiFixture.DataverseAdapter.Verify(mock => mock.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Post_WithValidUserCreatedMessage_ReturnsNoContent()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserCreatedMessage.MessageTypeName,
            Message = new
            {
                User = new
                {
                    UserId = Guid.NewGuid(),
                    EmailAddress = Faker.Internet.Email(),
                    Trn = "7654321",
                    MobileNumber = "07968987654"
                }
            }
        };

        UpdateTeacherIdentityInfoCommand? actualCommand = null;
        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());
        ApiFixture.DataverseAdapter
            .Setup(d => d.UpdateTeacherIdentityInfo(It.IsAny<UpdateTeacherIdentityInfoCommand>()))
            .Returns(Task.CompletedTask)
            .Callback<UpdateTeacherIdentityInfoCommand>(c => actualCommand = c);

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        Assert.Equal(content.Message.User.UserId, actualCommand?.IdentityUserId);
        Assert.Equal(content.Message.User.EmailAddress, actualCommand?.EmailAddress);
        Assert.Equal(content.Message.User.MobileNumber, actualCommand?.MobilePhone);
        Assert.Equal(content.TimeUtc, actualCommand?.UpdateTimeUtc);
    }

    [Fact]
    public async Task Post_WithUserUpdatedMessageForRemovedTrn_RemovesLinkFromDqt()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = CreateOptions(clientSecret);

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var identityUserId = Guid.NewGuid();
        var timeUtc = DateTime.UtcNow;

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = timeUtc,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = new
            {
                User = new
                {
                    UserId = identityUserId,
                    EmailAddress = Faker.Internet.Email(),
                    Trn = (string?)null,
                    MobileNumber = "07968987654"
                },
                Changes = new
                {
                    Trn = (string?)null
                }
            }
        };

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(clientSecret, jsonContent);
        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        ApiFixture.DataverseAdapter.Verify(mock => mock.ClearTeacherIdentityInfo(identityUserId, timeUtc));
    }

    private static StringContent CreateJsonContent(string json) => new(json, Encoding.UTF8, "application/json");

    private static GetAnIdentityOptions CreateOptions(string clientSecret) => new()
    {
        WebHookClientSecret = clientSecret,
        BaseAddress = "dummy",
        ClientId = "dummy",
        ClientSecret = "dummy",
        TokenEndpoint = "dummy"
    };

    private static string GenerateSignature(string secret, string content)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var source = Encoding.UTF8.GetBytes(content);
        return Convert.ToHexString(HMACSHA256.HashData(key, source));
    }
}
