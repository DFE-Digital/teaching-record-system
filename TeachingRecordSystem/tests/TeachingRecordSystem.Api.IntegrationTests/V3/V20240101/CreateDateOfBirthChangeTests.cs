using System.Text;
using JustEat.HttpClientInterception;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240101;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateDateOfBirthChangeTests : TestBase
{
    public CreateDateOfBirthChangeTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: [ApiRoles.UpdatePerson])]
    public async Task PostCreateDateOfBirthChange_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var trn = await TestData.GenerateTrnAsync();
        var newDateOfBirth = TestData.GenerateDateOfBirth();
        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        HostFixture.ConfigureEvidenceFilesHttpClient(options =>
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
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(false, "1990-07-01", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "1990-07-01", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "1990-07-01", null, "https://place.com/evidence.jpg")]
    [InlineData(true, "1990-07-01", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        bool includeTrn,
        string? newDateOfBirthString,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());

        var newDateOfBirth = newDateOfBirthString is not null ? DateOnly.ParseExact(newDateOfBirthString, "yyyy-MM-dd") : (DateOnly?)null;

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                trn = includeTrn ? createPersonResult.Trn : "",
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_TeacherWithTrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = await TestData.GenerateTrnAsync();
        var newDateOfBirth = TestData.GenerateDateOfBirth();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        HostFixture.ConfigureEvidenceFilesHttpClient(options =>
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
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, 10001, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(currentDateOfBirth: createPersonResult.DateOfBirth);

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teachers/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                trn = createPersonResult.Trn,
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, 10028, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesIncident()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(currentDateOfBirth: createPersonResult.DateOfBirth);

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        HostFixture.ConfigureEvidenceFilesHttpClient(options =>
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
                trn = createPersonResult.Trn,
                dateOfBirth = newDateOfBirth,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        var crmQuery = new QueryByAttribute(Incident.EntityLogicalName);
        crmQuery.AddAttributeValue(Incident.Fields.CustomerId, new EntityReference(Contact.EntityLogicalName, createPersonResult.ContactId));
        var crmResults = TestData.OrganizationService.RetrieveMultiple(crmQuery);
        Assert.NotEmpty(crmResults.Entities);
    }
}
