using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;
using Xunit.Sdk;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.NoMatches;

public class CheckAnswersTests : ResolveNpqTrnRequestTestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse()
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });
    }

    [Fact]
    public async Task Get_CreatingNewRecord_HasBackLinkToLandingPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var expectedBackLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/no-matches/check-answers");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
    }

    [Fact]
    public async Task Post_CreatingNewRecord_CreatesRecordUpdatesSupportTaskPublishesEventAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var requestMetadata = supportTask.TrnRequestMetadata;
        Assert.NotNull(requestMetadata);
        var comments = Faker.Lorem.Paragraph();

        EventPublisher.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/no-matches/check-answers");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        // redirect
        Assert.Equal("/support-tasks/npq-trn-requests", response.Headers.Location?.OriginalString);

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        var linkToPersonRecord = GetLinkToPersonFromBanner(nextPageDoc);
        Assert.NotNull(linkToPersonRecord);
        var personId = Guid.Parse(linkToPersonRecord!.Substring("/persons/".Length));

        // person record is updated
        await WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons
                .SingleAsync(p => p.PersonId == personId);
            Assert.Equal(person.FirstName, requestMetadata.FirstName);
            Assert.Equal(person.MiddleName, requestMetadata.MiddleName);
            Assert.Equal(person.LastName, requestMetadata.LastName);
            Assert.Equal(person.DateOfBirth, requestMetadata.DateOfBirth);
            Assert.Equal(person.EmailAddress, requestMetadata.EmailAddress);
            Assert.Equal(person.NationalInsuranceNumber, requestMetadata.NationalInsuranceNumber);
        });

        // support task is updated
        await WithDbContext(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            Assert.Equal(personId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            AssertPersonAttributesMatch(supportTaskData.ResolvedAttributes, new NpqTrnRequestDataPersonAttributes()
            {
                FirstName = requestMetadata.FirstName!,
                MiddleName = requestMetadata.MiddleName ?? string.Empty,
                LastName = requestMetadata.LastName!,
                DateOfBirth = requestMetadata.DateOfBirth,
                EmailAddress = requestMetadata.EmailAddress,
                NationalInsuranceNumber = requestMetadata.NationalInsuranceNumber,
                Gender = requestMetadata.Gender
            });
        });

        // event is published
        var expectedMetadata = EventModels.TrnRequestMetadata.FromModel(requestMetadata) with
        {
            ResolvedPersonId = personId
        };
        EventPublisher.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<NpqTrnRequestSupportTaskUpdatedEvent>(e);
            AssertSupportTaskEventIsExpected(actualEvent, expectedPersonId: personId);

            AssertTrnRequestMetadataMatches(expectedMetadata, actualEvent.RequestData);
            Assert.Equal(requestMetadata.NpqEvidenceFileId, actualEvent.RequestData?.NpqEvidenceFileId);
            Assert.Equal(requestMetadata.NpqEvidenceFileName, actualEvent.RequestData?.NpqEvidenceFileName);
        });
    }

    public string? GetLinkToPersonFromBanner(IHtmlDocument doc, string? expectedHeading = null, string? expectedMessage = null)
    {
        var banner = doc.GetElementsByClassName("govuk-notification-banner--success").SingleOrDefault();

        if (banner is null)
        {
            throw new XunitException("No notification banner found.");
        }
        var link = banner.QuerySelector(".govuk-link");

        var href = link?.GetAttribute("href");
        return href;
    }

    private void AssertSupportTaskEventIsExpected(
        NpqTrnRequestSupportTaskUpdatedEvent @event,
        Guid expectedPersonId)
    {
        Assert.Equal(expectedPersonId, @event.PersonId);
        Assert.Equal(Clock.UtcNow, @event.CreatedUtc);
        Assert.Equal(SupportTaskStatus.Open, @event.OldSupportTask.Status);
        Assert.Equal(SupportTaskStatus.Closed, @event.SupportTask.Status);
        Assert.Equal(NpqTrnRequestSupportTaskUpdatedEventChanges.None, @event.Changes);
    }

    private void AssertPersonAttributesMatch(
        NpqTrnRequestDataPersonAttributes? personAttributes,
        NpqTrnRequestDataPersonAttributes expectedPersonAttributes)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.DateOfBirth, expectedPersonAttributes.DateOfBirth);
        Assert.Equal(personAttributes.EmailAddress, expectedPersonAttributes.EmailAddress);
        Assert.Equal(personAttributes.NationalInsuranceNumber, expectedPersonAttributes.NationalInsuranceNumber);
    }

    private void AssertTrnRequestMetadataMatches(EventModels.TrnRequestMetadata expected, EventModels.TrnRequestMetadata actual)
    {
        Assert.Equal(expected.FirstName, actual.FirstName);
        Assert.Equal(expected.MiddleName, actual.MiddleName);
        Assert.Equal(expected.LastName, actual.LastName);
        Assert.Equal(expected.EmailAddress, actual.EmailAddress);
        Assert.Equal(expected.NationalInsuranceNumber, actual.NationalInsuranceNumber);
        Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
        Assert.Equal(expected.ResolvedPersonId, actual.ResolvedPersonId);
        Assert.Equivalent(expected.Matches, actual.Matches);
    }
}
