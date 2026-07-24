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

        var state = CreateState(personA, s =>
                    {
                        s.PersonBId = personB.PersonId;
                        s.PersonBTrn = personB.Trn;
                        s.PrimaryPersonId = personA.PersonId;
                        s.PersonAttributeSourcesSet = true;
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

        var state = CreateState(personA, s =>
                    {
                        s.PersonBId = personB.PersonId;
                        s.PersonBTrn = personB.Trn;
                        s.PrimaryPersonId = personA.PersonId;
                        s.PersonAttributeSourcesSet = true;
                        s.Evidence = new()
                        {
                            UploadEvidence = false
                        };
                    });
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

        TimeProvider.Advance(TimeSpan.FromDays(1));

        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var comments = Faker.Lorem.Paragraph();

        var state = CreateState(personA, s =>
                    {
                        s.PersonBId = personB.PersonId;
                        s.PersonBTrn = personB.Trn;
                        s.PrimaryPersonId = personA.PersonId;
                        s.PersonAttributeSourcesSet = true;
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
                DateOnly dateOnly => dateOnly.ToString(WebConstants.DateDisplayFormat),
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
            value => ((DateOnly?)value)?.ToString(WebConstants.DateDisplayFormat)
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

    private string GetRequestPath(TestData.CreatePersonResult person, MergePersonJourneyCoordinator? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/check-answers?{journeyInstance?.GetUniqueIdQueryParameter()}";

    [Theory]
    [InlineData("merge")]
    public async Task Get_BacklinkLinksToExpected(string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [InlineData("Confirm and update primary record", "Cancel and return to record")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string continueButtonText, string cancelButtonText)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal(continueButtonText, b.TrimmedText()),
            b => Assert.Equal(cancelButtonText, b.TrimmedText()));
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var pageUrl = GetRequestPath(personA, journeyInstance);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        AssertEx.ResponseIsRedirectTo(redirectResponse, $"/persons/{personA.PersonId}");

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_PersonAIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personA.Person);
            personA.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonAHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Post_PersonAHasInvalidInductionStatus_ReturnsBadRequest(InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonBIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personB.Person);
            personB.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonBHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Post_PersonBHasInvalidInductionStatus_ReturnsBadRequest(InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    public async Task Post_RedirectsToExpected(string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    [Theory]
    [InlineData(null)]
    public async Task Post_WithReturnUrlToCheckAnswersPage_RedirectsToCheckAnswersPage(string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var checkAnswersUrl = $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(personA, journeyInstance)}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}")
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }
}
