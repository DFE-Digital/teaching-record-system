using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class MatchesTests(HostFixture hostFixture) : MergePersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_FieldsPopulatedFromPerson()
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Jan 2001"))
            .WithEmailAddress("an@email.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Female));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 2002"))
            .WithEmailAddress("another@email.com")
            .WithNationalInsuranceNumber("AB987654D")
            .WithGender(Gender.Other));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("person-a", "First name", v => Assert.Equal("Alfred", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "Middle name", v => Assert.Equal("The", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "Last name", v => Assert.Equal("Great", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "Date of birth", v => Assert.Equal("1 January 2001", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "Email", v => Assert.Equal("an@email.com", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "National Insurance number", v => Assert.Equal("AB123456D", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "Gender", v => Assert.Equal("Female", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-a", "TRN", v => Assert.Equal(personA.Trn, v.TrimmedText()));

        doc.AssertSummaryListRowValue("person-b", "First name", v => Assert.Equal("Lily", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "Middle name", v => Assert.Equal("The", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "Last name", v => Assert.Equal("Pink", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "Date of birth", v => Assert.Equal("1 February 2002", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "Email", v => Assert.Equal("another@email.com", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "National Insurance number", v => Assert.Equal("AB987654D", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "Gender", v => Assert.Equal("Other", v.TrimmedText()));
        doc.AssertSummaryListRowValue("person-b", "TRN", v => Assert.Equal(personB.Trn, v.TrimmedText()));
    }

    [Fact]
    public async Task Get_PersonHasOpenAlert_ShowsAlertCountAndLinkToAlertsPage()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null))
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("person-a", "Alerts", v =>
        {
            var a = v.QuerySelector("a") as IHtmlAnchorElement;
            Assert.NotNull(a);
            Assert.Equal("(1) open alert (opens in a new tab)", a.TrimmedText());
            Assert.Contains($"/persons/{personA.PersonId}/alerts", a.Href);
        });
        doc.AssertSummaryListRowValue("person-b", "Alerts", v =>
        {
            var a = v.QuerySelector("a") as IHtmlAnchorElement;
            Assert.NotNull(a);
            Assert.Equal("(2) open alerts (opens in a new tab)", a.TrimmedText());
            Assert.Contains($"/persons/{personB.PersonId}/alerts", a.Href);
        });
    }

    public static (PersonMatchedAttribute[] Attributes, bool UseNullValues)[] GetHighlightedDifferencesData() =>
    [
        // We could go nuts creating loads of combinations here, but checking every attribute once seems sufficient
        ([PersonMatchedAttribute.FirstName], false),
        ([PersonMatchedAttribute.MiddleName], false),
        ([PersonMatchedAttribute.LastName], false),
        ([PersonMatchedAttribute.DateOfBirth], false),
        ([PersonMatchedAttribute.EmailAddress], false),
        ([PersonMatchedAttribute.EmailAddress], true),
        ([PersonMatchedAttribute.NationalInsuranceNumber], false),
        ([PersonMatchedAttribute.NationalInsuranceNumber], true),
        ([PersonMatchedAttribute.Gender], false),
        ([PersonMatchedAttribute.Gender], true)
    ];

    [Theory]
    [MemberData(nameof(GetHighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenPersonAAndPersonB(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes, bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithMultipleDifferencesToMatch(matchedAttributes, useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertMatchRowHasExpectedHighlight("person-a", "First name", false);
        doc.AssertMatchRowHasExpectedHighlight("person-a", "Middle name", false);
        doc.AssertMatchRowHasExpectedHighlight("person-a", "Last name", false);
        doc.AssertMatchRowHasExpectedHighlight("person-a", "Date of birth", false);
        doc.AssertMatchRowHasExpectedHighlight("person-a", "Email", false);
        doc.AssertMatchRowHasExpectedHighlight("person-a", "National Insurance number", false);
        doc.AssertMatchRowHasExpectedHighlight("person-a", "Gender", false);

        doc.AssertMatchRowHasExpectedHighlight("person-b", "First name", !matchedAttributes.Contains(PersonMatchedAttribute.FirstName));
        doc.AssertMatchRowHasExpectedHighlight("person-b", "Middle name", !matchedAttributes.Contains(PersonMatchedAttribute.MiddleName));
        doc.AssertMatchRowHasExpectedHighlight("person-b", "Last name", !matchedAttributes.Contains(PersonMatchedAttribute.LastName));
        doc.AssertMatchRowHasExpectedHighlight("person-b", "Date of birth", !matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth));
        doc.AssertMatchRowHasExpectedHighlight("person-b", "Email", !matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress));
        doc.AssertMatchRowHasExpectedHighlight("person-b", "National Insurance number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
        doc.AssertMatchRowHasExpectedHighlight("person-b", "Gender", !matchedAttributes.Contains(PersonMatchedAttribute.Gender));
    }

    [Fact]
    public async Task Get_PersonBIsDeactivated_ShowsWarningAndHidesContinueButton()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

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
            }));

        // Assert
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryPersonOptions = doc.GetElementByTestId("primary-person-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        Assert.NotNull(warningText);
        Assert.Contains("One of these records has been deactivated. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
        Assert.Null(primaryPersonOptions);
        Assert.Null(continueButton);
    }

    [Fact]
    public async Task Get_PersonBHasOpenAlert_ShowsWarningAndHidesContinueButton()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryPersonOptions = doc.GetElementByTestId("primary-person-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        Assert.NotNull(warningText);
        Assert.Contains("One of these records has an alert. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
        Assert.Null(primaryPersonOptions);
        Assert.Null(continueButton);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress, false)]
    [InlineData(InductionStatus.Passed, false)]
    [InlineData(InductionStatus.Failed, false)]
    [InlineData(InductionStatus.None, true)]
    [InlineData(InductionStatus.Exempt, true)]
    [InlineData(InductionStatus.FailedInWales, true)]
    [InlineData(InductionStatus.RequiredToComplete, true)]
    public async Task Get_PersonBWithInductionStatus_ShowsWarningAndHidesContinueButtonAsExpected(InductionStatus status, bool expectMergeToBeAllowed)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync(p => p
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
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryPersonOptions = doc.GetElementByTestId("primary-person-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        if (expectMergeToBeAllowed)
        {
            Assert.Null(warningText);
            Assert.NotNull(primaryPersonOptions);
            Assert.NotNull(continueButton);
        }
        else
        {
            Assert.NotNull(warningText);
            Assert.Contains($"The induction status of one of these records is {status.GetDisplayName()}. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
            Assert.Null(primaryPersonOptions);
            Assert.Null(continueButton);
        }
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Get_PersonBHasOpenAlertAndInvalidInductionStatus_ShowsWarningAndHidesContinueButton(InductionStatus status)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null))
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
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryPersonOptions = doc.GetElementByTestId("primary-person-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        Assert.NotNull(warningText);
        Assert.Contains($"One of these records has an alert and an induction status of {status.GetDisplayName()}. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
        Assert.Null(primaryPersonOptions);
        Assert.Null(continueButton);
    }

    [Fact]
    public async Task Get_PrimaryPersonAlreadySelected_SelectsChosenPerson()
    {
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personB.PersonId;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var primaryPersonChoice = doc.GetChildElementsOfTestId<IHtmlInputElement>("primary-person-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(personB.PersonId.ToString(), primaryPersonChoice);
    }

    [Fact]
    public async Task Post_PrimaryPersonNotSelected_ShowsPageError()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithPrimaryPersonId(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(MatchesModel.PrimaryPersonId), "Select primary record");
    }

    [Fact]
    public async Task Post_PersistsDetailsAndRedirectsToNextPage()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithPrimaryPersonId(personB.PersonId)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        Assert.Equal(personB.PersonId, journeyInstance.State.PrimaryPersonId);
    }

    [Fact]
    public async Task Post_PrimaryPersonChanged_SwapsPrimaryAndSecondarySources_ToKeepSelectedDataCorrect()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.FirstNameSource = PersonAttributeSource.PrimaryPerson;
                s.MiddleNameSource = PersonAttributeSource.SecondaryPerson;
                // Leaving LastNameSource unselected
                s.DateOfBirthSource = PersonAttributeSource.SecondaryPerson;
                s.EmailAddressSource = PersonAttributeSource.SecondaryPerson;
                // Leaving NationalInsuranceNumberSource unselected
                s.GenderSource = PersonAttributeSource.PrimaryPerson;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
                s.Comments = null;
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithPrimaryPersonId(personB.PersonId)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        Assert.Equal(personB.PersonId, journeyInstance.State.PrimaryPersonId);

        Assert.Equal(PersonAttributeSource.SecondaryPerson, journeyInstance.State.FirstNameSource);
        Assert.Equal(PersonAttributeSource.PrimaryPerson, journeyInstance.State.MiddleNameSource);
        Assert.Null(journeyInstance.State.LastNameSource);
        Assert.Equal(PersonAttributeSource.PrimaryPerson, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(PersonAttributeSource.PrimaryPerson, journeyInstance.State.EmailAddressSource);
        Assert.Null(journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(PersonAttributeSource.SecondaryPerson, journeyInstance.State.GenderSource);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, MergePersonJourneyCoordinator? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/matches?{journeyInstance?.GetUniqueIdQueryParameter()}";

    [Theory]
    [InlineData("enter-trn")]
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
    [InlineData("check-answers")]
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToExpected(string? expectedPage)
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

        var checkAnswersUrl = $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{GetRequestPath(personA, journeyInstance)}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}");
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
    [InlineData("Continue", "Cancel and return to record")]
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
    [InlineData("merge")]
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
    [InlineData("check-answers")]
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
