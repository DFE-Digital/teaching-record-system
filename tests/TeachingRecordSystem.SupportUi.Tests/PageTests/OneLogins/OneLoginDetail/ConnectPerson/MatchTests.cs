using AngleSharp.Dom;
using Optional;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.ConnectPerson;

public class MatchTests(HostFixture hostFixture) : ConnectPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutJourneyInstance_ReturnsBadRequest()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithInvalidOneLoginUserSubject_ReturnsNotFound()
    {
        // Arrange
        var oneLoginUserSubject = "invalid-subject";

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUserSubject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUserSubject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidOneLoginUserAndPerson_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithMiddleName("")
            .WithLastName("Doe")
            .WithDateOfBirth(new DateOnly(1990, 1, 15))
            .WithEmailAddress("john.doe@example.com")
            .WithNationalInsuranceNumber("AB123456C"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var oneLoginCard = doc.GetElementByTestId("one-login");
        Assert.NotNull(oneLoginCard);
        Assert.Equal("John Doe", oneLoginCard.GetSummaryListValueByKey("Name"));
        Assert.Equal("test@example.com", oneLoginCard.GetSummaryListValueByKey("Email address"));
        Assert.Equal("15 January 1990", oneLoginCard.GetSummaryListValueByKey("Date of birth"));

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        Assert.Equal("John Doe", recordCard.GetSummaryListValueByKey("Name"));
        Assert.Equal("john.doe@example.com", recordCard.GetSummaryListValueByKey("Email address"));
        Assert.Equal("15 January 1990", recordCard.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(person.Trn, recordCard.GetSummaryListValueByKey("TRN"));
        Assert.Equal("AB123456C", recordCard.GetSummaryListValueByKey("National Insurance number"));

        var viewRecordLink = doc.QuerySelector("a[href*='/persons/']");
        Assert.NotNull(viewRecordLink);
        Assert.Contains(person.PersonId.ToString(), viewRecordLink.GetAttribute("href"));
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            .WithMiddleName("")
            .WithLastName("Doe")
            .WithDateOfBirth(new DateOnly(1990, 1, 15))
            .WithEmailAddress("test@example.com"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowNotHighlighted(recordCard, "Name");
        AssertPersonRowNotHighlighted(recordCard, "Email address");
        AssertPersonRowNotHighlighted(recordCard, "Date of birth");
    }

    [Fact]
    public async Task Get_WithDifferentNames_HighlightsNameDifferences()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .WithDateOfBirth(new DateOnly(1990, 1, 15)));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowIsHighlighted(recordCard, "Name");
    }

    [Fact]
    public async Task Get_WithDifferentDateOfBirth_HighlightsDateOfBirthDifference()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithDateOfBirth(new DateOnly(1985, 5, 20)));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowIsHighlighted(recordCard, "Date of birth");
    }

    [Fact]
    public async Task Get_WithDifferentEmailAddress_HighlightsEmailDifference()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailAddress("person@example.com"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("onelogin@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowIsHighlighted(recordCard, "Email address");
    }

    [Fact]
    public async Task Get_WithMultipleVerifiedNames_OneMatching_DoesNotHighlightNames()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("John")
            .WithMiddleName("")
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowNotHighlighted(recordCard, "Name");
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowIsHighlighted(recordCard, "Name");
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
                        new DateOnly(1990, 1, 15),  // This one matches
                        new DateOnly(1992, 12, 25)
                    ])));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowNotHighlighted(recordCard, "Date of birth");
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var recordCard = doc.GetElementByTestId("record");
        Assert.NotNull(recordCard);
        AssertPersonRowIsHighlighted(recordCard, "Date of birth");
    }

    [Fact]
    public async Task Get_WithExistingConnectedOneLoginUsers_DisplaysConnectedEmails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateOneLoginUserAsync(person, email: Option.Some<string?>("existing1@example.com"));
        await TestData.CreateOneLoginUserAsync(person, email: Option.Some<string?>("existing2@example.com"));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("new@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Contains("existing1@example.com", doc.Body!.TextContent);
        Assert.Contains("existing2@example.com", doc.Body!.TextContent);
    }

    [Fact]
    public async Task Post_ValidSubmission_RedirectsToReasonPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/one-logins/{oneLoginUser.Subject}/connect-person/reason?", response.Headers.Location?.OriginalString);
        Assert.Contains(journeyInstance.GetUniqueIdQueryParameter(), response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToOneLoginDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "True" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    private void AssertPersonRowIsHighlighted(IElement recordCard, string summaryListKey)
    {
        var valueElement = recordCard.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElements = valueElement.GetElementsByClassName("hods-highlight");

        Assert.False(highlightElements.Length == 0, $"{summaryListKey} should be highlighted");
    }

    private void AssertPersonRowNotHighlighted(IElement recordCard, string summaryListKey)
    {
        var valueElement = recordCard.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElements = valueElement.GetElementsByClassName("hods-highlight");

        Assert.True(highlightElements.Length == 0, $"{summaryListKey} should not be highlighted");
    }
}
