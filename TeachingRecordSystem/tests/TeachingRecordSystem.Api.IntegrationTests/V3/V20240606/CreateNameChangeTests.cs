using System.Text;
using JustEat.HttpClientInterception;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240606;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateNameChangeTests : TestBase
{
    public CreateNameChangeTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Theory]
    [InlineData(null, "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("First", "Middle", null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("First", "Middle", "Last", null, "https://place.com/evidence.jpg")]
    [InlineData("First", "Middle", "Last", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        string? newFirstName,
        string? newMiddleName,
        string? newLastName,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/name-changes")
        {
            Content = CreateJsonContent(new
            {
                firstName = newFirstName,
                middleName = newMiddleName,
                lastName = newLastName,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(createPersonResult.Trn!).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/name-changes")
        {
            Content = CreateJsonContent(new
            {
                firstName = newFirstName,
                middleName = newMiddleName,
                lastName = newLastName,
                evidenceFileName,
                evidenceFileUrl
            })
        };

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(createPersonResult.Trn!).SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, 10028, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesSupportTaskAndSendsEmailAndReturnsTicketNumber()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

        var emailAddress = Faker.Internet.Email();
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

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/name-changes")
        {
            Content = CreateJsonContent(new
            {
                firstName = newFirstName,
                middleName = newMiddleName,
                lastName = newLastName,
                evidenceFileName,
                evidenceFileUrl,
                emailAddress
            })
        };

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(createPersonResult.Trn!).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.PersonId == createPersonResult.PersonId);
            Assert.NotNull(supportTask);
            Assert.Equal(SupportTaskType.ChangeNameRequest, supportTask.SupportTaskType);
            var requestData = supportTask.Data as ChangeNameRequestData;
            Assert.NotNull(requestData);
            Assert.Equal(newFirstName, requestData.FirstName);
            Assert.Equal(newMiddleName, requestData.MiddleName);
            Assert.Equal(newLastName, requestData.LastName);
            Assert.Equal(evidenceFileName, requestData.EvidenceFileName);

            var email = await dbContext.Emails
                .Where(e => e.EmailAddress == emailAddress)
                .SingleOrDefaultAsync();
            Assert.NotNull(email);
            Assert.NotNull(email.SentOn);

            await AssertEx.JsonResponseEqualsAsync(
                response,
                expected: new
                {
                    caseNumber = supportTask.SupportTaskReference
                });
        });
    }
}
