using System.Text;
using JustEat.HttpClientInterception;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240307;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateDateOfBirthChangeTests : TestBase
{
    private const string RequestPath = "/v3/teachers/date-of-birth-changes";

    public CreateDateOfBirthChangeTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    private void ConfigureEvidenceFile(string evidenceFileUrl)
    {
        HostFixture.ConfigureEvidenceFilesHttpClient(options =>
        {
            var uri = new Uri(evidenceFileUrl);
            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForGet()
                .ForHttps()
                .ForHost(uri.Host)
                .ForPath(uri.LocalPath.TrimStart('/'))
                .Responds()
                .WithContentStream(() => new MemoryStream(Encoding.UTF8.GetBytes("Test file")))
                .RegisterWith(options);
        });
    }

    [Theory, RoleNamesData(except: [ApiRoles.UpdatePerson])]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                trn = "1234567",
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.jpg",
                evidenceFileUrl = "https://place.com/evidence.jpg"
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(false, "1990-01-01", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "1990-01-01", null, "https://place.com/evidence.jpg")]
    [InlineData(true, "1990-01-01", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        bool includeTrn,
        string? dateOfBirth,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                trn = includeTrn ? person.Trn : "",
                dateOfBirth,
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
    public async Task Post_TeacherWithTrnDoesNotExist_ReturnsError()
    {
        // Arrange
        var evidenceFileUrl = "https://place.com/evidence.jpg";
        ConfigureEvidenceFile(evidenceFileUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                trn = "0000000",
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.jpg",
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
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                trn = person.Trn,
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.txt",
                evidenceFileUrl = "https://place.com/does-not-exist.txt"
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, 10028, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesDateOfBirthChangeRequestAndReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var evidenceFileUrl = "https://place.com/evidence.jpg";
        ConfigureEvidenceFile(evidenceFileUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                trn = person.Trn,
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.jpg",
                evidenceFileUrl
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t =>
                t.PersonId == person.PersonId && t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest);
            Assert.NotNull(supportTask);
        });
    }
}
