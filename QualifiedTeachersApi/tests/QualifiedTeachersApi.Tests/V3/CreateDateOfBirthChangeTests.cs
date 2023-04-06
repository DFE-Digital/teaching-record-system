using System;
using System.IO;
using System.Net.Http;
using System.Text;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.TestCommon;
using Xunit;

namespace QualifiedTeachersApi.Tests.V3;

public class CreateDateOfBirthChangeTests : ApiTestBase
{
    public CreateDateOfBirthChangeTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Theory]
    [InlineData(null, "1990-07-01", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1234567", null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1234567", "1990-07-01", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1234567", "1990-07-01", null, "https://place.com/evidence.jpg")]
    [InlineData("1234567", "1990-07-01", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        string? trn,
        string? newDateOfBirthString,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var newDateOfBirth = newDateOfBirthString is not null ? DateOnly.ParseExact(newDateOfBirthString, "yyyy-MM-dd") : (DateOnly?)null;

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_TeacherWithTrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = "1234567";
        var contactId = Guid.NewGuid();
        var newDateOfBirth = Faker.Identification.DateOfBirth().ToDateOnly();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync((Contact?)null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.ResponseIsError(response, 10001, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var trn = "1234567";
        var contactId = Guid.NewGuid();
        var newDateOfBirth = Faker.Identification.DateOfBirth().ToDateOnly();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = contactId,
                dfeta_TRN = trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.ResponseIsError(response, 10028, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesIncident()
    {
        // Arrange
        var trn = "1234567";
        var contactId = Guid.NewGuid();
        var newDateOfBirth = Faker.Identification.DateOfBirth().ToDateOnly();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = contactId,
                dfeta_TRN = trn
            });

        ApiFixture.ConfigureEvidenceFilesHttpClient(options =>
        {
            var builder = new HttpRequestInterceptionBuilder();

            var evidenceFileUri = new Uri(evidenceFileUrl);

            builder
                .Requests()
                .ForGet()
                .ForHttps()
                .ForHost(evidenceFileUri.Host)
                .ForPath(evidenceFileUri.LocalPath.TrimStart('/'))
                .Responds()
                .WithContentStream(() => new MemoryStream(evidenceFileContent))
                .RegisterWith(options);
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        ApiFixture.DataverseAdapter
            .Verify(mock => mock.CreateDateOfBirthChangeIncident(It.Is<CreateDateOfBirthChangeIncidentCommand>(cmd =>
                cmd.Trn == trn &&
                cmd.ContactId == contactId &&
                cmd.DateOfBirth == newDateOfBirth &&
                cmd.EvidenceFileName == evidenceFileName &&
                cmd.EvidenceFileMimeType == "text/plain")));
    }
}
