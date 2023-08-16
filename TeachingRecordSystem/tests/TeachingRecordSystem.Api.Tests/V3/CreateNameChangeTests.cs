using System.Text;
using JustEat.HttpClientInterception;

namespace TeachingRecordSystem.Api.Tests.V3;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateNameChangeTests : ApiTestBase
{
    public CreateNameChangeTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Theory]
    [InlineData(null, "First", "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1234567", null, "Middle", "Last", "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1234567", "First", "Middle", null, "evidence.jpg", "https://place.com/evidence.jpg")]
    [InlineData("1234567", "First", "Middle", "Last", null, "https://place.com/evidence.jpg")]
    [InlineData("1234567", "First", "Middle", "Last", "evidence.jpg", null)]
    public async Task Post_InvalidRequest_ReturnsBadRequest(
        string? trn,
        string? newFirstName,
        string? newMiddleName,
        string? newLastName,
        string? evidenceFileName,
        string? evidenceFileUrl)
    {
        // Arrange
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
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_TeacherWithTrnDoesNotExist_ReturnsBadRequest()
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

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync((Contact?)null);

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
        var trn = "1234567";
        var contactId = Guid.NewGuid();
        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();
        var evidenceFileContent = Encoding.UTF8.GetBytes("Test file");

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
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
        await AssertEx.JsonResponseIsError(response, 10028, StatusCodes.Status400BadRequest);
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

        DataverseAdapterMock
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

        DataverseAdapterMock
            .Verify(mock => mock.CreateNameChangeIncident(It.Is<CreateNameChangeIncidentCommand>(cmd =>
                cmd.ContactId == contactId &&
                cmd.FirstName == newFirstName &&
                cmd.MiddleName == newMiddleName &&
                cmd.LastName == newLastName &&
                cmd.EvidenceFileName == evidenceFileName &&
                cmd.EvidenceFileMimeType == "text/plain" &&
                cmd.FromIdentity == true)));
    }
}
