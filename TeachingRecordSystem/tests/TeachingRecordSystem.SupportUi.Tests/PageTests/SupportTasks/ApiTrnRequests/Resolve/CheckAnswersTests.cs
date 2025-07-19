using FakeXrmEasy.Extensions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public class CheckAnswersTests : ResolveApiTrnRequestTestBase
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
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = null });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [MemberData(nameof(PersonAttributeInfoData))]
    public async Task Get_AttributeSourceIsTrnRequest_RendersChosenAttributeValues(PersonAttributeInfo sourcedFromRequestDataAttribute)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            sourcedFromRequestDataAttribute.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveApiTrnRequestState()
        {
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceToTrnRequest(state, sourcedFromRequestDataAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var allSummaryListRowValues = doc.GetElementsByClassName("govuk-summary-list__row")
            .ToDictionary(
                row => row.GetElementsByClassName("govuk-summary-list__key").Single().TrimmedText(),
                row => row.GetElementsByClassName("govuk-summary-list__value").Single().TrimmedText());

        foreach (var kvp in allSummaryListRowValues)
        {
            var attributeInfo = PersonAttributeInfos.SingleOrDefault(i => i.SummaryListRowKey == kvp.Key);
            if (attributeInfo is null)
            {
                continue;
            }

            static object? FormatValue(object? value) =>
                value is DateOnly dateOnly ? dateOnly.ToString(UiDefaults.DateOnlyDisplayFormat) : value;

            if (sourcedFromRequestDataAttribute.SummaryListRowKey == kvp.Key)
            {
                var requestDataValue = FormatValue(sourcedFromRequestDataAttribute.GetValueFromRequestData(requestData));
                Assert.Equal(requestDataValue, kvp.Value);
            }
            else
            {
                var existingRecordValue = FormatValue(
                    PersonAttributeInfos.Single(i => i.SummaryListRowKey == kvp.Key, kvp.Key).GetValueFromPerson(matchedPerson));
                Assert.Equal(existingRecordValue, kvp.Value);
            }
        }
    }

    [Fact]
    public async Task Get_CreatingNewRecord_DoesNotShowTrnRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueElementForKey("TRN"));
    }

    [Fact]
    public async Task Get_UpdatingExistingRecord_DoesShowTrnRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetSummaryListValueElementForKey("TRN"));
    }

    [Fact]
    public async Task Get_ShowsComments()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = true,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(comments, doc.GetSummaryListValueElementForKey("Comments")?.TrimmedText());
    }

    [Fact]
    public async Task Get_CreatingNewRecord_HasBackAndChangeLinksToMatchesPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var expectedBackLink = $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        Assert.Equal(expectedBackLink, doc.GetElementByTestId("change-link")?.GetAttribute("href"));
    }

    [Fact]
    public async Task Get_UpdatingExistingRecord_HasBackAndChangeLinksToMergePage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var expectedBackLink = $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        Assert.Equal(expectedBackLink, doc.GetElementByTestId("change-link")?.GetAttribute("href"));
    }

    [Fact]
    public async Task Post_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = null });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_CreatingNewRecord_CreatesNewRecordInCrmUpdatesSupportTaskStatusAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = CreateNewRecordPersonIdSentinel,
                Comments = comments
            });

        EventPublisher.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith("/support-tasks/api-trn-requests?waitForJobId=", response.Headers.Location?.OriginalString);

        var expectedCrmRequestId = TrnRequestService.GetCrmTrnRequestId(applicationUser.UserId, requestData.RequestId);
        var crmContact = XrmFakedContext.CreateQuery<Contact>().Single(c => c.dfeta_TrnRequestID == expectedCrmRequestId);
        Assert.NotEqual(matchedPerson.ContactId, crmContact.Id);
        Assert.Equal(requestData.FirstName, crmContact.FirstName);
        Assert.Equal(requestData.MiddleName, crmContact.MiddleName);
        Assert.Equal(requestData.LastName, crmContact.LastName);
        Assert.Equal(requestData.DateOfBirth, crmContact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(requestData.EmailAddress, crmContact.EMailAddress1);
        Assert.Equal(requestData.NationalInsuranceNumber, crmContact.dfeta_NINumber);
        Assert.NotNull(crmContact.dfeta_TRN);

        var updatedSupportTask = await WithDbContext(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(crmContact.Id, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
        Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken);
        var supportTaskData = updatedSupportTask.GetData<ApiTrnRequestData>();
        AssertPersonAttributesMatchContact(supportTaskData.ResolvedAttributes, crmContact);
        Assert.Null(supportTaskData.SelectedPersonAttributes);

        EventPublisher.AssertEventsSaved(@event =>
        {
            var apiTrnRequestSupportTaskUpdatedEvent = Assert.IsType<ApiTrnRequestSupportTaskUpdatedEvent>(@event);
            AssertEventIsExpected(apiTrnRequestSupportTaskUpdatedEvent, expectOldPersonAttributes: false, expectedPersonId: crmContact.Id, comments);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"Records merged successfully for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
    }

    [Fact]
    public async Task Post_UpdatingExistingRecord_UpdatesRecordInCrmUpdatesSupportTaskAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState()
            {
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        var originalContact = XrmFakedContext.CreateQuery<Contact>().Single(c => c.Id == matchedPerson.ContactId);

        EventPublisher.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith("/support-tasks/api-trn-requests?waitForJobId=", response.Headers.Location?.OriginalString);

        var crmContact = XrmFakedContext.CreateQuery<Contact>().Single(c => c.Id == matchedPerson.ContactId);
        Assert.Equal(requestData.MiddleName, crmContact.MiddleName);

        var updatedSupportTask = await WithDbContext(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(crmContact.Id, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
        Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken);
        var supportTaskData = updatedSupportTask.GetData<ApiTrnRequestData>();
        AssertPersonAttributesMatchContact(supportTaskData.ResolvedAttributes, crmContact);
        AssertPersonAttributesMatchContact(supportTaskData.SelectedPersonAttributes, originalContact);

        EventPublisher.AssertEventsSaved(@event =>
        {
            var apiTrnRequestSupportTaskUpdatedEvent = Assert.IsType<ApiTrnRequestSupportTaskUpdatedEvent>(@event);
            AssertEventIsExpected(apiTrnRequestSupportTaskUpdatedEvent, expectOldPersonAttributes: true, expectedPersonId: originalContact.Id, comments);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"Records merged successfully for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
    }

    [Theory]
    [MemberData(nameof(PersonAttributeInfoData))]
    public async Task Post_UpdatingExistingRecord_OnlyUpdatesAttributesSourcedFromRequestData(PersonAttributeInfo attributeSourcedFromRequestData)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            attributeSourcedFromRequestData.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveApiTrnRequestState()
        {
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceToTrnRequest(state, attributeSourcedFromRequestData.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        var matchedCrmContact = XrmFakedContext.CreateQuery<Contact>().Single(c => c.Id == matchedPerson.ContactId).Clone();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var updatedContact = XrmFakedContext.CreateQuery<Contact>().Single(c => c.Id == matchedPerson.ContactId);

        static object? FormatValue(object? value) =>
            value is DateOnly dateOnly ? dateOnly.ToDateTimeWithDqtBstFix(isLocalTime: false) : value;

        var allAttributes = PersonAttributeInfos.SelectMany(i => i.CrmAttributes);
        foreach (var attr in allAttributes)
        {
            if (attributeSourcedFromRequestData.CrmAttributes.Contains(attr))
            {
                Assert.Equal(
                    FormatValue(attributeSourcedFromRequestData.GetValueFromRequestData(requestData)),
                    updatedContact.Attributes[attr]);
            }
            else
            {
                Assert.Equal(matchedCrmContact.Attributes[attr], updatedContact.Attributes[attr]);
            }
        }
    }

    private Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveApiTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveApiTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

    private static void SetPersonAttributeSourceToTrnRequest(ResolveApiTrnRequestState state, PersonMatchedAttribute attribute)
    {
        state.FirstNameSource = attribute is PersonMatchedAttribute.FirstName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.MiddleNameSource = attribute is PersonMatchedAttribute.MiddleName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.LastNameSource = attribute is PersonMatchedAttribute.LastName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
    }

    private void AssertPersonAttributesMatchContact(
        ApiTrnRequestDataPersonAttributes? personAttributes,
        Contact contact)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.FirstName, contact.FirstName);
        Assert.Equal(personAttributes.MiddleName, contact.MiddleName);
        Assert.Equal(personAttributes.LastName, contact.LastName);
        Assert.Equal(personAttributes.DateOfBirth, contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(personAttributes.EmailAddress, contact.EMailAddress1);
        Assert.Equal(personAttributes.NationalInsuranceNumber, contact.dfeta_NINumber);
    }

    private void AssertEventIsExpected(
        ApiTrnRequestSupportTaskUpdatedEvent @event,
        bool expectOldPersonAttributes,
        Guid expectedPersonId,
        string? comments)
    {
        Assert.Equal(expectedPersonId, @event.PersonId);
        Assert.Equal(Clock.UtcNow, @event.CreatedUtc);
        Assert.True(@event.Changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.Status));

        Assert.Equal(SupportTaskStatus.Open, @event.OldSupportTask.Status);
        Assert.Equal(SupportTaskStatus.Closed, @event.SupportTask.Status);

        if (expectOldPersonAttributes)
        {
            Assert.NotNull(@event.OldPersonAttributes);
        }
        else
        {
            Assert.Null(@event.OldPersonAttributes);
        }

        Assert.Equal(comments, @event.Comments);
    }

    public static PersonAttributeInfo[] PersonAttributeInfos { get; } =
    [
        new(
            PersonMatchedAttribute.FirstName,
            "FirstName",
            "First name",
            [Contact.Fields.FirstName],
            d => d.FirstName,
            p => p.FirstName
        ),
        new(
            PersonMatchedAttribute.MiddleName,
            "MiddleName",
            "Middle name",
            [Contact.Fields.MiddleName],
            d => d.MiddleName,
            p => p.MiddleName
        ),
        new(
            PersonMatchedAttribute.LastName,
            "LastName",
            "Last name",
            [Contact.Fields.LastName],
            d => d.LastName,
            p => p.LastName
        ),
        new(
            PersonMatchedAttribute.DateOfBirth,
            "DateOfBirth",
            "Date of birth",
            [Contact.Fields.BirthDate],
            d => d.DateOfBirth,
            p => p.DateOfBirth,
            value => ((DateOnly?)value)?.ToString(UiDefaults.DateOnlyDisplayFormat)
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            "EmailAddress",
            "Email address",
            [Contact.Fields.EMailAddress1],
            d => d.EmailAddress,
            p => p.Email
        ),
        new(
            PersonMatchedAttribute.NationalInsuranceNumber,
            "NationalInsuranceNumber",
            "National Insurance number",
            [Contact.Fields.dfeta_NINumber],
            d => d.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber
        )
    ];

    public static IEnumerable<object[]> PersonAttributeInfoData { get; } = PersonAttributeInfos.Select(i => new object[] { i });

    public record PersonAttributeInfo(
        PersonMatchedAttribute Attribute,
        string FieldName,
        string SummaryListRowKey,
        string[] CrmAttributes,
        Func<TrnRequestMetadata, object?> GetValueFromRequestData,
        Func<TestData.CreatePersonResult, object?> GetValueFromPerson,
        Func<object?, object?>? MapValueToSummaryListRowValue = null);
}
