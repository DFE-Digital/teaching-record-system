using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;
using static TeachingRecordSystem.TestCommon.TestData;

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

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Comments = comments;
            s.Evidence = new()
            {
                UploadEvidence = true,
                UploadedEvidenceFile = new()
                {
                    FileId = evidenceFileId,
                    FileName = evidenceFileName,
                    FileSizeDescription = "5MB"
                }
            };
        });

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

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
            SetPersonAttributeSourceToSecondaryPerson(s, sourcedFromSecondaryPersonAttribute.Attribute);
        });

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
            DateOnly dateOnly => dateOnly.ToString(WebConstants.DateDisplayFormat),
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
                var primaryPersonValue = FormatValue(attributeInfo.GetValueFromPerson(personB));
                Assert.Equal(primaryPersonValue, kvp.Value);
            }
            else
            {
                var secondaryPersonValue = FormatValue(attributeInfo.GetValueFromPerson(personA));
                Assert.Equal(secondaryPersonValue, kvp.Value);
            }
        }
    }

    [Fact]
    public async Task Get_BacklinkLinksToMergePage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
        });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var backlink = doc.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/{personA.PersonId}/merge/merge", backlink.Href);
    }

    [Fact]
    public async Task Get_ChangeLinks_LinkToMergePageWithReturnUrl()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
            SetPersonAttributeSourceToSecondaryPerson(s, PersonMatchedAttribute.FirstName);
        });

        var pageUrl = GetRequestPath(personA, journeyInstance);
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeLink = doc.GetElementByTestId("change-firstname-link") as IHtmlAnchorElement;
        Assert.NotNull(changeLink);
        Assert.Contains($"/persons/{personA.PersonId}/merge/merge", changeLink.Href);
        Assert.Contains($"returnUrl={Uri.EscapeDataString(pageUrl)}", changeLink.Href);
    }

    [Fact]
    public async Task Get_ConfirmAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
        });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Confirm and update primary record", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_PersonIsDeactivated_ReturnsBadRequest(bool deactivatePersonA)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContextAsync(async dbContext =>
        {
            var person = deactivatePersonA ? personA : personB;
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
        });

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_PersonHasOpenAlert_ReturnsBadRequest(bool alertOnPersonA)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(
            configurePersonA: p => { if (alertOnPersonA) { p.WithAlert(a => a.WithEndDate(null)); } },
            configurePersonB: p => { if (!alertOnPersonA) { p.WithAlert(a => a.WithEndDate(null)); } });

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
        });

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    public static TheoryData<bool, InductionStatus> Post_PersonHasInvalidInductionStatus_ReturnsBadRequestData =>
        new MatrixTheoryData<bool, InductionStatus>(
            [true, false],
            [InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed]);

    [Theory]
    [MemberData(nameof(Post_PersonHasInvalidInductionStatus_ReturnsBadRequestData))]
    public async Task Post_PersonHasInvalidInductionStatus_ReturnsBadRequest(bool statusOnPersonA, InductionStatus status)
    {
        // Arrange
        void ConfigureInductionStatus(CreatePersonBuilder p) => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1)));

        var (personA, personB) = await CreatePersonsWithNoDifferences(
            configurePersonA: p => { if (statusOnPersonA) { ConfigureInductionStatus(p); } },
            configurePersonB: p => { if (!statusOnPersonA) { ConfigureInductionStatus(p); } });

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
        });

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new() { UploadEvidence = false };
        });

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response, $"/persons/{personA.PersonId}");

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Evidence = new()
            {
                UploadEvidence = true,
                UploadedEvidenceFile = new()
                {
                    FileId = evidenceFileId,
                    FileName = "evidence.jpg",
                    FileSizeDescription = "5MB"
                }
            };
        });

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        await HttpClient.SendAsync(request);

        // Assert
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [MemberData(nameof(GetPersonAttributeInfoData))]
    public async Task Post_UpdatesPrimaryPersonPublishesEventDeactivatesSecondaryPersonDeletesJourneyAndRedirects(
        PersonAttributeInfo sourcedFromSecondaryPersonAttribute,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(
            sourcedFromSecondaryPersonAttribute.Attribute,
            useNullValues: useNullValues);

        TimeProvider.Advance(TimeSpan.FromDays(1));

        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var comments = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(personA, personB, s =>
        {
            s.PrimaryPersonId = personA.PersonId;
            s.Comments = comments;
            s.Evidence = new()
            {
                UploadEvidence = true,
                UploadedEvidenceFile = new()
                {
                    FileId = evidenceFileId,
                    FileName = evidenceFileName,
                    FileSizeDescription = "5MB"
                }
            };
            SetPersonAttributeSourceToSecondaryPerson(s, sourcedFromSecondaryPersonAttribute.Attribute);
        });

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
                DateOnly dateOnly => dateOnly.ToString(WebConstants.DateDisplayFormat),
                Gender gender => gender.GetDisplayName(),
                _ => value
            };

        foreach (var attr in PersonAttributeInfos)
        {
            if (attr.Attribute == sourcedFromSecondaryPersonAttribute.Attribute)
            {
                Assert.Equal(FormatValue(attr.GetValueFromPerson(personB)), FormatValue(attr.GetValueFromPerson(primaryPerson)));
            }
            else
            {
                Assert.Equal(FormatValue(attr.GetValueFromPerson(personA)), FormatValue(attr.GetValueFromPerson(primaryPerson)));
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
                Assert.Equal(FormatValue(attr.GetValueFromPerson(personA)), FormatValue(attr.GetValueFromPersonAttributes(actualEvent.OldPersonAttributes)));

                if (attr.Attribute == sourcedFromSecondaryPersonAttribute.Attribute)
                {
                    Assert.Equal(FormatValue(attr.GetValueFromPerson(personB)), FormatValue(attr.GetValueFromPersonAttributes(actualEvent.PersonAttributes)));
                }
                else
                {
                    Assert.Equal(FormatValue(attr.GetValueFromPerson(personA)), FormatValue(attr.GetValueFromPersonAttributes(actualEvent.PersonAttributes)));
                }
            }

            Assert.Equal(evidenceFileId, actualEvent.EvidenceFile?.FileId);
            Assert.Equal(evidenceFileName, actualEvent.EvidenceFile?.Name);
            Assert.Equal(comments, actualEvent.Comments);
            Assert.Equal(TimeProvider.UtcNow, actualEvent.CreatedUtc);

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
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            $"Records merged for {primaryPerson.FirstName} {primaryPerson.MiddleName} {primaryPerson.LastName}");

        Assert.Null(GetJourneyInstanceState(journeyInstance));
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
            p => p.FirstName
        ),
        new(
            PersonMatchedAttribute.MiddleName,
            "MiddleName",
            "Middle name",
            p => p.MiddleName,
            p => p.MiddleName
        ),
        new(
            PersonMatchedAttribute.LastName,
            "LastName",
            "Last name",
            p => p.LastName,
            p => p.LastName
        ),
        new(
            PersonMatchedAttribute.DateOfBirth,
            "DateOfBirth",
            "Date of birth",
            p => p.DateOfBirth,
            p => p.DateOfBirth,
            value => ((DateOnly?)value)?.ToString(WebConstants.DateDisplayFormat)
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            "EmailAddress",
            "Email address",
            p => p.EmailAddress,
            p => p.EmailAddress
        ),
        new(
            PersonMatchedAttribute.NationalInsuranceNumber,
            "NationalInsuranceNumber",
            "National Insurance number",
            p => p.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber
        ),
        new(
            PersonMatchedAttribute.Gender,
            "Gender",
            "Gender",
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
        Func<Person, object?> GetValueFromPerson,
        Func<PersonDetails, object?> GetValueFromPersonAttributes,
        Func<object?, object?>? MapValueToSummaryListRowValue = null);

    private string GetRequestPath(Person person, MergePersonJourneyCoordinator journeyInstance) =>
        $"/persons/{person.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
}
