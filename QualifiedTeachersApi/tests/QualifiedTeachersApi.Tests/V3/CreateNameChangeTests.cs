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

public class CreateNameChangeTests : ApiTestBase
{
    public CreateNameChangeTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Post_TeacherWithTrnDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var contactId = Guid.NewGuid();
        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync((Contact)null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/name-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                firstName = newFirstName,
                middleName = newMiddleName,
                lastName = newLastName,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.ResponseIsError(response, 10001, StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var trn = "1234567";
        var contactId = Guid.NewGuid();
        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new DataStore.Crm.Models.Contact()
            {
                Id = contactId,
                dfeta_TRN = trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/name-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                firstName = newFirstName,
                middleName = newMiddleName,
                lastName = newLastName,
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
        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new DataStore.Crm.Models.Contact()
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

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/name-changes")
        {
            Content = CreateJsonContent(new
            {
                trn,
                firstName = newFirstName,
                middleName = newMiddleName,
                lastName = newLastName,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        ApiFixture.DataverseAdapter
            .Verify(mock => mock.CreateNameChangeIncident(It.Is<CreateNameChangeIncidentCommand>(cmd =>
                cmd.Trn == trn &&
                cmd.ContactId == contactId &&
                cmd.FirstName == newFirstName &&
                cmd.MiddleName == newMiddleName &&
                cmd.LastName == newLastName &&
                cmd.EvidenceFileName == evidenceFileName &&
                cmd.EvidenceFileMimeType == "text/plain")));
    }
}
