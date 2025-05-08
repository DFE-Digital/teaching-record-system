using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.NewPersonDetails);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.NewPersonDetails);
        base.Dispose();
    }

    [Fact]
    public async Task Get_PageLegend_PopulatedFromOriginalPersonName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great"));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("A", "New", "Name", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("edit-details-caption");
        Assert.Equal("Alfred The Great", caption!.TextContent);
    }

    [Fact]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Continue", b.TextContent),
            b => Assert.Equal("Cancel and return to record", b.TextContent));
    }

    [Fact]
    public async Task Get_InitializesJourneyStateWithPersonData()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("test@test.com")
            .WithMobileNumber("07891234567")
            .WithNationalInsuranceNumber("AB123456C"));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/edit-details?ffiid=", response.Headers.Location?.OriginalString);
        var redirectRequest = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location!.OriginalString);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        Assert.Equal(StatusCodes.Status200OK, (int)redirectResponse.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(redirectResponse);
        var firstName = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-first-name", "input");
        var middleName = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-middle-name", "input");
        var lastName = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-last-name", "input");
        var dateOfBirth = GetChildElementsOfTestId<IHtmlInputElement>(doc, "edit-details-date-of-birth", "input");
        var emailAddress = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-email-address", "input");
        var mobileNumber = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-mobile-number", "input");
        var nationalInsuranceNumber = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-national-insurance-number", "input");

        Assert.Equal("Alfred", firstName.Value);
        Assert.Equal("The", middleName.Value);
        Assert.Equal("Great", lastName.Value);
        Assert.Collection(dateOfBirth,
            day => Assert.Equal("1", day.Value),
            month => Assert.Equal("2", month.Value),
            year => Assert.Equal("1980", year.Value));
        Assert.Equal("test@test.com", emailAddress.Value);
        Assert.Equal("AB 12 34 56 C", nationalInsuranceNumber.Value);
        Assert.Equal("07891234567", mobileNumber.Value);
    }

    [Fact]
    public async Task Get_PopulatesFieldsFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var firstName = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-first-name", "input");
        var middleName = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-middle-name", "input");
        var lastName = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-last-name", "input");
        var dateOfBirth = GetChildElementsOfTestId<IHtmlInputElement>(doc, "edit-details-date-of-birth", "input");
        var emailAddress = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-email-address", "input");
        var mobileNumber = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-mobile-number", "input");
        var nationalInsuranceNumber = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-national-insurance-number", "input");

        Assert.Equal("Alfred", firstName.Value);
        Assert.Equal("The", middleName.Value);
        Assert.Equal("Great", lastName.Value);
        Assert.Collection(dateOfBirth,
            day => Assert.Equal("1", day.Value),
            month => Assert.Equal("2", month.Value),
            year => Assert.Equal("1980", year.Value));
        Assert.Equal("test@test.com", emailAddress.Value);
        Assert.Equal("07891234567", mobileNumber.Value);
        Assert.Equal("AB 12 34 56 C", nationalInsuranceNumber.Value);
    }

    [Fact]
    public async Task Post_FirstNameMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName(null)
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.FirstName), "Enter the person\u2019s first name");
    }

    [Fact]
    public async Task Post_FirstNameWhiteSpace_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("   ")
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.FirstName), "Enter the person\u2019s first name");
    }

    [Fact]
    public async Task Post_FirstNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName(string.Join("", Enumerable.Repeat('x', 101)))
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.FirstName), "Person\u2019s first name must be 100 characters or less");
    }

    [Fact]
    public async Task Post_MiddleNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName(string.Join("", Enumerable.Repeat('x', 101)))
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.MiddleName), "Person\u2019s middle name must be 100 characters or less");
    }

    [Fact]
    public async Task Post_LastNameMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName(null)
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.LastName), "Enter the person\u2019s last name");
    }

    [Fact]
    public async Task Post_LastNameWhiteSpace_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName("    ")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.LastName), "Enter the person\u2019s last name");
    }

    [Fact]
    public async Task Post_LastNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName(string.Join("", Enumerable.Repeat('x', 101)))
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.LastName), "Person\u2019s last name must be 100 characters or less");
    }

    [Fact]
    public async Task Post_DateOfBirthMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(null)
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.DateOfBirth), "Enter the person\u2019s date of birth");
    }

    [Fact]
    public async Task Post_DateOfBirthInTheFuture_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 2030"))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.DateOfBirth), "Person\u2019s date of birth must be in the past");
    }

    [Fact]
    public async Task Post_EmailAddressInvalid_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .WithEmailAddress("XYZ")
                    .WithMobileNumber("07891 234567")
                    .WithNationalInsuranceNumber("AB 12 34 56 C")
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.EmailAddress), "Enter a valid email address");
    }

    [Fact]
    public async Task Post_MobileNumberInvalid_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .WithNationalInsuranceNumber("AB 12 34 56 C")
                    .WithMobileNumber("XYZ")
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.MobileNumber), "Enter a valid UK or international mobile phone number");
    }

    [Fact]
    public async Task Post_NationalInsuranceNumberInvalid_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithMiddleName("The")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .WithNationalInsuranceNumber("XYZ")
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
    }

    [Fact]
    public async Task Post_DetailsChanged_PersistsDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new FormUrlEncodedContent(
                new EditDetailsPostRequestBuilder()
                    .WithFirstName("Jimmy")
                    .WithMiddleName("A")
                    .WithLastName("Person")
                    .WithDateOfBirth(DateOnly.Parse("2 Mar 1981"))
                    .WithEmailAddress("new@email.com")
                    .WithMobileNumber("07987 654321")
                    .WithNationalInsuranceNumber("AB 65 43 21 D")
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal("Jimmy", journeyInstance.State.FirstName);
        Assert.Equal("A", journeyInstance.State.MiddleName);
        Assert.Equal("Person", journeyInstance.State.LastName);
        Assert.Equal(DateOnly.Parse("2 Mar 1981"), journeyInstance.State.DateOfBirth);
        Assert.Equal("new@email.com", journeyInstance.State.EmailAddress?.ToString());
        Assert.Equal("447987654321", journeyInstance.State.MobileNumber?.ToString());
        Assert.Equal("AB654321D", journeyInstance.State.NationalInsuranceNumber?.ToString());
    }

    private string GetRequestPath(TestData.CreatePersonResult person) =>
        $"/persons/{person.PersonId}/edit-details";

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
