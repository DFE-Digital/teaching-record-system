using AngleSharp.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

public class MatchesTests(HostFixture hostFixture) : ResolveOneLoginUserIdVerificationTestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserIsNotVerified_RedirectsToIndex()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NoMatches_RedirectsToNoMatches()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal($"{supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}", requestDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(supportTaskData.StatedDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(oneLoginUser.EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(supportTaskData.StatedNationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(supportTaskData.StatedTrn, requestDetails.GetSummaryListValueByKey("TRN"));
    }

    [Fact]
    public async Task Get_MatchedRecords_HighlightsNotMatchedFields()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        // Person who matches on NINO
        var matchedPerson1 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(supportTaskData.StatedNationalInsuranceNumber!));

        // Person who matches on first name, last name & DOB
        var matchedPerson2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(supportTaskData.StatedFirstName)
            .WithLastName(supportTaskData.StatedLastName)
            .WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        // Person who matches on previous surname & DOB
        var matchedPerson3 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(supportTaskData.StatedFirstName)
            .WithLastName(TestData.GenerateChangedLastName(supportTaskData.StatedLastName))
            .WithPreviousNames((TestData.GenerateChangedFirstName(supportTaskData.StatedFirstName), TestData.GenerateMiddleName(), supportTaskData.StatedLastName, Clock.UtcNow))
            .WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson1.PersonId,
                matchedPerson1.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.NationalInsuranceNumber, matchedPerson1.NationalInsuranceNumber!)
                ]),
            new MatchPersonResult(
                matchedPerson2.PersonId,
                matchedPerson2.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson2.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson2.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson2.DateOfBirth.ToString("yyyy-MM-dd"))
                ]),
            new MatchPersonResult(
                matchedPerson3.PersonId,
                matchedPerson3.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson3.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson3.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson3.DateOfBirth.ToString("yyyy-MM-dd"))
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        // match on NI number appears first
        var matchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(matchDetails);
        Assert.Equal($"{matchedPerson1.FirstName} {matchedPerson1.LastName}", matchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(matchedPerson1.NationalInsuranceNumber, matchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(matchedPerson1.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), matchDetails.GetSummaryListValueByKey("Date of birth"));
        AssertMatchRowIsHighlighted(matchDetails, "Name");
        AssertMatchRowIsHighlighted(matchDetails, "Date of birth");

        // match on first name, surname and DOB appears second
        matchDetails = doc.GetAllElementsByTestId("match")[1];
        Assert.NotNull(matchDetails);
        Assert.Equal($"{matchedPerson2.FirstName} {matchedPerson2.LastName}", matchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, matchDetails.GetSummaryListValueByKey("NI number"));
        AssertMatchRowNotHighlighted(matchDetails, "Name");
        AssertMatchRowIsHighlighted(matchDetails, "NI number");

        // match on previous surname and DOB
        matchDetails = doc.GetAllElementsByTestId("match")[2];
        Assert.NotNull(matchDetails);
        Assert.Equal($"{matchedPerson3.FirstName} {matchedPerson3.LastName}", matchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal($"{matchedPerson3.PreviousNames.First().FirstName} {matchedPerson3.PreviousNames.First().MiddleName} {matchedPerson3.PreviousNames.First().LastName}", matchDetails.GetSummaryListValueByKey("Previous names"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, matchDetails.GetSummaryListValueByKey("NI number"));
        AssertMatchRowNotHighlighted(matchDetails, "Name");
        AssertMatchRowIsHighlighted(matchDetails, "NI number");
    }

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        // Person who matches on last name and DOB
        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithLastName(supportTaskData.StatedLastName)
            .WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd"))
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        var firstMatchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(firstMatchDetails);
        Assert.Equal($"{matchedPerson.FirstName} {matchedPerson.LastName}", firstMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        AssertMatchRowNotHighlighted(firstMatchDetails, "NI number");
    }

    [Fact]
    public async Task Get_ShowsExpectedMergeOptions()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        // Person who matches on last name and DOB
        var matchedPerson1 = await TestData.CreatePersonAsync(p => p
            .WithLastName(supportTaskData.StatedNationalInsuranceNumber!)
            .WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        // Person who matches on NINO
        var matchedPerson2 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(supportTaskData.StatedNationalInsuranceNumber!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson1.PersonId,
                matchedPerson1.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.LastName,  matchedPerson1.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson1.DateOfBirth.ToString("yyyy-MM-dd"))
                ]),
            new MatchPersonResult(
                matchedPerson2.PersonId,
                matchedPerson2.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.NationalInsuranceNumber, matchedPerson2.NationalInsuranceNumber!)
                ]));

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
    public async Task Post_NoChosenOption_ReturnsError()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "MatchedPersonId", "Select what you want to do with this GOV.UK One Login");
    }

    [Fact]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", matchedPerson.PersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(matchedPerson.PersonId, journeyInstance.State.MatchedPersonId);
    }

    [Fact]
    public async Task Post_DoNotConnectToRecordChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel, journeyInstance.State.MatchedPersonId);
    }

    [Fact]
    public async Task Post_SaveAndComeBackLater_PersistsJourneyStateIntoTaskAndRedirectsToListPage()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var chosenPersonId = matchedPerson.PersonId;

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Action", "SaveAndComeBackLater" },
                { "MatchedPersonId", chosenPersonId.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            supportTask = (await dbContext.SupportTasks.FindAsync(supportTask.SupportTaskReference))!;
            Assert.NotNull(supportTask.ResolveJourneySavedState);

            Assert.Equal("Matches", supportTask.ResolveJourneySavedState.PageName);

            Assert.Collection(
                supportTask.ResolveJourneySavedState.ModelStateValues,
                kvp =>
                {
                    Assert.Equal("MatchedPersonId", kvp.Key);
                    Assert.Equal(chosenPersonId.ToString(), kvp.Value);
                });

            var savedState = supportTask.ResolveJourneySavedState.GetState<ResolveOneLoginUserIdVerificationState>();
            Assert.NotNull(savedState);
            Assert.True(savedState.Verified);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToListPage()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Action", "Cancel" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private void AssertMatchRowIsHighlighted(IElement matchDetails, string summaryListKey)
    {
        var valueElement = matchDetails.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

        Assert.False(highlightElement == null, $"{summaryListKey} should be highlighted");
    }

    private void AssertMatchRowNotHighlighted(IElement matchDetails, string summaryListKey)
    {
        var valueElement = matchDetails.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

        Assert.True(highlightElement == null, $"{summaryListKey} should not be highlighted");
    }
}
