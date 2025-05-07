using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public class MergeTests(HostFixture hostFixture) : ResolveApiTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = null });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_CreateNewRecordSelected_RedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = CreateNewRecordPersonIdSentinel });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeIsNotDifferent_RendersDisabledAndUnselectedRadioButtons(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeIsDifferent_RendersRadioButtonsWithExistingValueHighlighted(
        PersonMatchedAttribute attribute,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(applicationUser.UserId, attribute);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            matchedPerson.PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToTrnRequestInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                FirstNameSource = PersonAttributeSource.TrnRequest,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                LastNameSource = PersonAttributeSource.TrnRequest,
                DateOfBirthSource = PersonAttributeSource.TrnRequest,
                EmailAddressSource = PersonAttributeSource.TrnRequest,
                NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
                Comments = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[0].IsChecked());
    }

    [Theory]
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToExistingRecordInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                PersonId = matchedPerson.PersonId,
                FirstNameSource = PersonAttributeSource.ExistingRecord,
                MiddleNameSource = PersonAttributeSource.ExistingRecord,
                LastNameSource = PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                Comments = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[1].IsChecked());
    }

    [Fact]
    public async Task Get_CommentsSetInState_RendersExistingValue()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = "Some comments";

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.Equal(comments, doc.GetElementsByName("Comments").Single().TextContent);
    }

    [Fact]
    public async Task Post_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = null });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_CreateNewRecordSelected_RedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = CreateNewRecordPersonIdSentinel });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(PersonMatchedAttribute.FirstName, "FirstNameSource", "Select a first name")]
    [InlineData(PersonMatchedAttribute.MiddleName, "MiddleNameSource", "Select a middle name")]
    [InlineData(PersonMatchedAttribute.LastName, "LastNameSource", "Select a last name")]
    [InlineData(PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource", "Select a date of birth")]
    [InlineData(PersonMatchedAttribute.EmailAddress, "EmailAddressSource", "Select an email")]
    [InlineData(PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", "Select a National Insurance number")]
    public async Task Post_AttributeSourceNotSelected_RendersError(
        PersonMatchedAttribute differentAttribute,
        string fieldName,
        string expectedErrorMessage)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(applicationUser.UserId, differentAttribute);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            matchedPerson.PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, fieldName, expectedErrorMessage);
    }

    [Fact]
    public async Task Post_EmptyRequestWithNoDifferencesToSelect_Succeeds()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.True((int)response.StatusCode < 400);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            matchedPerson.PersonId);

        var firstNameSelection = Enum.GetValues<PersonAttributeSource>().RandomOne();
        var middleNameSelection = Enum.GetValues<PersonAttributeSource>().RandomOne();
        var lastNameSelection = Enum.GetValues<PersonAttributeSource>().RandomOne();
        var dateOfBirthSelection = Enum.GetValues<PersonAttributeSource>().RandomOne();
        var emailAddressSelection = Enum.GetValues<PersonAttributeSource>().RandomOne();
        var nationalInsuranceNumberSelection = Enum.GetValues<PersonAttributeSource>().RandomOne();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "FirstNameSource", firstNameSelection },
                { "MiddleNameSource", middleNameSelection },
                { "LastNameSource", lastNameSelection },
                { "DateOfBirthSource", dateOfBirthSelection },
                { "EmailAddressSource", emailAddressSelection },
                { "NationalInsuranceNumberSource", nationalInsuranceNumberSelection },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(firstNameSelection, journeyInstance.State.FirstNameSource);
        Assert.Equal(middleNameSelection, journeyInstance.State.MiddleNameSource);
        Assert.Equal(lastNameSelection, journeyInstance.State.LastNameSource);
        Assert.Equal(dateOfBirthSelection, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(emailAddressSelection, journeyInstance.State.EmailAddressSource);
        Assert.Equal(nationalInsuranceNumberSelection, journeyInstance.State.NationalInsuranceNumberSource);
    }

    public static TheoryData<PersonMatchedAttribute, string> AttributesAndFieldsData { get; } = new()
    {
        { PersonMatchedAttribute.FirstName, "FirstNameSource" },
        { PersonMatchedAttribute.MiddleName, "MiddleNameSource" },
        { PersonMatchedAttribute.LastName, "LastNameSource" },
        { PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource" },
        { PersonMatchedAttribute.EmailAddress, "EmailAddressSource" },
        { PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource" }
    };

    private Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            Guid personId) =>
        CreateJourneyInstance(supportTaskReference, new ResolveApiTrnRequestState() { PersonId = personId });

    private Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveApiTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveApiTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
