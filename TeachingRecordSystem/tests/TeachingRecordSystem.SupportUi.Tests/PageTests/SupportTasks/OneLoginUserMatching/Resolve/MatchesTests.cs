using AngleSharp.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class MatchesTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_NoMatches_RedirectsToNoMatches(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ValidRequest_ShowsRequestDetails(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithStatedFirstName(matchedPerson.FirstName)
                    .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));
        var supportTaskData = supportTask.GetData<IOneLoginUserMatchingData>();
        var firstVerifiedOrStatedName = supportTaskData.VerifiedOrStatedNames!.First();

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal($"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.Last()}", requestDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(supportTaskData.VerifiedOrStatedDatesOfBirth!.First().ToString(WebConstants.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(oneLoginUser.EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(supportTaskData.StatedNationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(supportTaskData.StatedTrn, requestDetails.GetSummaryListValueByKey("TRN"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_MatchedRecords_HighlightsNotMatchedFields(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var supportTaskData = supportTask.GetData<IOneLoginUserMatchingData>();
        var firstVerifiedOrStatedName = supportTaskData.VerifiedOrStatedNames!.First();
        var firstVerifiedOrStatedDateOfBirth = supportTaskData.VerifiedOrStatedDatesOfBirth!.First();

        // Person who matches on NINO
        var matchedPerson1 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(supportTaskData.StatedNationalInsuranceNumber!));

        // Person who matches on first name, last name & DOB
        var matchedPerson2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstVerifiedOrStatedName.First())
            .WithLastName(firstVerifiedOrStatedName.Last())
            .WithDateOfBirth(firstVerifiedOrStatedDateOfBirth));

        // Person who matches on previous surname & DOB
        var matchedPerson3 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstVerifiedOrStatedName.First())
            .WithLastName(TestData.GenerateChangedLastName(firstVerifiedOrStatedName.Last()))
            .WithPreviousNames((TestData.GenerateChangedFirstName(firstVerifiedOrStatedName.First()), TestData.GenerateMiddleName(), firstVerifiedOrStatedName.Last(), Clock.UtcNow))
            .WithDateOfBirth(firstVerifiedOrStatedDateOfBirth));

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        // match on NI number appears first
        var matchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(matchDetails);
        Assert.Equal($"{matchedPerson1.FirstName} {matchedPerson1.LastName}", matchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(matchedPerson1.NationalInsuranceNumber, matchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(matchedPerson1.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), matchDetails.GetSummaryListValueByKey("Date of birth"));
        AssertMatchRowIsHighlighted(matchDetails, "Name");
        AssertMatchRowIsHighlighted(matchDetails, "Date of birth");

        // match on first name, surname and DOB appears second
        matchDetails = doc.GetAllElementsByTestId("match")[1];
        Assert.NotNull(matchDetails);
        Assert.Equal($"{matchedPerson2.FirstName} {matchedPerson2.LastName}", matchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(WebConstants.EmptyFallbackContent, matchDetails.GetSummaryListValueByKey("NI number"));
        AssertMatchRowNotHighlighted(matchDetails, "Name");
        AssertMatchRowIsHighlighted(matchDetails, "NI number");

        // match on previous surname and DOB
        matchDetails = doc.GetAllElementsByTestId("match")[2];
        Assert.NotNull(matchDetails);
        Assert.Equal($"{matchedPerson3.FirstName} {matchedPerson3.LastName}", matchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal($"{matchedPerson3.PreviousNames.First().FirstName} {matchedPerson3.PreviousNames.First().MiddleName} {matchedPerson3.PreviousNames.First().LastName}", matchDetails.GetSummaryListValueByKey("Previous names"));
        Assert.Equal(WebConstants.EmptyFallbackContent, matchDetails.GetSummaryListValueByKey("NI number"));
        AssertMatchRowNotHighlighted(matchDetails, "Name");
        AssertMatchRowIsHighlighted(matchDetails, "NI number");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var supportTaskData = supportTask.GetData<IOneLoginUserMatchingData>();
        var firstVerifiedOrStatedName = supportTaskData.VerifiedOrStatedNames!.First();
        var firstVerifiedOrStatedDateOfBirth = supportTaskData.VerifiedOrStatedDatesOfBirth!.First();

        // Person who matches on last name and DOB
        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithLastName(firstVerifiedOrStatedName.Last())
            .WithDateOfBirth(firstVerifiedOrStatedDateOfBirth));

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        var firstMatchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(firstMatchDetails);
        Assert.Equal($"{matchedPerson.FirstName} {matchedPerson.LastName}", firstMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(WebConstants.EmptyFallbackContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ShowsExpectedMergeOptions(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var supportTaskData = supportTask.GetData<IOneLoginUserMatchingData>();
        var firstVerifiedOrStatedName = supportTaskData.VerifiedOrStatedNames!.First();
        var firstVerifiedOrStatedDateOfBirth = supportTaskData.VerifiedOrStatedDatesOfBirth!.First();

        // Person who matches on last name and DOB
        var matchedPerson1 = await TestData.CreatePersonAsync(p => p
            .WithLastName(firstVerifiedOrStatedName.Last())
            .WithDateOfBirth(firstVerifiedOrStatedDateOfBirth));

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_NoChosenOption_ReturnsError(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "MatchedPersonId", "Select what you want to do with this GOV.UK One Login");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", matchedPerson.PersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(matchedPerson.PersonId, journeyInstance.State.MatchedPersonId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_DoNotConnectToRecordChosen_UpdatesStateAndRedirects(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "MatchedPersonId", ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel, journeyInstance.State.MatchedPersonId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_SaveAndComeBackLater_PersistsJourneyStateIntoTaskAndRedirectsToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        if (isRecordMatchingOnlySupportTask)
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
        }

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

            var savedState = supportTask.ResolveJourneySavedState.GetState<ResolveOneLoginUserMatchingState>();
            Assert.NotNull(savedState);
            Assert.True(savedState.Verified);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskSaving, p.ProcessContext.ProcessType));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Action", "Cancel" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (isRecordMatchingOnlySupportTask)
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
        }

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
