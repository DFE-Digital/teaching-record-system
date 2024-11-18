using System.Text;
using JustEat.HttpClientInterception;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Api.Tests.V3.V20240412;

[Collection(nameof(DisableParallelization))]  // Configures EvidenceFilesHttpClient
public class CreateNameChangeTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teacher/name-changes")
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
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

        var evidenceFileName = "evidence.txt";
        var evidenceFileUrl = Faker.Internet.SecureUrl();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teacher/name-changes")
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
    public async Task Post_ValidRequest_CreatesIncidentAndReturnsTicketNumber()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());
        var newFirstName = TestData.GenerateFirstName();
        var newMiddleName = TestData.GenerateMiddleName();
        var newLastName = TestData.GenerateLastName();

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

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/teacher/name-changes")
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
        var crmQuery = new QueryByAttribute(Incident.EntityLogicalName);
        crmQuery.AddAttributeValue(Incident.Fields.CustomerId, new EntityReference(Contact.EntityLogicalName, createPersonResult.ContactId));
        var crmResults = TestData.OrganizationService.RetrieveMultiple(crmQuery);
        var incident = Assert.Single(crmResults.Entities).ToEntity<Incident>();

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                caseNumber = incident.TicketNumber
            });
    }
}
