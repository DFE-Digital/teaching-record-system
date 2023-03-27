﻿using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using Xunit;

namespace QualifiedTeachersApi.Tests.Services.GetAnIdentity.WebHooks;

public class UserUpdatedTests : ApiTestBase
{
    public UserUpdatedTests(ApiFixture apiFixture)
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
        var identityOptions = new GetAnIdentityOptions()
        {
            WebHookClientSecret = clientSecret
        };

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
    public async Task Post_WithInvalidContent_ReturnsBadRequest()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = new GetAnIdentityOptions()
        {
            WebHookClientSecret = clientSecret
        };

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            UnexpectedProperty = "blah blah"
        };

        var jsonContent = JsonSerializer.Serialize(content);
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
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMessageTypeWeAreNotInterestedIn_ReturnsNoContent()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = new GetAnIdentityOptions()
        {
            WebHookClientSecret = clientSecret
        };

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

        var jsonContent = JsonSerializer.Serialize(content);
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
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNonJsonMessage_ReturnsBadRequest()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = new GetAnIdentityOptions()
        {
            WebHookClientSecret = clientSecret
        };

        ApiFixture.GetAnIdentityOptions
            .Setup(o => o.Value)
            .Returns(identityOptions);

        var content = new
        {
            NotificationId = Guid.NewGuid(),
            TimeUtc = DateTime.UtcNow,
            MessageType = UserUpdatedMessage.MessageTypeName,
            Message = (string)null
        };

        var jsonContent = JsonSerializer.Serialize(content);
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
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidMessageFormat_ReturnsBadRequest()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = new GetAnIdentityOptions()
        {
            WebHookClientSecret = clientSecret
        };

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

        var jsonContent = JsonSerializer.Serialize(content);
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
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidMessage_ReturnsNoContent()
    {
        // Arrange
        var clientSecret = "MySecret";
        var identityOptions = new GetAnIdentityOptions()
        {
            WebHookClientSecret = clientSecret
        };

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
                }
            }
        };

        UpdateTeacherIdentityInfoCommand actualCommand = null;
        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new DataStore.Crm.Models.Contact());
        ApiFixture.DataverseAdapter
            .Setup(d => d.UpdateTeacherIdentityInfo(It.IsAny<UpdateTeacherIdentityInfoCommand>()))
            .Returns(Task.CompletedTask)
            .Callback<UpdateTeacherIdentityInfoCommand>(c => actualCommand = c);

        var jsonContent = JsonSerializer.Serialize(content);
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
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        Assert.Equal(content.Message.User.UserId, actualCommand.IdentityUserId);
        Assert.Equal(content.Message.User.EmailAddress, actualCommand.EmailAddress);
        Assert.Equal(content.Message.User.MobileNumber, actualCommand.MobilePhone);
        Assert.Equal(content.TimeUtc, actualCommand.UpdateTimeUtc);
    }

    private string GenerateSignature(string secret, string content)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var source = Encoding.UTF8.GetBytes(content);
        return Convert.ToHexString(HMACSHA256.HashData(key, source));
    }
}
