using System.Text;
using JustEat.HttpClientInterception;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240606;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateDateOfBirthChangeTests : TestBase
{
    public CreateDateOfBirthChangeTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Theory]
    [InlineData(null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1990-07-01", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1990-07-01", null, "https://place.com/evidence.jpg")]
    [InlineData("1990-07-01", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        string? newDateOfBirthString,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();

        var newDateOfBirth = newDateOfBirthString is not null ? DateOnly.ParseExact(newDateOfBirthString, "yyyy-MM-dd") : (DateOnly?)null;

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                dateOfBirth = newDateOfBirth,
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

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/date-of-birth-changes")
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
        var response = await GetHttpClientWithIdentityAccessToken(trn).SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, 10001, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Post_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(currentDateOfBirth: createPersonResult.DateOfBirth);

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                dateOfBirth = newDateOfBirth,
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
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(currentDateOfBirth: createPersonResult.DateOfBirth);

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

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/person/date-of-birth-changes")
        {
            Content = CreateJsonContent(new
            {
                dateOfBirth = newDateOfBirth,
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
            Assert.Equal(SupportTaskType.ChangeDateOfBirthRequest, supportTask.SupportTaskType);
            var requestData = supportTask.Data as ChangeDateOfBirthRequestData;
            Assert.NotNull(requestData);
            Assert.Equal(newDateOfBirth, requestData.DateOfBirth);
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
