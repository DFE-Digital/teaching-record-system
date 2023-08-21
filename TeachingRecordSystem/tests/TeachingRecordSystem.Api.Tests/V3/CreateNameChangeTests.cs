using System.Text;
using JustEat.HttpClientInterception;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Api.Tests.V3;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateNameChangeTests : ApiTestBase
{
    public CreateNameChangeTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Theory]
    [InlineData(false, "First", "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, null, "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "First", "Middle", null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "First", "Middle", "Last", null, "https://place.com/evidence.jpg")]
    [InlineData(true, "First", "Middle", "Last", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        bool includeTrn,
        string? newFirstName,
        string? newMiddleName,
        string? newLastName,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/name-changes")
        {
            Content = CreateJsonContent(new
            {
                trn = includeTrn ? createPersonResult.Trn : "",
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
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_TeacherWithTrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = await TestData.GenerateTrn();
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

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
        await AssertEx.JsonResponseIsError(response, 10001, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson();
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/name-changes")
        {
            Content = CreateJsonContent(new
            {
                trn = createPersonResult.Trn,
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
        await AssertEx.JsonResponseIsError(response, 10028, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesIncident()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson();
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

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
                trn = createPersonResult.Trn,
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

        var crmQuery = new QueryByAttribute(Incident.EntityLogicalName);
        crmQuery.AddAttributeValue(Incident.Fields.CustomerId, new EntityReference(Contact.EntityLogicalName, createPersonResult.ContactId));
        var crmResults = TestData.OrganizationService.RetrieveMultiple(crmQuery);
        Assert.NotEmpty(crmResults.Entities);
    }
}
