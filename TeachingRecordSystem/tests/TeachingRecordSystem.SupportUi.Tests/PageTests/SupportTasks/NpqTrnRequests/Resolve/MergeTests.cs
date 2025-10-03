using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;

public class MergeTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Test]
    [Arguments(false, "matches")]
    [Arguments(true, "check-answers")]
    public async Task Get_HasExpectedBackLink(bool fromCheckAnswers, string expectedBackLink)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}" + (fromCheckAnswers ? "&fromCheckAnswers=true" : ""));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/{expectedBackLink}?{journeyInstance.GetUniqueIdQueryParameter()}", doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
    }

    [Test]
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_CreateNewRecordSelected_RedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    [MethodDataSource(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeIsNotDifferent_RendersDisabledAndUnselectedRadioButtons(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            personId: supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);

        foreach (var radio in radios)
        {
            Assert.True(radio.IsDisabled());
            Assert.False(radio.IsChecked());
        }
    }

    [Test]
    [MethodDataSource(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeIsDifferent_RendersRadioButtonsWithExistingValueHighlighted(
        PersonMatchedAttribute attribute,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(applicationUser.UserId, attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask, personId: matchedPerson.PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);

        Assert.Collection(
            radios,
            fromRequestRadio =>
            {
                Assert.False(fromRequestRadio.IsDisabled());
            },
            fromExistingRecordRadio =>
            {
                Assert.False(fromExistingRecordRadio.IsDisabled());
                Assert.NotEmpty(
                    fromExistingRecordRadio.GetAncestor<IHtmlDivElement>()!.GetElementsByClassName("hods-highlight"));
            });
    }

    [Test]
    [MethodDataSource(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToTrnRequestInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                DateOfBirthSource = PersonAttributeSource.TrnRequest,
                EmailAddressSource = PersonAttributeSource.TrnRequest,
                NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
                GenderSource = PersonAttributeSource.TrnRequest,
                Comments = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[0].IsChecked());
    }

    [Test]
    [MethodDataSource(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToExistingRecordInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = matchedPerson.PersonId,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
                Comments = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[1].IsChecked());
    }

    [Test]
    public async Task Get_CommentsSetInState_RendersExistingValue()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = "Some comments";

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.Equal(comments, doc.GetElementsByName("Comments").Single().TrimmedText());
    }

    [Test]
    public async Task Post_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_CreateNewRecordSelected_RedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource", "Select a date of birth")]
    [Arguments(PersonMatchedAttribute.EmailAddress, "EmailAddressSource", "Select an email")]
    [Arguments(PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", "Select a National Insurance number")]
    public async Task Post_AttributeSourceNotSelected_RendersError(
        PersonMatchedAttribute differentAttribute,
        string fieldName,
        string expectedErrorMessage)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(applicationUser.UserId, differentAttribute);

        var journeyInstance = await CreateJourneyInstance(supportTask, personId: matchedPerson.PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, fieldName, expectedErrorMessage);
    }

    [Test]
    public async Task Post_EmptyRequestWithNoDifferencesToSelect_Succeeds()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            personId: supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.True((int)response.StatusCode < 400);
    }

    [Test]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask, personId: matchedPerson.PersonId);

        var dateOfBirthSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var emailAddressSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var nationalInsuranceNumberSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var genderSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "DateOfBirthSource", dateOfBirthSelection },
                { "EmailAddressSource", emailAddressSelection },
                { "NationalInsuranceNumberSource", nationalInsuranceNumberSelection },
                { "GenderSource", genderSelection }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(dateOfBirthSelection, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(emailAddressSelection, journeyInstance.State.EmailAddressSource);
        Assert.Equal(nationalInsuranceNumberSelection, journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(genderSelection, journeyInstance.State.GenderSource);
    }

    [Test]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask, personId: matchedPerson.PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/support-tasks/npq-trn-requests", location);
        Assert.Null(await ReloadJourneyInstance(journeyInstance));
    }

    public static (PersonMatchedAttribute Attribute, string FieldName)[] GetAttributesAndFieldsData() =>
    [
        (PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource"),
        (PersonMatchedAttribute.EmailAddress, "EmailAddressSource"),
        (PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource"),
        (PersonMatchedAttribute.Gender, "GenderSource")
    ];

    private async Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(SupportTask supportTask, Guid? personId)
    {
        var state = await CreateJourneyStateWithFactory<ResolveNpqTrnRequestStateFactory, ResolveNpqTrnRequestState>(
            factory => factory.CreateAsync(supportTask));
        state.PersonId = personId;

        return await CreateJourneyInstance(supportTask.SupportTaskReference, state);
    }

    private Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveNpqTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveNpqTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
