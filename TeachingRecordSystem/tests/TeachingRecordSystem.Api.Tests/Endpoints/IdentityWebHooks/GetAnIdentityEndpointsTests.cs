using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;
using TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Tests.Endpoints.IdentityWebHooks;

public class GetAnIdentityEndpointsTests : TestBase
{
    public GetAnIdentityEndpointsTests(HostFixture hostFixture)
       : base(hostFixture)
    {
    }

    private IOptions<GetAnIdentityOptions> GetAnIdentityOptions => HostFixture.Services.GetRequiredService<IOptions<GetAnIdentityOptions>>();

    [Fact]
    public async Task Post_WithNoSignatureInHeader_ReturnsUnauthorised()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

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
        var signature = "InvalidSignature";
        var httpClient = HostFixture.CreateClient();
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
        var content = new
        {
            UnexpectedProperty = "blah blah"
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
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
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
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
        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = (string?)null
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
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
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act / Assert
        await Assert.ThrowsAsync<JsonException>(() => httpClient.SendAsync(request));
    }

    [Fact]
    public async Task Post_WithValidUserMergedMessage_ReturnsNoContent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var masterContactId = Guid.NewGuid();
        var mergedContactId = Guid.NewGuid();
        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = now,
            MessageType = UserMergedMessage.MessageTypeName,
            Message = new
            {
                MasterUser = new
                {
                    UserId = masterContactId,
                    EmailAddress = Faker.Internet.Email(),
                    Trn = "7654321",
                    MobileNumber = "07968987654"
                },
                MergedUserId = mergedContactId,
                Changes = new { }
            }
        };

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        DataverseAdapterMock.Verify(mock => mock.ClearTeacherIdentityInfo(mergedContactId, now));
    }

    [Fact]
    public async Task Post_WithValidUserUpdatedMessage_ReturnsNoContent()
    {
        // Arrange
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
        DataverseAdapterMock
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());
        DataverseAdapterMock
            .Setup(d => d.UpdateTeacherIdentityInfo(It.IsAny<UpdateTeacherIdentityInfoCommand>()))
            .Returns(Task.CompletedTask)
            .Callback<UpdateTeacherIdentityInfoCommand>(c => actualCommand = c);

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
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

        DataverseAdapterMock
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(content)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        DataverseAdapterMock.Verify(mock => mock.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Post_WithValidUserCreatedMessage_ReturnsNoContent()
    {
        // Arrange
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
        DataverseAdapterMock
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());
        DataverseAdapterMock
            .Setup(d => d.UpdateTeacherIdentityInfo(It.IsAny<UpdateTeacherIdentityInfoCommand>()))
            .Returns(Task.CompletedTask)
            .Callback<UpdateTeacherIdentityInfoCommand>(c => actualCommand = c);

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
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

        DataverseAdapterMock
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact());

        var jsonContent = JsonSerializer.Serialize(content, GetAnIdentityEndpoints.SerializerOptions);
        var signature = GenerateSignature(GetAnIdentityOptions.Value.WebHookClientSecret, jsonContent);
        var httpClient = HostFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Hub-Signature-256", signature);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/identity")
        {
            Content = CreateJsonContent(jsonContent)
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        DataverseAdapterMock.Verify(mock => mock.ClearTeacherIdentityInfo(identityUserId, timeUtc));
    }

    private static StringContent CreateJsonContent(string json) => new(json, Encoding.UTF8, "application/json");

    private static string GenerateSignature(string secret, string content)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var source = Encoding.UTF8.GetBytes(content);
        return Convert.ToHexString(HMACSHA256.HashData(key, source));
    }
}
