using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Create;

public class PersonalDetailsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel", b.TrimmedText()));
    }

    [Test]
    public async Task Get_PopulatesFieldsFromJourneyState()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmail("test@test.com")
                .WithNationalInsuranceNumber("AB 12 34 56 C")
                .WithGender(Gender.Female)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var firstName = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-first-name", "input");
        var middleName = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-middle-name", "input");
        var lastName = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-last-name", "input");
        var dateOfBirth = doc.GetChildElementsOfTestId<IHtmlInputElement>("edit-details-date-of-birth", "input");
        var emailAddress = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-email-address", "input");
        var nationalInsuranceNumber = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-national-insurance-number", "input");
        var genderSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("edit-details-gender-options", "input[type='radio']")
            .Single(i => i.IsChecked);

        Assert.Equal("Alfred", firstName.Value.Trim());
        Assert.Equal("The", middleName.Value.Trim());
        Assert.Equal("Great", lastName.Value.Trim());
        Assert.Collection(dateOfBirth,
            day => Assert.Equal("1", day.Value.Trim()),
            month => Assert.Equal("2", month.Value.Trim()),
            year => Assert.Equal("1980", year.Value.Trim()));
        Assert.Equal("test@test.com", emailAddress.Value.Trim());
        Assert.Equal("AB 12 34 56 C", nationalInsuranceNumber.Value.Trim());
        Assert.Equal("Female", genderSelection.Value.Trim());
    }

    [Test]
    public async Task Get_GenderOptionsAsExpected()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var genderOptions = doc.GetChildElementsOfTestId<IHtmlInputElement>("edit-details-gender-options", "input[type='radio']")
            .Select(radio => radio.NextElementSibling!.TrimmedText());

        Assert.Equal(genderOptions, ["Male", "Female", "Other", "Not provided"]);
    }

    [Test]
    public async Task Post_FirstNameMissing_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName(null)
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.FirstName), "Enter the person\u2019s first name");
    }

    [Test]
    public async Task Post_FirstNameWhiteSpace_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("   ")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.FirstName), "Enter the person\u2019s first name");
    }

    [Test]
    public async Task Post_FirstNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName(string.Join("", Enumerable.Repeat('x', 101)))
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.FirstName), "Person\u2019s first name must be 100 characters or less");
    }

    [Test]
    public async Task Post_MiddleNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName(string.Join("", Enumerable.Repeat('x', 101)))
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.MiddleName), "Person\u2019s middle name must be 100 characters or less");
    }

    [Test]
    public async Task Post_LastNameMissing_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName(null)
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.LastName), "Enter the person\u2019s last name");
    }

    [Test]
    public async Task Post_LastNameWhiteSpace_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("    ")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.LastName), "Enter the person\u2019s last name");
    }

    [Test]
    public async Task Post_LastNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName(string.Join("", Enumerable.Repeat('x', 101)))
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.LastName), "Person\u2019s last name must be 100 characters or less");
    }

    [Test]
    public async Task Post_DateOfBirthMissing_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.DateOfBirth), "Enter the person\u2019s date of birth");
    }

    [Test]
    public async Task Post_DateOfBirthInTheFuture_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 2030"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.DateOfBirth), "Person\u2019s date of birth must be in the past");
    }

    [Test]
    public async Task Post_EmailAddressMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithEmailAddress($"test@{string.Join(".", Enumerable.Repeat("test", 20))}.com")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.EmailAddress), "Person\u2019s email address must be 100 characters or less");
    }

    [Test]
    [Arguments("test", false)]
    [Arguments("test.test.test", false)]
    [Arguments("test@test", false)]
    [Arguments("test@test.%test", false)]
    [Arguments("test@test..test", false)]
    [Arguments("test@test.t", false)]
    [Arguments("test@\"test\".test", false)]
    [Arguments("test.test.test.test@test.test.test.test", true)]
    [Arguments(".!#$%&'*+/=?^_`{|}~-@test.test", true)]
    // Ignore surrounding whitespace
    [Arguments("    test@test.test    ", true)]
    public async Task Post_ValidatesEmailAddress_ShowsPageErrorIfInvalid(string emailAddress, bool shouldBeValid)
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress(emailAddress)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        if (shouldBeValid)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.EmailAddress), "Enter a valid email address");
        }
    }

    [Test]
    // https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
    // A NINO is made up of 2 letters, 6 numbers and a final letter, which is always A, B, C, or D.
    [Arguments("test", false)]
    [Arguments("A 12 34 56 A", false)]
    [Arguments("AB 12 34 56 AB", false)]
    [Arguments("AB 12 34 5 A", false)]
    [Arguments("AB 12 34 56 7 A", false)]
    [Arguments("AB CD 34 56 A", false)]
    [Arguments("AB 12 34 56 A", true)]
    [Arguments("AB 12 34 56 B", true)]
    [Arguments("AB 12 34 56 C", true)]
    [Arguments("AB 12 34 56 D", true)]
    [Arguments("AB 12 34 56 E", false)]
    [Arguments("AB 12 34 56 X", false)]
    // 2025-07-03: F|M|U are allowed as postfixes to accomodate legacy data
    [Arguments("AB 12 34 56 F", true)]
    [Arguments("AB 12 34 56 M", true)]
    [Arguments("AB 12 34 56 U", true)]
    // The characters D, F, I, (Q), U, and V are not used as either the first or second letter of a NINO prefix.
    // 2025-07-03: Q is allowed as a prefix to accomodate legacy data
    [Arguments("DA 12 34 56 A", false)]
    [Arguments("FA 12 34 56 A", false)]
    [Arguments("IA 12 34 56 A", false)]
    [Arguments("QA 12 34 56 A", true)]
    [Arguments("UA 12 34 56 A", false)]
    [Arguments("VA 12 34 56 A", false)]
    [Arguments("AD 12 34 56 A", false)]
    [Arguments("AF 12 34 56 A", false)]
    [Arguments("AI 12 34 56 A", false)]
    [Arguments("AQ 12 34 56 A", true)]
    [Arguments("AU 12 34 56 A", false)]
    [Arguments("AV 12 34 56 A", false)]
    // The letter O is not used as the second letter of a prefix.
    [Arguments("AO 12 34 56 A", false)]
    // Prefixes BG, GB, KN, NK, NT, TN and ZZ are not to be used
    [Arguments("BG 12 34 56 A", false)]
    [Arguments("GB 12 34 56 A", false)]
    [Arguments("KN 12 34 56 A", false)]
    [Arguments("NK 12 34 56 A", false)]
    [Arguments("NT 12 34 56 A", false)]
    [Arguments("TN 12 34 56 A", false)]
    [Arguments("ZZ 12 34 56 A", false)]
    // Ignore whitespace
    [Arguments("  AB   12   34  56    D    ", true)]
    [Arguments("AB123456D", true)]
    public async Task Post_ValidatesNationalInsuranceNumber_ShowsPageErrorIfInvalid(string niNumber, bool shouldBeValid)
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithNationalInsuranceNumber(niNumber)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        if (shouldBeValid)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }
    }

    [Test]
    // https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
    // It is sometimes necessary to use a Temporary Reference Number (TRN) for Individuals. The format of a TRN is 11 a1 11 11
    [Arguments("test", false)]
    [Arguments("1 A2 34 56", false)]
    [Arguments("12 BC 34 56", false)]
    [Arguments("12 D3 45 67", true)]
    // Ignore whitespace
    [Arguments("  98   Z 7  6  543    ", true)]
    [Arguments("45X67890", true)]
    public async Task Post_ValidatesNationalInsuranceNumber_AllowesTemporaryNino_ShowsPageErrorIfInvalid(string niNumber, bool shouldBeValid)
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithNationalInsuranceNumber(niNumber)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        if (shouldBeValid)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }
    }

    [Test]
    public async Task Post_GenderNotAvailable_ReturnsBadRequest()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithGender(Gender.NotAvailable)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_GenderNotProvided_Succeeds()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithGender(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance.State.Gender);
    }

    [Test]
    public async Task Post_PersistsDetails()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName("Jimmy")
                .WithMiddleName("A")
                .WithLastName("Person")
                .WithDateOfBirth(DateOnly.Parse("2 Mar 1981"))
                .WithEmailAddress("new@email.com")
                .WithMobileNumber("07987 654321")
                .WithNationalInsuranceNumber("AB 65 43 21 D")
                .WithGender(Gender.Other)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal("Jimmy", journeyInstance.State.FirstName);
        Assert.Equal("A", journeyInstance.State.MiddleName);
        Assert.Equal("Person", journeyInstance.State.LastName);
        Assert.Equal(DateOnly.Parse("2 Mar 1981"), journeyInstance.State.DateOfBirth);
        Assert.Equal("new@email.com", journeyInstance.State.EmailAddress.Parsed?.ToString());
        Assert.Equal("AB654321D", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
        Assert.Equal(Gender.Other, journeyInstance.State.Gender);
    }

    private string GetRequestPath() =>
        $"/persons/create/personal-details";

    private string GetRequestPath(JourneyInstance<CreateState> journeyInstance) =>
        $"/persons/create/personal-details?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<CreateState>> CreateJourneyInstanceAsync(CreateState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.CreatePerson,
            state ?? new CreateState());
}
