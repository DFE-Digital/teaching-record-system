using AngleSharp.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class MatchesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private OneLoginUser[]? OneLoginUsers { get; set; }
    private SupportTask[]? SupportTasks { get; set; }

    private async Task CreateSupportTasksWithOneLoginUsersAsync()
    {
        OneLoginUsers = new OneLoginUser[]
        {
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null),
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null)
        };

        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[0].Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[1].Subject);

        SupportTasks = new SupportTask[]
        {
            supportTask1,
            supportTask2
        };
    }

    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{taskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();
        var supportTask = SupportTasks![0];
        supportTask.Status = SupportTaskStatus.Closed;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();
        var supportTask = SupportTasks![0];
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;
        var person = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', supportTaskData!.StatedFirstName, supportTaskData.StatedLastName), requestDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(supportTaskData.StatedDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(OneLoginUsers![0].EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(supportTaskData.StatedNationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(supportTaskData.StatedTrn, requestDetails.GetSummaryListValueByKey("TRN"));
    }

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordButPopulatedInRequest_ShowsHighlightedNotProvided()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject, options =>
        {
            options.WithStatedNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person1 = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        // Person who matches on NINO
        var person2 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(supportTaskData!.StatedNationalInsuranceNumber!));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        // match on NI number appears first
        var nextMatchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(nextMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', person2.FirstName, person2.LastName), nextMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(person2.NationalInsuranceNumber, nextMatchDetails.GetSummaryListValueByKey("National Insurance number"));
        Assert.Equal(person2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), nextMatchDetails.GetSummaryListValueByKey("Date of birth"));
        AssertMatchRowHasExpectedHighlight(nextMatchDetails, "Name", true);
        AssertMatchRowHasExpectedHighlight(nextMatchDetails, "Date of birth", true);

        // match on surname and DOB appears second
        var firstMatchDetails = doc.GetAllElementsByTestId("match")[1];
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', person1.FirstName, person1.LastName), firstMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("National Insurance number"));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "National Insurance number", true);
    }

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject);
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person1 = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        // match on surname and DOB appears second
        var firstMatchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', person1.FirstName, person1.LastName), firstMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("National Insurance number"));
    }

    [Fact]
    public async Task Get_WithDefiniteMatch_ShowsExpectedMergeOptions()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject, options =>
        {
            options.WithStatedNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person1 = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        // Person who matches on NINO
        var person2 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(supportTaskData!.StatedNationalInsuranceNumber!));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radioInputs = doc.QuerySelectorAll("input[type='radio']");
        Assert.Equal(3, radioInputs.Length);
        Assert.Equal("Connect it to Record A", radioInputs[0].NextElementSibling?.TextContent.Trim());
        Assert.Equal("Connect it to Record B", radioInputs[1].NextElementSibling?.TextContent.Trim());
        Assert.Equal("Do not connect it to a record", radioInputs[2].NextElementSibling?.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithNoMatches_RedirectsToNoMatchesPage()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject, options =>
        {
            options.WithStatedNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        var person = await TestData.CreatePersonAsync();

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject, options =>
        {
            options.WithStatus(SupportTaskStatus.Closed);
        });
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", person.PersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_SubmittedPersonIdIsNotValid_ReturnsBadRequest()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject);
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", Guid.NewGuid() } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NoChosenOption_ReturnsError()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject);
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "MatchedPersonId", "Select what you want to do with this GOV.UK One Login account");
    }

    [Fact]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject);
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", person.PersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(person.PersonId, journeyInstance.State.MatchedPersonId);
    }

    [Fact]
    public async Task Post_DoNotConnectToRecordChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject);
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", ResolveOneLoginUserIdVerificationState.DoNotConnectARecordPersonIdSentinel } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ResolveOneLoginUserIdVerificationState.DoNotConnectARecordPersonIdSentinel, journeyInstance.State.MatchedPersonId);
    }

    private void AssertMatchRowHasExpectedHighlight(IElement matchDetails, string summaryListKey, bool expectHighlight)
    {
        var valueElement = matchDetails.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

        if (expectHighlight)
        {
            Assert.False(highlightElement == null, $"{summaryListKey} should be highlighted");
        }
        else
        {
            Assert.True(highlightElement == null, $"{summaryListKey} should not be highlighted");
        }
    }
}
