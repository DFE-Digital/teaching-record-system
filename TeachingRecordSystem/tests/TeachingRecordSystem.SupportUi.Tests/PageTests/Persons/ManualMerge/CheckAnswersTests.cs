using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.ManualMerge;

[Collection(nameof(DisableParallelization))]
public class CheckAnswersTests : ManualMergeTestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Fact]
    public async Task Get_RendersNonAttributeValues()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var comments = Faker.Lorem.Paragraph();

        var state = new MergeStateBuilder()
            .WithInitializedState(personA)
            .WithPersonB(personB)
            .WithPrimaryRecord(personA)
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

        doc.AssertRow("TRN", v => Assert.Equal(personA.Trn, v.TrimmedText()));
        doc.AssertRow("Evidence", v =>
        {
            var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal($"{evidenceFileName} (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });
        doc.AssertRow("Comments", v => Assert.Equal(comments, v.TrimmedText()));
    }

    [Theory]
    [MemberData(nameof(PersonAttributeInfoData))]
    public async Task Get_AttributeSourceIsSecondaryRecord_RendersChosenAttributeValues(PersonAttributeInfo sourcedFromSecondaryRecordAttribute, bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(
            sourcedFromSecondaryRecordAttribute.Attribute,
            useNullValues: useNullValues);

        var state = new MergeStateBuilder()
            .WithInitializedState(personA)
            .WithPersonB(personB)
            .WithPrimaryRecord(personA)
            .WithAttributeSourcesSet()
            .Build();
        SetPersonAttributeSourceToSecondaryRecord(state, sourcedFromSecondaryRecordAttribute.Attribute);

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
            null => UiDefaults.EmptyDisplayContent,
            DateOnly dateOnly => dateOnly.ToString(UiDefaults.DateOnlyDisplayFormat),
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

            if (sourcedFromSecondaryRecordAttribute.SummaryListRowKey == kvp.Key)
            {
                var primaryRecordValue = FormatValue(attributeInfo.GetValueFromPerson(personB));
                Assert.Equal(primaryRecordValue, kvp.Value);
            }
            else
            {
                var secondaryRecordValue = FormatValue(attributeInfo.GetValueFromPerson(personA));
                Assert.Equal(secondaryRecordValue, kvp.Value);
            }
        }
    }

    [Theory]
    [MemberData(nameof(PersonAttributeInfoData))]
    public async Task Post_UpdatesPrimaryRecordPublishesEventDeactivatesSecondaryRecordCompletesJourneyAndRedirects(PersonAttributeInfo sourcedFromSecondaryRecordAttribute, bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(
            sourcedFromSecondaryRecordAttribute.Attribute,
            useNullValues: useNullValues);

        Clock.Advance();

        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var comments = Faker.Lorem.Paragraph();

        var state = new MergeStateBuilder()
            .WithInitializedState(personA)
            .WithPersonB(personB)
            .WithPrimaryRecord(personA)
            .WithAttributeSourcesSet()
            .WithComments(comments)
            .WithUploadEvidenceChoice(true, evidenceFileId, evidenceFileName)
            .Build();
        SetPersonAttributeSourceToSecondaryRecord(state, sourcedFromSecondaryRecordAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstanceAsync(personA.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance));

        EventPublisher.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}");

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons
            .IgnoreQueryFilters()
            .SingleAsync(p => p.PersonId == personA.PersonId));
        Assert.Equal(PersonStatus.Active, updatedPerson.Status);

        var secondaryRecord = await WithDbContext(dbContext => dbContext.Persons
            .IgnoreQueryFilters()
            .SingleAsync(p => p.PersonId == personB.PersonId));
        Assert.Equal(PersonStatus.Deactivated, secondaryRecord.Status);

        static object? FormatValue(object? value) =>
            value switch
            {
                null => UiDefaults.EmptyDisplayContent,
                DateOnly dateOnly => dateOnly.ToString(UiDefaults.DateOnlyDisplayFormat),
                Gender gender => gender.GetDisplayName(),
                _ => value
            };

        foreach (var attr in PersonAttributeInfos)
        {
            if (attr.Attribute == sourcedFromSecondaryRecordAttribute.Attribute)
            {
                var x = attr.GetValueFromPerson(personB);
                var fx = FormatValue(x);
                var y = attr.GetValueFromPersonRecord(updatedPerson);
                var fy = FormatValue(y);
                Assert.Equal(fx, fy);
            }
            else
            {
                var x = attr.GetValueFromPerson(personA);
                var fx = FormatValue(x);
                var y = attr.GetValueFromPersonRecord(updatedPerson);
                var fy = FormatValue(y);
                Assert.Equal(fx, fy);

            }
        }

        // event is published
        EventPublisher.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<PersonsMergedEvent>(e);
            Assert.Equal(personA.PersonId, actualEvent.PersonId);

            foreach (var attr in PersonAttributeInfos)
            {
                var x = attr.GetValueFromPerson(personA);
                var fx = FormatValue(x);
                var y = attr.GetValueFromPersonAttributes(actualEvent.OldPersonAttributes);
                var fy = FormatValue(y);
                Assert.Equal(fx, fy);

                if (attr.Attribute == sourcedFromSecondaryRecordAttribute.Attribute)
                {
                    var a = attr.GetValueFromPerson(personB);
                    var fa = FormatValue(a);
                    var b = attr.GetValueFromPersonAttributes(actualEvent.PersonAttributes);
                    var fb = FormatValue(b);
                    Assert.Equal(fa, fb);
                }
                else
                {
                    var a = attr.GetValueFromPerson(personA);
                    var fa = FormatValue(a);
                    var b = attr.GetValueFromPersonAttributes(actualEvent.PersonAttributes);
                    var fb = FormatValue(b);
                    Assert.Equal(fa, fb);
                }
            }

            Assert.Equal(evidenceFileId, actualEvent.EvidenceFile?.FileId);
            Assert.Equal(evidenceFileName, actualEvent.EvidenceFile?.Name);
            Assert.Equal(comments, actualEvent.Comments);
            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"Records merged successfully for {updatedPerson.FirstName} {updatedPerson.MiddleName} {updatedPerson.LastName}");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private static void SetPersonAttributeSourceToSecondaryRecord(ManualMergeState state, PersonMatchedAttribute attribute)
    {
        state.FirstNameSource = attribute is PersonMatchedAttribute.FirstName ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
        state.MiddleNameSource = attribute is PersonMatchedAttribute.MiddleName ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
        state.LastNameSource = attribute is PersonMatchedAttribute.LastName ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
        state.GenderSource = attribute is PersonMatchedAttribute.Gender ? PersonAttributeSource.SecondaryRecord : PersonAttributeSource.PrimaryRecord;
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
            value => ((DateOnly?)value)?.ToString(UiDefaults.DateOnlyDisplayFormat)
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            "EmailAddress",
            "Email address",
            p => p.Email,
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

    public static IEnumerable<object[]> PersonAttributeInfoData { get; } = PersonAttributeInfos.SelectMany(i => new object[][] { new object[] { i, false }, new object[] { i, true } });

    public record PersonAttributeInfo(
        PersonMatchedAttribute Attribute,
        string FieldName,
        string SummaryListRowKey,
        Func<TestData.CreatePersonResult, object?> GetValueFromPerson,
        Func<Person, object?> GetValueFromPersonRecord,
        Func<PersonAttributes, object?> GetValueFromPersonAttributes,
        Func<object?, object?>? MapValueToSummaryListRowValue = null);

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<ManualMergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/manual-merge/check-answers?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<ManualMergeState>> CreateJourneyInstanceAsync(Guid personId, ManualMergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.ManualMergePerson,
            state ?? new ManualMergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
