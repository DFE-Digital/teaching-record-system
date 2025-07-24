using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

[Collection(nameof(DisableParallelization))]
public class CompareMatchingRecordsTests : TestBase
{
    public CompareMatchingRecordsTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Fact]
    public async Task Get_OtherTrnNotSelected_RedirectsToEnterTrnPage()
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/enter-trn?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Fact]
    public async Task Get_BacklinkLinksToPersonDetails()
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var backlink = doc.GetElementByTestId("back-link") as IHtmlAnchorElement;

        Assert.NotNull(backlink);
        Assert.Contains($"/persons/{personA.PersonId}/merge/enter-trn", backlink.Href);
    }

    [Fact]
    public async Task Get_FieldsPopulatedFromPersonRecord()
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Jan 2001"))
            .WithEmail("an@email.com")
            .WithNationalInsuranceNumber("AB123456D"));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 2002"))
            .WithEmail("another@email.com")
            .WithNationalInsuranceNumber("AB987654D"));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRow("record-a", "First name", v => Assert.Equal("Alfred", v.TrimmedText()));
        doc.AssertRow("record-a", "Middle name", v => Assert.Equal("The", v.TrimmedText()));
        doc.AssertRow("record-a", "Last name", v => Assert.Equal("Great", v.TrimmedText()));
        doc.AssertRow("record-a", "Date of birth", v => Assert.Equal("1 January 2001", v.TrimmedText()));
        doc.AssertRow("record-a", "Email", v => Assert.Equal("an@email.com", v.TrimmedText()));
        doc.AssertRow("record-a", "National Insurance number", v => Assert.Equal("AB123456D", v.TrimmedText()));
        doc.AssertRow("record-a", "TRN", v => Assert.Equal(personA.Trn, v.TrimmedText()));

        doc.AssertRow("record-b", "First name", v => Assert.Equal("Lily", v.TrimmedText()));
        doc.AssertRow("record-b", "Middle name", v => Assert.Equal("The", v.TrimmedText()));
        doc.AssertRow("record-b", "Last name", v => Assert.Equal("Pink", v.TrimmedText()));
        doc.AssertRow("record-b", "Date of birth", v => Assert.Equal("1 February 2002", v.TrimmedText()));
        doc.AssertRow("record-b", "Email", v => Assert.Equal("another@email.com", v.TrimmedText()));
        doc.AssertRow("record-b", "National Insurance number", v => Assert.Equal("AB987654D", v.TrimmedText()));
        doc.AssertRow("record-b", "TRN", v => Assert.Equal(personB.Trn, v.TrimmedText()));
    }

    [Fact]
    public async Task Get_PersonHasOpenAlert_ShowsAlertCountAndLinkToAlertsPage()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null)));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null))
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRow("record-a", "Alerts", v =>
        {
            var a = v.QuerySelector("a") as IHtmlAnchorElement;
            Assert.NotNull(a);
            Assert.Equal("(1) open alert (opens in a new tab)", a.TrimmedText());
            Assert.Contains($"/persons/{personA.PersonId}/alerts", a.Href);
        });
        doc.AssertRow("record-b", "Alerts", v =>
        {
            var a = v.QuerySelector("a") as IHtmlAnchorElement;
            Assert.NotNull(a);
            Assert.Equal("(2) open alerts (opens in a new tab)", a.TrimmedText());
            Assert.Contains($"/persons/{personB.PersonId}/alerts", a.Href);
        });
    }

    public static TheoryData<PersonMatchedAttribute[]> HighlightedDifferencesData { get; } = new()
    {
        // We could go nuts creating loads of combinations here, but checking every attribute once seems sufficient
        new[] { PersonMatchedAttribute.FirstName },
        new[] { PersonMatchedAttribute.MiddleName },
        new[] { PersonMatchedAttribute.LastName },
        new[] { PersonMatchedAttribute.DateOfBirth },
        new[] { PersonMatchedAttribute.EmailAddress },
        new[] { PersonMatchedAttribute.NationalInsuranceNumber }
    };

    [Theory]
    [MemberData(nameof(HighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenPersonAAndPersonB(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithEmail()
            .WithNationalInsuranceNumber());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithFirstName(
                matchedAttributes.Contains(PersonMatchedAttribute.FirstName)
                    ? personA.FirstName
                    : TestData.GenerateChangedFirstName(personA.FirstName))
            .WithMiddleName(
                matchedAttributes.Contains(PersonMatchedAttribute.MiddleName)
                    ? personA.MiddleName
                    : TestData.GenerateChangedMiddleName(personA.MiddleName))
            .WithLastName(
                matchedAttributes.Contains(PersonMatchedAttribute.LastName)
                    ? personA.LastName
                    : TestData.GenerateChangedLastName(personA.LastName))
            .WithDateOfBirth(
                matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth)
                    ? personA.DateOfBirth
                    : TestData.GenerateChangedDateOfBirth(personA.DateOfBirth))
            .WithEmail(
                matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress)
                    ? personA.Email ?? ""
                    : TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(
                matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                    ? personA.NationalInsuranceNumber ?? ""
                    : TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber!)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertMatchRowHasExpectedHighlight("record-a", "First name", false);
        doc.AssertMatchRowHasExpectedHighlight("record-a", "Middle name", false);
        doc.AssertMatchRowHasExpectedHighlight("record-a", "Last name", false);
        doc.AssertMatchRowHasExpectedHighlight("record-a", "Date of birth", false);
        doc.AssertMatchRowHasExpectedHighlight("record-a", "Email", false);
        doc.AssertMatchRowHasExpectedHighlight("record-a", "National Insurance number", false);

        doc.AssertMatchRowHasExpectedHighlight("record-b", "First name", !matchedAttributes.Contains(PersonMatchedAttribute.FirstName));
        doc.AssertMatchRowHasExpectedHighlight("record-b", "Middle name", !matchedAttributes.Contains(PersonMatchedAttribute.MiddleName));
        doc.AssertMatchRowHasExpectedHighlight("record-b", "Last name", !matchedAttributes.Contains(PersonMatchedAttribute.LastName));
        doc.AssertMatchRowHasExpectedHighlight("record-b", "Date of birth", !matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth));
        doc.AssertMatchRowHasExpectedHighlight("record-b", "Email", !matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress));
        doc.AssertMatchRowHasExpectedHighlight("record-b", "National Insurance number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
    }

    [Fact]
    public async Task Post_PersonBIsDeactivated_ShowsWarningAndHidesContinueButton()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(personB.Person);
            personB.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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
        var primaryRecordOptions = doc.GetElementByTestId("primary-record-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        Assert.NotNull(warningText);
        Assert.Contains("One of these records has been deactivated. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
        Assert.Null(primaryRecordOptions);
        Assert.Null(continueButton);
    }

    [Fact]
    public async Task Get_PersonBHasOpenAlert_ShowsWarningAndHidesContinueButton()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryRecordOptions = doc.GetElementByTestId("primary-record-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        Assert.NotNull(warningText);
        Assert.Contains("One of these records has an alert. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
        Assert.Null(primaryRecordOptions);
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryRecordOptions = doc.GetElementByTestId("primary-record-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        if (expectMergeToBeAllowed)
        {
            Assert.Null(warningText);
            Assert.NotNull(primaryRecordOptions);
            Assert.NotNull(continueButton);
        }
        else
        {
            Assert.NotNull(warningText);
            Assert.Contains($"The induction status of one of these records is {status.GetDisplayName()}. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
            Assert.Null(primaryRecordOptions);
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null))
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var warningText = doc.GetElementByTestId("warning-text");
        var primaryRecordOptions = doc.GetElementByTestId("primary-record-options");
        var continueButton = doc.GetElementByTestId("continue-button");

        Assert.NotNull(warningText);
        Assert.Contains($"One of these records has an alert and an induction status of {status.GetDisplayName()}. Refer this to the Teaching Regulation Agency (TRA).", warningText.TrimmedText());
        Assert.Null(primaryRecordOptions);
        Assert.Null(continueButton);
    }

    [Fact]
    public async Task Get_PrimaryRecordAlreadySelected_SelectsChosenRecord()
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryRecord(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var primaryRecordChoice = doc.GetChildElementsOfTestId<IHtmlInputElement>("primary-record-options", "input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(personB.PersonId.ToString(), primaryRecordChoice);
    }

    [Fact]
    public async Task Post_PrimaryRecordNotSelected_ShowsPageError()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(CompareMatchingRecordsModel.PrimaryRecordId), "Select primary record");
    }

    [Fact]
    public async Task Post_PersonAIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(personA.Person);
            personA.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personB.PersonId)
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null)));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personB.PersonId)
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personB.PersonId)
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(personB.Person);
            personB.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personA.PersonId)
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personA.PersonId)
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personA.PersonId)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersistsDetailsAndRedirectsToNextPage()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithPrimaryRecordId(personB.PersonId)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/select-details-to-merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personB.PersonId, journeyInstance.State.PrimaryRecordId);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<MergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/compare-matching-records?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
