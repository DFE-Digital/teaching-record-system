using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;

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
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState() { PersonId = null });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
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

        var state = new ResolveNpqTrnRequestState()
        {
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceStateToTrnRequest(state, sourcedFromRequestDataAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState()
            {
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = true,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(comments, doc.GetSummaryListValueElementForKey("Comments")?.TrimmedText());
    }

    [Fact]
    public async Task Get_UpdatingExistingRecord_HasBackAndChangeLinksToMergePage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var expectedBackLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState() { PersonId = null });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState()
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_UpdatingExistingRecord_UpdatesRecordUpdatesSupportTaskPublishesEventAndRedirects()
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
            new ResolveNpqTrnRequestState()
            {
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        EventPublisher.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        // redirect
        Assert.Equal("/support-tasks", response.Headers.Location?.OriginalString);

        // person record is updated
        await WithDbContext(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .SingleAsync(p => p.PersonId == matchedPerson.PersonId);
            Assert.Equal(requestData.MiddleName, updatedPersonRecord.MiddleName);
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
            Assert.Equal(matchedPerson.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
            //Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken); // CML TODO what's the TRN token for?
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            AssertPersonAttributesMatch(supportTaskData.SelectedPersonAttributes, matchedPerson.Person);
            AssertPersonAttributesMatch(supportTaskData.ResolvedAttributes, new NpqTrnRequestDataPersonAttributes()
            {
                FirstName = matchedPerson.FirstName,
                MiddleName = requestData.MiddleName!,
                LastName = matchedPerson.LastName,
                DateOfBirth = matchedPerson.DateOfBirth,
                EmailAddress = matchedPerson.Email,
                NationalInsuranceNumber = matchedPerson.NationalInsuranceNumber
            });
        });

        // event is published
        EventPublisher.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<PersonDetailsUpdatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(matchedPerson.PersonId, actualEvent.PersonId);
            Assert.Equal(matchedPerson.FirstName, actualEvent.Details.FirstName);
            Assert.Equal(supportTask.TrnRequestMetadata!.MiddleName, actualEvent.Details.MiddleName);
            Assert.Equal("Updated person from NPQ TRN request", actualEvent.NameChangeReason);
            Assert.Null(actualEvent.DetailsChangeReason);
            Assert.Equal(PersonDetailsUpdatedEventChanges.MiddleName, actualEvent.Changes);
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

        var state = new ResolveNpqTrnRequestState()
        {
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceStateToTrnRequest(state, attributeSourcedFromRequestData.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContext(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .SingleAsync(p => p.PersonId == matchedPerson.PersonId);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.FirstName ? supportTask.TrnRequestMetadata!.FirstName : matchedPerson.FirstName, updatedPersonRecord.FirstName);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.MiddleName ? supportTask.TrnRequestMetadata!.MiddleName : matchedPerson.MiddleName, updatedPersonRecord.MiddleName);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.LastName ? supportTask.TrnRequestMetadata!.LastName : matchedPerson.LastName, updatedPersonRecord.LastName);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.DateOfBirth ? supportTask.TrnRequestMetadata!.DateOfBirth : matchedPerson.DateOfBirth, updatedPersonRecord.DateOfBirth);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.EmailAddress ? supportTask.TrnRequestMetadata!.EmailAddress : matchedPerson.Email, updatedPersonRecord.EmailAddress);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.NationalInsuranceNumber ? supportTask.TrnRequestMetadata!.NationalInsuranceNumber : matchedPerson.NationalInsuranceNumber, updatedPersonRecord.NationalInsuranceNumber);
        });
    }

    [Theory]
    [MemberData(nameof(PersonAttributeInfoData))]
    public async Task Post_UpdatingExistingRecord_UpdatesSupportTask(PersonAttributeInfo attributeSourcedFromRequestData)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            attributeSourcedFromRequestData.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveNpqTrnRequestState()
        {
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceStateToTrnRequest(state, attributeSourcedFromRequestData.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        // support task is updated
        await WithDbContext(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            Assert.Equal(matchedPerson.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
            //Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken); // CML TODO what's the TRN token for?
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            AssertPersonAttributesMatch(supportTaskData.SelectedPersonAttributes, matchedPerson.Person);
            AssertPersonAttributesMatch(supportTaskData.ResolvedAttributes, new NpqTrnRequestDataPersonAttributes()
            {
                FirstName = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.FirstName ? supportTask.TrnRequestMetadata!.FirstName! : matchedPerson.FirstName,
                MiddleName = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.MiddleName ? supportTask.TrnRequestMetadata!.MiddleName! : matchedPerson.MiddleName,
                LastName = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.LastName ? supportTask.TrnRequestMetadata!.LastName! : matchedPerson.LastName,
                DateOfBirth = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.DateOfBirth ? supportTask.TrnRequestMetadata!.DateOfBirth! : matchedPerson.DateOfBirth,
                EmailAddress = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.EmailAddress ? supportTask.TrnRequestMetadata!.EmailAddress! : matchedPerson.Email,
                NationalInsuranceNumber = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.NationalInsuranceNumber ? supportTask.TrnRequestMetadata!.NationalInsuranceNumber! : matchedPerson.NationalInsuranceNumber
            });
        });
    }

    private Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveNpqTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveNpqTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

    private static void SetPersonAttributeSourceStateToTrnRequest(ResolveNpqTrnRequestState state, PersonMatchedAttribute attribute)
    {
        state.FirstNameSource = attribute is PersonMatchedAttribute.FirstName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.MiddleNameSource = attribute is PersonMatchedAttribute.MiddleName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.LastNameSource = attribute is PersonMatchedAttribute.LastName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
    }

    private void AssertPersonAttributesMatch(
        NpqTrnRequestDataPersonAttributes? personAttributes,
        Person person)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.FirstName, person.FirstName);
        Assert.Equal(personAttributes.MiddleName, person.MiddleName);
        Assert.Equal(personAttributes.LastName, person.LastName);
        Assert.Equal(personAttributes.DateOfBirth, person.DateOfBirth);
        Assert.Equal(personAttributes.EmailAddress, person.EmailAddress);
        Assert.Equal(personAttributes.NationalInsuranceNumber, person.NationalInsuranceNumber);
    }

    private void AssertPersonAttributesMatch(
        NpqTrnRequestDataPersonAttributes? personAttributes,
        NpqTrnRequestDataPersonAttributes expectedPersonAttributes)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.FirstName, expectedPersonAttributes.FirstName);
        Assert.Equal(personAttributes.MiddleName, expectedPersonAttributes.MiddleName);
        Assert.Equal(personAttributes.LastName, expectedPersonAttributes.LastName);
        Assert.Equal(personAttributes.DateOfBirth, expectedPersonAttributes.DateOfBirth);
        Assert.Equal(personAttributes.EmailAddress, expectedPersonAttributes.EmailAddress);
        Assert.Equal(personAttributes.NationalInsuranceNumber, expectedPersonAttributes.NationalInsuranceNumber);
    }

    public static PersonAttributeInfo[] PersonAttributeInfos { get; } =
    [
        new(
            PersonMatchedAttribute.FirstName,
            "FirstName",
            "First name",
            d => d.FirstName,
            p => p.FirstName
        ),
        new(
            PersonMatchedAttribute.MiddleName,
            "MiddleName",
            "Middle name",
            d => d.MiddleName,
            p => p.MiddleName
        ),
        new(
            PersonMatchedAttribute.LastName,
            "LastName",
            "Last name",
            d => d.LastName,
            p => p.LastName
        ),
        new(
            PersonMatchedAttribute.DateOfBirth,
            "DateOfBirth",
            "Date of birth",
            d => d.DateOfBirth,
            p => p.DateOfBirth,
            value => ((DateOnly?)value)?.ToString(UiDefaults.DateOnlyDisplayFormat)
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            "EmailAddress",
            "Email address",
            d => d.EmailAddress,
            p => p.Email
        ),
        new(
            PersonMatchedAttribute.NationalInsuranceNumber,
            "NationalInsuranceNumber",
            "National Insurance number",
            d => d.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber
        )
    ];

    public static IEnumerable<object[]> PersonAttributeInfoData { get; } = PersonAttributeInfos.Select(i => new object[] { i });

    public record PersonAttributeInfo(
        PersonMatchedAttribute Attribute,
        string FieldName,
        string SummaryListRowKey,
        Func<TrnRequestMetadata, object?> GetValueFromRequestData,
        Func<TestData.CreatePersonResult, object?> GetValueFromPerson,
        Func<object?, object?>? MapValueToSummaryListRowValue = null);
}
