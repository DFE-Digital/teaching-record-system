using System.Text;
using JustEat.HttpClientInterception;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240416;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateDateOfBirthChangeTests : TestBase
{
    private const string RequestPath = "/v3/teacher/date-of-birth-changes";

    public CreateDateOfBirthChangeTests(HostFixture hostFixture) : base(hostFixture)
    {
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

    [Fact]
    public async Task Post_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.jpg",
                evidenceFileUrl = "https://place.com/evidence.jpg"
            })
        };

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1990-01-01", null, "https://place.com/evidence.jpg")]
    [InlineData("1990-01-01", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        string? dateOfBirth,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var httpClient = GetHttpClientWithIdentityAccessToken(person.Trn);

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new { dateOfBirth, evidenceFileName, evidenceFileUrl })
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var httpClient = GetHttpClientWithIdentityAccessToken(person.Trn);

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.txt",
                evidenceFileUrl = "https://place.com/does-not-exist.txt"
            })
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, 10028, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesDateOfBirthChangeRequestAndReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var evidenceFileUrl = "https://place.com/evidence.jpg";
        ConfigureEvidenceFile(evidenceFileUrl);
        var httpClient = GetHttpClientWithIdentityAccessToken(person.Trn);

        var request = new HttpRequestMessage(HttpMethod.Post, RequestPath)
        {
            Content = CreateJsonContent(new
            {
                dateOfBirth = new DateOnly(1990, 1, 1),
                evidenceFileName = "evidence.jpg",
                evidenceFileUrl
            })
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t =>
                t.PersonId == person.PersonId && t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest);
            Assert.NotNull(supportTask);
        });
    }
}
