using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class MatchesTests(HostFixture hostFixture) : MergePersonTestBase(hostFixture)
{
    [Test]
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
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
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
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
    [MethodDataSource(nameof(GetHighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenPersonAAndPersonB(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes, bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithMultipleDifferencesToMatch(matchedAttributes, useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
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
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
    public async Task Get_PersonBHasOpenAlert_ShowsWarningAndHidesContinueButton()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
    [Arguments(InductionStatus.InProgress, false)]
    [Arguments(InductionStatus.Passed, false)]
    [Arguments(InductionStatus.Failed, false)]
    [Arguments(InductionStatus.None, true)]
    [Arguments(InductionStatus.Exempt, true)]
    [Arguments(InductionStatus.FailedInWales, true)]
    [Arguments(InductionStatus.RequiredToComplete, true)]
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
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
    [Arguments(InductionStatus.InProgress)]
    [Arguments(InductionStatus.Passed)]
    [Arguments(InductionStatus.Failed)]
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
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
    public async Task Get_PrimaryPersonAlreadySelected_SelectsChosenPerson()
    {
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var primaryPersonChoice = doc.GetChildElementsOfTestId<IHtmlInputElement>("primary-person-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(personB.PersonId.ToString(), primaryPersonChoice);
    }

    [Test]
    public async Task Post_PrimaryPersonNotSelected_ShowsPageError()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

    [Test]
    public async Task Post_PersistsDetailsAndRedirectsToNextPage()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

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

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personB.PersonId, journeyInstance.State.PrimaryPersonId);
    }

    [Test]
    public async Task Post_PrimaryPersonChanged_SwapsPrimaryAndSecondarySources_ToKeepSelectedDataCorrect()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithFirstNameSource(PersonAttributeSource.PrimaryPerson)
                .WithMiddleNameSource(PersonAttributeSource.SecondaryPerson)
                // Leaving LastNameSource unselected
                .WithDateOfBirthSource(PersonAttributeSource.SecondaryPerson)
                .WithEmailAddressSource(PersonAttributeSource.SecondaryPerson)
                // Leaving NationalInsuranceNumberSource unselected
                .WithGenderSource(PersonAttributeSource.PrimaryPerson)
                .WithUploadEvidenceChoice(false)
                .WithComments(null)
                .Build());

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

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personB.PersonId, journeyInstance.State.PrimaryPersonId);

        Assert.Equal(PersonAttributeSource.SecondaryPerson, journeyInstance.State.FirstNameSource);
        Assert.Equal(PersonAttributeSource.PrimaryPerson, journeyInstance.State.MiddleNameSource);
        Assert.Null(journeyInstance.State.LastNameSource);
        Assert.Equal(PersonAttributeSource.PrimaryPerson, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(PersonAttributeSource.PrimaryPerson, journeyInstance.State.EmailAddressSource);
        Assert.Null(journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(PersonAttributeSource.SecondaryPerson, journeyInstance.State.GenderSource);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<MergePersonState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/matches?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergePersonState>> CreateJourneyInstanceAsync(Guid personId, MergePersonState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergePersonState(),
            new KeyValuePair<string, object>("personId", personId));
}
