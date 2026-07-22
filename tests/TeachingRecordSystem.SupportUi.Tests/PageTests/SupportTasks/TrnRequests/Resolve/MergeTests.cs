using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequests.Resolve;

public class MergeTests(HostFixture hostFixture) : ResolveApiTrnRequestTestBase(hostFixture)
{
    [Theory]
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeIsNotDifferent_RendersDisabledAndUnselectedRadioButtons(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            personId: matchedPersonIds[0]);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    [MemberData(nameof(GetAttributesAndFieldsData))]
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
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToTrnRequestInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(matchedPerson.PersonId, [])],
                PersonId = matchedPerson.PersonId,
                FirstNameSource = PersonAttributeSource.TrnRequest,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                LastNameSource = PersonAttributeSource.TrnRequest,
                DateOfBirthSource = PersonAttributeSource.TrnRequest,
                EmailAddressSource = PersonAttributeSource.TrnRequest,
                NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
                GenderSource = PersonAttributeSource.TrnRequest,
                Comments = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[0].IsChecked());
    }

    [Theory]
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToExistingRecordInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(matchedPerson.PersonId, [])],
                PersonId = matchedPerson.PersonId,
                FirstNameSource = PersonAttributeSource.ExistingRecord,
                MiddleNameSource = PersonAttributeSource.ExistingRecord,
                LastNameSource = PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
                Comments = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = "Some comments";

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.Equal(comments, doc.GetElementsByName("Comments").Single().TrimmedText());
    }

    [Theory]
    [InlineData(PersonMatchedAttribute.FirstName, "FirstNameSource", "Select a first name")]
    [InlineData(PersonMatchedAttribute.MiddleName, "MiddleNameSource", "Select a middle name")]
    [InlineData(PersonMatchedAttribute.LastName, "LastNameSource", "Select a last name")]
    [InlineData(PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource", "Select a date of birth")]
    [InlineData(PersonMatchedAttribute.EmailAddress, "EmailAddressSource", "Select an email")]
    [InlineData(PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", "Select a National Insurance number")]
    [InlineData(PersonMatchedAttribute.Gender, "GenderSource", "Select a gender")]
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
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            personId: matchedPersonIds[0]);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        // Deliberately create a support task with all differences to test setting all fields (even though it would not actually be a match!)
        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(matchedPerson.PersonId, [])],
                PersonId = matchedPerson.PersonId
            });

        var firstNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var middleNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var lastNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var dateOfBirthSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var emailAddressSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var nationalInsuranceNumberSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var genderSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "FirstNameSource", firstNameSelection },
                { "MiddleNameSource", middleNameSelection },
                { "LastNameSource", lastNameSelection },
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
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(firstNameSelection, journeyState!.FirstNameSource);
        Assert.Equal(middleNameSelection, journeyState!.MiddleNameSource);
        Assert.Equal(lastNameSelection, journeyState!.LastNameSource);
        Assert.Equal(dateOfBirthSelection, journeyState!.DateOfBirthSource);
        Assert.Equal(emailAddressSelection, journeyState!.EmailAddressSource);
        Assert.Equal(nationalInsuranceNumberSelection, journeyState!.NationalInsuranceNumberSource);
        Assert.Equal(genderSelection, journeyState!.GenderSource);
    }

    public static (PersonMatchedAttribute Attribute, string SourceFieldName)[] GetAttributesAndFieldsData() =>
    [
        (PersonMatchedAttribute.FirstName, "FirstNameSource"),
        (PersonMatchedAttribute.MiddleName, "MiddleNameSource"),
        (PersonMatchedAttribute.LastName, "LastNameSource"),
        (PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource"),
        (PersonMatchedAttribute.EmailAddress, "EmailAddressSource"),
        (PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource"),
        (PersonMatchedAttribute.Gender, "GenderSource")
    ];

    private async Task<ResolveTrnRequestJourneyCoordinator> CreateJourneyInstance(
        SupportTask supportTask,
        Guid? personId)
    {
        var state = await CreateStateAsync(supportTask);
        state.PersonId = personId;

        return await CreateJourneyInstanceAsync(supportTask.SupportTaskReference, state);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToListPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, matchedPerson) = await CreateSupportTaskWithAllDifferences(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(matchedPerson.PersonId, [])],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "True" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/trn-requests", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
