using AngleSharp.Dom;
using Optional;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.ConnectOneLogin;

public class MatchTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutMatchedOneLogin_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithoutJourneyInstance_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithInvalidPersonId_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", personId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidPersonAndOneLoginUser_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithDateOfBirth(new DateOnly(1990, 1, 15))
            .WithEmailAddress("john.doe@example.com")
            .WithNationalInsuranceNumber("AB123456C"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.FirstName, person.FirstName),
                        KeyValuePair.Create(PersonMatchedAttribute.LastName, person.LastName),
                        KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, person.DateOfBirth.ToString("yyyy-MM-dd"))
                    ]),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        Assert.Equal("John Doe", recordCard.GetSummaryListValueByKey("Name"));
        Assert.Equal("john.doe@example.com", recordCard.GetSummaryListValueByKey("Email address"));
        Assert.Equal("15 January 1990", recordCard.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(person.Trn, recordCard.GetSummaryListValueByKey("TRN"));
        Assert.Equal("AB123456C", recordCard.GetSummaryListValueByKey("National Insurance number"));

        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);
        Assert.Equal("John Doe", oneLoginCard.GetSummaryListValueByKey("Name"));
        Assert.Equal("test@example.com", oneLoginCard.GetSummaryListValueByKey("Email address"));
        Assert.Equal("15 January 1990", oneLoginCard.GetSummaryListValueByKey("Date of birth"));
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedNames_DisplaysAllNames()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Smith"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Smith"], new DateOnly(1990, 1, 15)));

        await WithDbContextAsync(dbContext =>
            dbContext.OneLoginUsers
                .Where(u => u.Subject == oneLoginUser.Subject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VerifiedNames,
                    [
                        ["John", "Smith"],
                        ["Jane", "Doe"],
                        ["Johnny", "Smith"]
                    ])));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.FirstName, person.FirstName),
                        KeyValuePair.Create(PersonMatchedAttribute.LastName, person.LastName)
                    ]),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);


        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        var listItems = oneLoginCard.GetSummaryListValueElementByKey("Name")?.QuerySelectorAll("li");
        Assert.NotNull(listItems);
        Assert.Equal(3, listItems.Length);
        Assert.Equal("John Smith", listItems[0].TextContent.Trim());
        Assert.Equal("Jane Doe", listItems[1].TextContent.Trim());
        Assert.Equal("Johnny Smith", listItems[2].TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedDatesOfBirth_DisplaysAllDates()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(new DateOnly(1990, 1, 15)));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        await WithDbContextAsync(dbContext =>
            dbContext.OneLoginUsers
                .Where(u => u.Subject == oneLoginUser.Subject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VerifiedDatesOfBirth,
                    [
                        new DateOnly(1990, 1, 15),
                        new DateOnly(1985, 5, 20),
                        new DateOnly(1992, 12, 25)
                    ])));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, person.DateOfBirth.ToString("yyyy-MM-dd"))
                    ]),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        var listItems = oneLoginCard.GetSummaryListValueElementByKey("Date of birth")?.QuerySelectorAll("li");
        Assert.NotNull(listItems);
        Assert.Equal(3, listItems.Length);
        Assert.Equal("15 January 1990", listItems[0].TextContent.Trim());
        Assert.Equal("20 May 1985", listItems[1].TextContent.Trim());
        Assert.Equal("25 December 1992", listItems[2].TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithNoVerifiedData_DisplaysEmptyFallback()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            email: Option.Some<string?>("test@example.com"),
            verified: false);

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, []),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        Assert.Equal(WebConstants.EmptyFallbackContent, oneLoginCard.GetSummaryListValueByKey("Name"));
        Assert.Equal(WebConstants.EmptyFallbackContent, oneLoginCard.GetSummaryListValueByKey("Date of birth"));
    }

    [Fact]
    public async Task Get_WithMatchingAttributes_DoesNotHighlightMatchedValues()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithDateOfBirth(new DateOnly(1990, 1, 15))
            .WithEmailAddress("test@example.com"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.FirstName, person.FirstName),
                        KeyValuePair.Create(PersonMatchedAttribute.LastName, person.LastName),
                        KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, person.DateOfBirth.ToString("yyyy-MM-dd")),
                        KeyValuePair.Create(PersonMatchedAttribute.EmailAddress, person.EmailAddress!)
                    ]),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);
        AssertOneLoginRowNotHighlighted(oneLoginCard, "Name");
        AssertOneLoginRowNotHighlighted(oneLoginCard, "Email address");
        AssertOneLoginRowNotHighlighted(oneLoginCard, "Date of birth");
    }

    [Fact]
    public async Task Get_WithNonMatchingName_HighlightsName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Smith"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["Jane", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    []),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);
        AssertOneLoginRowIsHighlighted(oneLoginCard, "Name");
    }

    [Fact]
    public async Task Get_WithNonMatchingDateOfBirth_HighlightsDOB()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(new DateOnly(1990, 1, 15)));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1985, 5, 20)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    []),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);
        AssertOneLoginRowIsHighlighted(oneLoginCard, "Date of birth");
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedNames_OneMatching_DoesNotHighlightNames()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Smith"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["Jane", "Doe"], new DateOnly(1990, 1, 15)));

        // Add multiple verified names where one matches
        await WithDbContextAsync(dbContext =>
            dbContext.OneLoginUsers
                .Where(u => u.Subject == oneLoginUser.Subject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VerifiedNames,
                    [
                        ["Jane", "Doe"],
                        ["John", "Smith"],  // This one matches
                        ["Johnny", "Bloggs"]
                    ])));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.FirstName, person.FirstName),
                        KeyValuePair.Create(PersonMatchedAttribute.LastName, person.LastName)
                    ]),  // Name matches (one of the multiple names)

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        AssertOneLoginRowNotHighlighted(oneLoginCard, "Name");
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedNames_NoneMatching_HighlightsAllNames()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Smith"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["Jane", "Doe"], new DateOnly(1990, 1, 15)));

        await WithDbContextAsync(dbContext =>
            dbContext.OneLoginUsers
                .Where(u => u.Subject == oneLoginUser.Subject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VerifiedNames,
                    [
                        ["Jane", "Doe"],
                        ["Johnny", "Bloggs"],
                        ["Jim", "Brown"]
                    ])));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    []),  // No matched attributes - none of the names match

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        AssertOneLoginRowIsHighlighted(oneLoginCard, "Name");
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedDatesOfBirth_OneMatching_DoesNotHighlightDOBs()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(new DateOnly(1990, 1, 15)));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1985, 5, 20)));

        await WithDbContextAsync(dbContext =>
            dbContext.OneLoginUsers
                .Where(u => u.Subject == oneLoginUser.Subject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VerifiedDatesOfBirth,
                    [
                        new DateOnly(1985, 5, 20),
                        new DateOnly(1990, 1, 15),
                        new DateOnly(1992, 12, 25)
                    ])));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, person.DateOfBirth.ToString("yyyy-MM-dd"))
                    ]),  // DOB matches (one of the multiple DOBs)

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        AssertOneLoginRowNotHighlighted(oneLoginCard, "Date of birth");
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedDatesOfBirth_NoneMatching_HighlightsAllDOBs()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(new DateOnly(1990, 1, 15)));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1985, 5, 20)));

        await WithDbContextAsync(dbContext =>
            dbContext.OneLoginUsers
                .Where(u => u.Subject == oneLoginUser.Subject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VerifiedDatesOfBirth,
                    [
                        new DateOnly(1985, 5, 20),
                        new DateOnly(1988, 8, 10),
                        new DateOnly(1992, 12, 25)
                    ])));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    []),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);

        AssertOneLoginRowIsHighlighted(oneLoginCard, "Date of birth");
    }

    [Fact]
    public async Task Post_RedirectsToReasonPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.FirstName, person.FirstName),
                        KeyValuePair.Create(PersonMatchedAttribute.LastName, person.LastName),
                        KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, person.DateOfBirth.ToString("yyyy-MM-dd"))
                    ]),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/connect-one-login/reason", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                MatchedPerson = new MatchPersonResult(
                    person.PersonId,
                    person.Trn,
                    [
                        KeyValuePair.Create(PersonMatchedAttribute.FirstName, person.FirstName),
                        KeyValuePair.Create(PersonMatchedAttribute.LastName, person.LastName),
                        KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, person.DateOfBirth.ToString("yyyy-MM-dd"))
                    ]),

            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/match?handler=Cancel&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private void AssertOneLoginRowIsHighlighted(IElement oneLoginCard, string summaryListKey)
    {
        var valueElement = oneLoginCard.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElements = valueElement.GetElementsByClassName("hods-highlight");

        Assert.False(highlightElements.Length == 0, $"{summaryListKey} should be highlighted");
    }

    private void AssertOneLoginRowNotHighlighted(IElement oneLoginCard, string summaryListKey)
    {
        var valueElement = oneLoginCard.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElements = valueElement.GetElementsByClassName("hods-highlight");

        Assert.True(highlightElements.Length == 0, $"{summaryListKey} should not be highlighted");
    }
}

