using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class CheckAnswersTests(HostFixture hostFixture) : MergePersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_RendersNonAttributeValues()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var comments = Faker.Lorem.Paragraph();

        var state = new MergePersonStateBuilder()
            .WithInitializedState(personA)
            .WithPersonB(personB)
            .WithPrimaryPerson(personA)
            .WithAttributeSourcesSet()
            .WithComments(comments)
            .WithUploadEvidenceChoice(true, evidenceFileId, evidenceFileName)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(personA.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("TRN", v => Assert.Equal(personA.Trn, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Evidence", v =>
        {
            var urlEncoder = UrlEncoder.Default;
            var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
            var expectedFileUrl = $"http://localhost/files/evidence.jpg?fileUrl={expectedBlobStorageFileUrl}";
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal($"{evidenceFileName} (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });
        doc.AssertSummaryListRowValue("Comments", v => Assert.Equal(comments, v.TrimmedText()));
    }

    [Theory]
    [MemberData(nameof(GetPersonAttributeInfoData))]
    public async Task Get_AttributeSourceIsSecondaryPerson_RendersChosenAttributeValues(
        PersonAttributeInfo sourcedFromSecondaryPersonAttribute,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(
            sourcedFromSecondaryPersonAttribute.Attribute,
            useNullValues: useNullValues);

        var state = new MergePersonStateBuilder()
            .WithInitializedState(personA)
            .WithPersonB(personB)
            .WithPrimaryPerson(personA)
            .WithAttributeSourcesSet()
            .WithUploadEvidenceChoice(false)
            .Build();
        SetPersonAttributeSourceToSecondaryPerson(state, sourcedFromSecondaryPersonAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstanceAsync(personA.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var allSummaryListRowValues = doc.GetElementsByClassName("govuk-summary-list__row")
            .ToDictionary(
                row => row.GetElementsByClassName("govuk-summary-list__key").Single().TrimmedText(),
                row => row.GetElementsByClassName("govuk-summary-list__value").Single().TrimmedText());

        static object? FormatValue(object? value) => value switch
        {
            null => WebConstants.EmptyFallbackContent,
            DateOnly dateOnly => dateOnly.ToString(WebConstants.DateOnlyDisplayFormat),
            Gender gender => gender.GetDisplayName(),
            _ => value
        };

        foreach (var kvp in allSummaryListRowValues)
        {
            var attributeInfo = PersonAttributeInfos.SingleOrDefault(i => i.SummaryListRowKey == kvp.Key);
            if (attributeInfo is null)
            {
                continue;
            }

            if (sourcedFromSecondaryPersonAttribute.SummaryListRowKey == kvp.Key)
            {
                var primaryPersonValue = FormatValue(attributeInfo.GetValueFromPersonResult(personB));
                Assert.Equal(primaryPersonValue, kvp.Value);
            }
            else
            {
                var secondaryPersonValue = FormatValue(attributeInfo.GetValueFromPersonResult(personA));
                Assert.Equal(secondaryPersonValue, kvp.Value);
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetPersonAttributeInfoData))]
    public async Task Post_UpdatesPrimaryPersonPublishesEventDeactivatesSecondaryPersonCompletesJourneyAndRedirects(
        PersonAttributeInfo sourcedFromSecondaryPersonAttribute,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(
            sourcedFromSecondaryPersonAttribute.Attribute,
            useNullValues: useNullValues);

        Clock.Advance();

        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var comments = Faker.Lorem.Paragraph();

        var state = new MergePersonStateBuilder()
            .WithInitializedState(personA)
            .WithPersonB(personB)
            .WithPrimaryPerson(personA)
            .WithAttributeSourcesSet()
            .WithComments(comments)
            .WithUploadEvidenceChoice(true, evidenceFileId, evidenceFileName)
            .Build();
        SetPersonAttributeSourceToSecondaryPerson(state, sourcedFromSecondaryPersonAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstanceAsync(personA.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance));

        EventObserver.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}");

        var primaryPerson = await WithDbContextAsync(dbContext => dbContext.Persons
            .IgnoreQueryFilters()
            .Include(p => p.MergedWithPerson)
            .SingleAsync(p => p.PersonId == personA.PersonId));
        Assert.Equal(PersonStatus.Active, primaryPerson.Status);
        Assert.Null(primaryPerson.MergedWithPersonId);

        var secondaryPerson = await WithDbContextAsync(dbContext => dbContext.Persons
            .IgnoreQueryFilters()
            .Include(p => p.MergedWithPerson)
            .SingleAsync(p => p.PersonId == personB.PersonId));
        Assert.Equal(PersonStatus.Deactivated, secondaryPerson.Status);
        Assert.Equal(primaryPerson.PersonId, secondaryPerson.MergedWithPersonId);

        static object? FormatValue(object? value) =>
            value switch
            {
                null => WebConstants.EmptyFallbackContent,
                DateOnly dateOnly => dateOnly.ToString(WebConstants.DateOnlyDisplayFormat),
                Gender gender => gender.GetDisplayName(),
                _ => value
            };

        foreach (var attr in PersonAttributeInfos)
        {
            if (attr.Attribute == sourcedFromSecondaryPersonAttribute.Attribute)
            {
                Assert.Equal(FormatValue(attr.GetValueFromPersonResult(personB)), FormatValue(attr.GetValueFromPerson(primaryPerson)));
            }
            else
            {
                Assert.Equal(FormatValue(attr.GetValueFromPersonResult(personA)), FormatValue(attr.GetValueFromPerson(primaryPerson)));
            }
        }

        // event is published
        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<LegacyEvents.PersonsMergedEvent>(e);
            Assert.Equal(personA.PersonId, actualEvent.PersonId);
            Assert.Equal(personA.Trn, actualEvent.PersonTrn);
            Assert.Equal(personB.PersonId, actualEvent.SecondaryPersonId);
            Assert.Equal(personB.Trn, actualEvent.SecondaryPersonTrn);
            Assert.Equal(PersonStatus.Deactivated, actualEvent.SecondaryPersonStatus);

            foreach (var attr in PersonAttributeInfos)
            {
                Assert.Equal(FormatValue(attr.GetValueFromPersonResult(personA)), FormatValue(attr.GetValueFromPersonAttributes(actualEvent.OldPersonAttributes)));

                if (attr.Attribute == sourcedFromSecondaryPersonAttribute.Attribute)
                {
                    Assert.Equal(FormatValue(attr.GetValueFromPersonResult(personB)), FormatValue(attr.GetValueFromPersonAttributes(actualEvent.PersonAttributes)));
                }
                else
                {
                    Assert.Equal(FormatValue(attr.GetValueFromPersonResult(personA)), FormatValue(attr.GetValueFromPersonAttributes(actualEvent.PersonAttributes)));
                }
            }

            Assert.Equal(evidenceFileId, actualEvent.EvidenceFile?.FileId);
            Assert.Equal(evidenceFileName, actualEvent.EvidenceFile?.Name);
            Assert.Equal(comments, actualEvent.Comments);
            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);

            var expectedChange = sourcedFromSecondaryPersonAttribute.Attribute switch
            {
                PersonMatchedAttribute.FirstName => LegacyEvents.PersonsMergedEventChanges.FirstName,
                PersonMatchedAttribute.MiddleName => LegacyEvents.PersonsMergedEventChanges.MiddleName,
                PersonMatchedAttribute.LastName => LegacyEvents.PersonsMergedEventChanges.LastName,
                PersonMatchedAttribute.DateOfBirth => LegacyEvents.PersonsMergedEventChanges.DateOfBirth,
                PersonMatchedAttribute.EmailAddress => LegacyEvents.PersonsMergedEventChanges.EmailAddress,
                PersonMatchedAttribute.NationalInsuranceNumber => LegacyEvents.PersonsMergedEventChanges.NationalInsuranceNumber,
                PersonMatchedAttribute.Gender => LegacyEvents.PersonsMergedEventChanges.Gender,
                PersonMatchedAttribute.FullName => throw new NotImplementedException(),
                PersonMatchedAttribute.Trn => throw new NotImplementedException(),
                _ => LegacyEvents.PersonsMergedEventChanges.None
            };
            Assert.Equal(expectedChange, actualEvent.Changes);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.PersonMerging, p.ProcessContext.ProcessType);

            var changeReasonInfo = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Null(changeReasonInfo.Reason);
            Assert.Equal(comments, changeReasonInfo.Details);
            Assert.Equal(evidenceFileId, changeReasonInfo.EvidenceFile?.FileId);
            Assert.Equal("evidence.jpg", changeReasonInfo.EvidenceFile?.Name);

            p.AssertProcessHasEvents<PersonDeactivatedEvent, PersonDetailsUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"Records merged for {primaryPerson.FirstName} {primaryPerson.MiddleName} {primaryPerson.LastName}");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private static void SetPersonAttributeSourceToSecondaryPerson(MergePersonState state, PersonMatchedAttribute attribute)
    {
        state.FirstNameSource = attribute is PersonMatchedAttribute.FirstName ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
        state.MiddleNameSource = attribute is PersonMatchedAttribute.MiddleName ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
        state.LastNameSource = attribute is PersonMatchedAttribute.LastName ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
        state.GenderSource = attribute is PersonMatchedAttribute.Gender ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
    }

    public static PersonAttributeInfo[] PersonAttributeInfos { get; } =
    [
        new(
            PersonMatchedAttribute.FirstName,
            "FirstName",
            "First name",
            p => p.FirstName,
            p => p.FirstName,
            p => p.FirstName
        ),
        new(
            PersonMatchedAttribute.MiddleName,
            "MiddleName",
            "Middle name",
            p => p.MiddleName,
            p => p.MiddleName,
            p => p.MiddleName
        ),
        new(
            PersonMatchedAttribute.LastName,
            "LastName",
            "Last name",
            p => p.LastName,
            p => p.LastName,
            p => p.LastName
        ),
        new(
            PersonMatchedAttribute.DateOfBirth,
            "DateOfBirth",
            "Date of birth",
            p => p.DateOfBirth,
            p => p.DateOfBirth,
            p => p.DateOfBirth,
            value => ((DateOnly?)value)?.ToString(WebConstants.DateOnlyDisplayFormat)
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            "EmailAddress",
            "Email address",
            p => p.EmailAddress,
            p => p.EmailAddress,
            p => p.EmailAddress
        ),
        new(
            PersonMatchedAttribute.NationalInsuranceNumber,
            "NationalInsuranceNumber",
            "National Insurance number",
            p => p.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber
        ),
        new(
            PersonMatchedAttribute.Gender,
            "Gender",
            "Gender",
            p => p.Gender,
            p => p.Gender,
            p => p.Gender
        )
    ];

    public static (PersonAttributeInfo Attribute, bool UseNullValues)[] GetPersonAttributeInfoData() =>
        PersonAttributeInfos.SelectMany(i => new[] { (i, false), (i, true) }).ToArray();

    public record PersonAttributeInfo(
        PersonMatchedAttribute Attribute,
        string FieldName,
        string SummaryListRowKey,
        Func<TestData.CreatePersonResult, object?> GetValueFromPersonResult,
        Func<Person, object?> GetValueFromPerson,
        Func<PersonDetails, object?> GetValueFromPersonAttributes,
        Func<object?, object?>? MapValueToSummaryListRowValue = null);

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<MergePersonState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/check-answers?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergePersonState>> CreateJourneyInstanceAsync(Guid personId, MergePersonState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergePersonState(),
            new KeyValuePair<string, object>("personId", personId));
}
