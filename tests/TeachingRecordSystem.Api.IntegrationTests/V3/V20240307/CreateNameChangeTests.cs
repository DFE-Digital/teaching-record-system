using System.Text;
using JustEat.HttpClientInterception;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240307;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateNameChangeTests : TestBase
{
    private const string RequestPath = "/v3/teachers/name-changes";

    public CreateNameChangeTests(HostFixture hostFixture)
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
                firstName = TestData.GenerateFirstName(),
                middleName = TestData.GenerateMiddleName(),
                lastName = TestData.GenerateLastName(),
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
    [InlineData(false, "First", "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, null, "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "First", "Middle", null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData(true, "First", "Middle", "Last", null, "https://place.com/evidence.jpg")]
    [InlineData(true, "First", "Middle", "Last", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        bool includeTrn,
        string? firstName,
        string? middleName,
        string? lastName,
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
                firstName,
                middleName,
                lastName,
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
                firstName = TestData.GenerateFirstName(),
                middleName = TestData.GenerateMiddleName(),
                lastName = TestData.GenerateLastName(),
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
                firstName = TestData.GenerateFirstName(),
                middleName = TestData.GenerateMiddleName(),
                lastName = TestData.GenerateLastName(),
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
    public async Task Post_ValidRequest_CreatesNameChangeRequestAndReturnsNoContent()
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
                firstName = TestData.GenerateFirstName(),
                middleName = TestData.GenerateMiddleName(),
                lastName = TestData.GenerateLastName(),
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
                t.PersonId == person.PersonId && t.SupportTaskType == SupportTaskType.ChangeNameRequest);
            Assert.NotNull(supportTask);
        });
    }
}
