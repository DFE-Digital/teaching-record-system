using System.Globalization;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using PersonDetailsUpdatedEventChanges = TeachingRecordSystem.Core.Events.Legacy.PersonDetailsUpdatedEventChanges;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class PersonalDetailsTests(HostFixture hostFixture) : EditDetailsTestBase(hostFixture)
{
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
            CreateState(person, s =>
            {
                s.FirstName = "A";
                s.MiddleName = "New";
                s.LastName = "Name";
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("edit-details-caption");
        Assert.Equal("Alfred The Great", caption!.TrimmedText());
    }

    [Fact]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
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
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Female));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/edit-details?_jid=", response.Headers.Location?.OriginalString);
        var redirectRequest = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location!.OriginalString);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        Assert.Equal(StatusCodes.Status200OK, (int)redirectResponse.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(redirectResponse);
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

    [Fact]
    public async Task Get_PersonDataIsInvalid_JourneyInitializationDoesNotError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress("invalid")
            .WithNationalInsuranceNumber("invalid"));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/edit-details?_jid=", response.Headers.Location?.OriginalString);
        var redirectRequest = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location!.OriginalString);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        Assert.Equal(StatusCodes.Status200OK, (int)redirectResponse.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(redirectResponse);
        var emailAddress = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-email-address", "input");
        var nationalInsuranceNumber = doc.GetChildElementOfTestId<IHtmlInputElement>("edit-details-national-insurance-number", "input");

        // Mobile number is normalized during TRS Sync process so invalid numbers are converted to null
        Assert.Equal("invalid", emailAddress.Value.Trim());
        Assert.Equal("invalid", nationalInsuranceNumber.Value.Trim());
    }

    [Fact]
    public async Task Get_PopulatesFieldsFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue("test@test.com");
                s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue("AB 12 34 56 C");
                s.Gender = Gender.Other;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

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
        Assert.Equal("Other", genderSelection.Value.Trim());
    }

    [Theory]
    [InlineData(Gender.Male, new string[] { "Male", "Female", "Other", "Not provided" })]
    [InlineData(Gender.Female, new string[] { "Male", "Female", "Other", "Not provided" })]
    [InlineData(Gender.Other, new string[] { "Male", "Female", "Other", "Not provided" })]
    [InlineData(null, new string[] { "Male", "Female", "Other", "Not provided" })]
    [InlineData(Gender.NotAvailable, new string[] { "Male", "Female", "Other", "Not available", "Not provided" })]
    public async Task Get_NotAvailableGender_OptionNotVisible_UnlessPreExistingValueOnPersonRecord(Gender? preExistingGenderValue, string[] expectedOptions)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.Gender = preExistingGenderValue;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var genderOptions = doc.GetChildElementsOfTestId<IHtmlInputElement>("edit-details-gender-options", "input[type='radio']");

        Action<IHtmlInputElement> AssertRadioButtonLabel(string expectedValue) =>
            radio => Assert.Equal(expectedValue, radio.NextElementSibling!.TrimmedText());

        Assert.Collection(genderOptions, expectedOptions.Select(AssertRadioButtonLabel).ToArray());
    }

    [Fact]
    public async Task Post_FirstNameMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName(null)
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("   ")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName(string.Join("", Enumerable.Repeat('x', 101)))
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName(string.Join("", Enumerable.Repeat('x', 101)))
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName(null)
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("    ")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName(string.Join("", Enumerable.Repeat('x', 101)))
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(null)
                .BuildFormUrlEncoded()
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
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 2030"))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.DateOfBirth), "Person\u2019s date of birth must be in the past");
    }

    [Fact]
    public async Task Post_EmailAddressMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.EmailAddress), "Person\u2019s email address must be 100 characters or less");
    }

    [Theory]
    [InlineData("test", false)]
    [InlineData("test.test.test", false)]
    [InlineData("test@test", false)]
    [InlineData("test@test.%test", false)]
    [InlineData("test@test..test", false)]
    [InlineData("test@test.t", false)]
    [InlineData("test@\"test\".test", false)]
    [InlineData("test.test.test.test@test.test.test.test", true)]
    [InlineData(".!#$%&'*+/=?^_`{|}~-@test.test", true)]
    // Ignore surrounding whitespace
    [InlineData("    test@test.test    ", true)]
    public async Task Post_ValidatesEmailAddress_ShowsPageErrorIfInvalid(string emailAddress, bool shouldBeValid)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.EmailAddress), "Enter a valid email address");
        }
    }

    [Theory]
    // https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
    // A NINO is made up of 2 letters, 6 numbers and a final letter, which is always A, B, C, or D.
    [InlineData("test", false)]
    [InlineData("A 12 34 56 A", false)]
    [InlineData("AB 12 34 56 AB", false)]
    [InlineData("AB 12 34 5 A", false)]
    [InlineData("AB 12 34 56 7 A", false)]
    [InlineData("AB CD 34 56 A", false)]
    [InlineData("AB 12 34 56 A", true)]
    [InlineData("AB 12 34 56 B", true)]
    [InlineData("AB 12 34 56 C", true)]
    [InlineData("AB 12 34 56 D", true)]
    [InlineData("AB 12 34 56 E", false)]
    [InlineData("AB 12 34 56 X", false)]
    // 2025-07-03: F|M|U are allowed as postfixes to accomodate legacy data
    [InlineData("AB 12 34 56 F", true)]
    [InlineData("AB 12 34 56 M", true)]
    [InlineData("AB 12 34 56 U", true)]
    // The characters D, F, I, (Q), U, and V are not used as either the first or second letter of a NINO prefix.
    // 2025-07-03: Q is allowed as a prefix to accomodate legacy data
    [InlineData("DA 12 34 56 A", false)]
    [InlineData("FA 12 34 56 A", false)]
    [InlineData("IA 12 34 56 A", false)]
    [InlineData("QA 12 34 56 A", true)]
    [InlineData("UA 12 34 56 A", false)]
    [InlineData("VA 12 34 56 A", false)]
    [InlineData("AD 12 34 56 A", false)]
    [InlineData("AF 12 34 56 A", false)]
    [InlineData("AI 12 34 56 A", false)]
    [InlineData("AQ 12 34 56 A", true)]
    [InlineData("AU 12 34 56 A", false)]
    [InlineData("AV 12 34 56 A", false)]
    // The letter O is not used as the second letter of a prefix.
    [InlineData("AO 12 34 56 A", false)]
    // Prefixes BG, GB, KN, NK, NT, TN and ZZ are not to be used
    [InlineData("BG 12 34 56 A", false)]
    [InlineData("GB 12 34 56 A", false)]
    [InlineData("KN 12 34 56 A", false)]
    [InlineData("NK 12 34 56 A", false)]
    [InlineData("NT 12 34 56 A", false)]
    [InlineData("TN 12 34 56 A", false)]
    [InlineData("ZZ 12 34 56 A", false)]
    // Ignore whitespace
    [InlineData("  AB   12   34  56    D    ", true)]
    [InlineData("AB123456D", true)]
    public async Task Post_ValidatesNationalInsuranceNumber_ShowsPageErrorIfInvalid(string niNumber, bool shouldBeValid)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }
    }

    [Theory]
    // https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
    // It is sometimes necessary to use a Temporary Reference Number (TRN) for Individuals. The format of a TRN is 11 a1 11 11
    [InlineData("test", false)]
    [InlineData("1 A2 34 56", false)]
    [InlineData("12 BC 34 56", false)]
    [InlineData("12 D3 45 67", true)]
    // Ignore whitespace
    [InlineData("  98   Z 7  6  543    ", true)]
    [InlineData("45X67890", true)]
    public async Task Post_ValidatesNationalInsuranceNumber_AllowesTemporaryNino_ShowsPageErrorIfInvalid(string niNumber, bool shouldBeValid)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(IndexModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }
    }

    [Fact]
    public async Task Post_UpdatingGenderToNotAvailable_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Male));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("test@test.com")
                .WithMobileNumber("447891234567")
                .WithNationalInsuranceNumber("AB123456C")
                .WithGender(Gender.NotAvailable)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_LeavingPreExistingNotAvailableGenderUnchanged_Succeeds()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.NotAvailable));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfrede")
                .WithMiddleName("Thee")
                .WithLastName("Greate") // Need to change some values so the validation doesn't fail
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("test@test.com")
                .WithMobileNumber("447891234567")
                .WithNationalInsuranceNumber("AB123456C")
                .WithGender(Gender.NotAvailable)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.Equal(Gender.NotAvailable, journeyInstance.State.Gender);
    }

    [Fact]
    public async Task Post_UpdatingGenderToNotProvided_Succeeds()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Male));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("test@test.com")
                .WithMobileNumber("447891234567")
                .WithNationalInsuranceNumber("AB123456C")
                .WithGender(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.Null(journeyInstance.State.Gender);
    }

    [Fact]
    public async Task Post_NoDetailsChanged_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Male));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("test@test.com")
                .WithMobileNumber("447891234567")
                .WithNationalInsuranceNumber("AB123456C")
                .WithGender(Gender.Male)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasSummaryErrorAsync(response, "Please change one or more of the person\u2019s details");
    }

    [Fact]
    public async Task Post_DetailsChanged_PersistsDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue("test@test.com");
                s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue("AB 12 34 56 C");
                s.Gender = Gender.Male;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
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
        Assert.Equal("Jimmy", journeyInstance.State.FirstName);
        Assert.Equal("A", journeyInstance.State.MiddleName);
        Assert.Equal("Person", journeyInstance.State.LastName);
        Assert.Equal(DateOnly.Parse("2 Mar 1981"), journeyInstance.State.DateOfBirth);
        Assert.Equal("new@email.com", journeyInstance.State.EmailAddress.Parsed?.ToString());
        Assert.Equal("AB654321D", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
        Assert.Equal(Gender.Other, journeyInstance.State.Gender);
    }

    [Fact]
    public async Task Post_WhenNamePreviouslyChangedInSameJourney_AndNameUpdatedToOriginalValue_RemovesNameChangeReasonFromState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("original@email.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Female));

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Megan";
                s.MiddleName = "Thee";
                s.LastName = "Stallion";
                s.DateOfBirth = DateOnly.Parse("3 Aug 1999");
                s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue("new@email.com");
                s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue("AB654321D");
                s.Gender = Gender.Other;
                s.NameChangeReason = PersonNameChangeReason.MarriageOrCivilPartnership;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false,
                    UploadedEvidenceFile = new()
                    {
                        FileId = nameEvidenceFileId,
                        FileName = "name-evidence.pdf",
                        FileSizeDescription = "2.4 MB"
                    }
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.AnotherReason;
                s.OtherDetailsChangeReasonDetail = "Some reason";
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = otherEvidenceFileId,
                        FileName = "other-evidence.png",
                        FileSizeDescription = "1.3 KB"
                    }
                };
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("3 Aug 1999"))
                .WithEmailAddress("new@email.com")
                .WithMobileNumber("447987654321")
                .WithNationalInsuranceNumber("AB654321D")
                .WithGender(Gender.Other)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal("Alfred", journeyInstance.State.FirstName);
        Assert.Equal("The", journeyInstance.State.MiddleName);
        Assert.Equal("Great", journeyInstance.State.LastName);
        Assert.Null(journeyInstance.State.NameChangeReason);
        Assert.Null(journeyInstance.State.NameChangeEvidence.UploadEvidence);
        Assert.Null(journeyInstance.State.NameChangeEvidence.UploadedEvidenceFile);

        Assert.Equal(DateOnly.Parse("3 Aug 1999"), journeyInstance.State.DateOfBirth);
        Assert.Equal("new@email.com", journeyInstance.State.EmailAddress.Parsed?.ToString());
        Assert.Equal("AB654321D", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
        Assert.Equal(Gender.Other, journeyInstance.State.Gender);
        Assert.Equal(PersonDetailsChangeReason.AnotherReason, journeyInstance.State.OtherDetailsChangeReason);
        Assert.Equal("Some reason", journeyInstance.State.OtherDetailsChangeReasonDetail);
        Assert.Equal(true, journeyInstance.State.OtherDetailsChangeEvidence.UploadEvidence);
        Assert.Equal(otherEvidenceFileId, journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile!.FileId);
        Assert.Equal("other-evidence.png", journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile.FileName);
        Assert.Equal("1.3 KB", journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile.FileSizeDescription);
    }

    [Fact]
    public async Task Post_WhenOtherDetailsPreviouslyChangedInSameJourney_AndOtherDetailsUpdatedToOriginalValues_RemovesOtherDetailsChangeReasonFromState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("original@email.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Other));

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Megan";
                s.MiddleName = "Thee";
                s.LastName = "Stallion";
                s.DateOfBirth = DateOnly.Parse("3 Aug 1999");
                s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue("new@email.com");
                s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue("AB654321D");
                s.Gender = Gender.Female;
                s.NameChangeReason = PersonNameChangeReason.MarriageOrCivilPartnership;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = nameEvidenceFileId,
                        FileName = "name-evidence.pdf",
                        FileSizeDescription = "2.4 MB"
                    }
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.AnotherReason;
                s.OtherDetailsChangeReasonDetail = "Some reason";
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = otherEvidenceFileId,
                        FileName = "other-evidence.png",
                        FileSizeDescription = "1.3 KB"
                    }
                };
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Megan")
                .WithMiddleName("Thee")
                .WithLastName("Stallion")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("original@email.com")
                .WithMobileNumber("447891234567")
                .WithNationalInsuranceNumber("AB123456C")
                .WithGender(Gender.Other)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal("Megan", journeyInstance.State.FirstName);
        Assert.Equal("Thee", journeyInstance.State.MiddleName);
        Assert.Equal("Stallion", journeyInstance.State.LastName);
        Assert.Equal(PersonNameChangeReason.MarriageOrCivilPartnership, journeyInstance.State.NameChangeReason);
        Assert.Equal(true, journeyInstance.State.NameChangeEvidence.UploadEvidence);
        Assert.Equal(nameEvidenceFileId, journeyInstance.State.NameChangeEvidence.UploadedEvidenceFile!.FileId);
        Assert.Equal("name-evidence.pdf", journeyInstance.State.NameChangeEvidence.UploadedEvidenceFile.FileName);
        Assert.Equal("2.4 MB", journeyInstance.State.NameChangeEvidence.UploadedEvidenceFile.FileSizeDescription);

        Assert.Equal(DateOnly.Parse("1 Feb 1980"), journeyInstance.State.DateOfBirth);
        Assert.Equal("original@email.com", journeyInstance.State.EmailAddress.Parsed?.ToString());
        Assert.Equal("AB123456C", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
        Assert.Null(journeyInstance.State.OtherDetailsChangeReason);
        Assert.Null(journeyInstance.State.OtherDetailsChangeReasonDetail);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidence.UploadEvidence);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile);
    }

    private string GetRequestPath(TestData.CreatePersonResult person) =>
        $"/persons/{person.PersonId}/edit-details";

    private string GetRequestPath(TestData.CreatePersonResult person, EditDetailsJourneyCoordinator journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}";

    public static TheoryData<string?, HttpMethod> UserDoesNotHavePermission_ReturnsForbiddenData =>
        new MatrixTheoryData<string?, HttpMethod>(
            [UserRoles.Viewer, null],
            TestHttpMethods.GetAndPost.SplitTestMethods().ToArray());

    [Theory]
    [MemberData(nameof(UserDoesNotHavePermission_ReturnsForbiddenData))]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(string? role, HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
            }));

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
            }));

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, "")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, "")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "")]
    public async Task Get_BacklinkContainsExpected(PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Male));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/{person.PersonId}{expectedBackPage}", backlink.Href);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.MiddleName, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.LastName, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.Gender, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason")]
    // Edit details (name changes only): redirects to change name reason page
    // Edit details (other details changes only): redirects to change reason page
    // Edit details (name and other details changes): redirects to change name reason page
    // Name change reason (name changes only): redirects to check answers page
    // Name change reason (name and other details changes): redirects to change reason page
    // Change reason (other details changes only): redirects to check answers page
    // Change reason (name and other details changes): redirects to check answers page
    public async Task Post_RedirectsToExpectedPage(PersonDetailsUpdatedEventChanges changes, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var content = new EditDetailsPostRequestContentBuilder()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailsPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var pageUrl = $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}", location);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.jpg",
                        FileSizeDescription = "1.2 KB"
                    }
                };
            }));

        var pageUrl = $"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    // Every page goes back to check answers page (even if a new reason page was added to the journey on this visit)
    // Check answers page goes back to the appropriate reason page
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToCheckAnswersPage(PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Female));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Other : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-details?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}{expectedBackPage}", backlink!.Href);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?returnUrl={0}&")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason?returnUrl={0}&")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?returnUrl={0}&")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason?returnUrl={0}&")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // No new changes: redirects to check answers page
    // Switches from name change to other details change (& vice versa): redirects to appropriate reason page
    // Adds name/other details change: redirects to appropriate reason page
    // Removes name/other details change: redirects to check answers page
    // Change name reason (whether original or subsequent name change): redirects to check answers page
    // Change reason (whether original or subsequent other details change): redirects to check answers page
    public async Task Post_WithReturnUrlToCheckAnswersPage_AndMoreChangesMade_RedirectsToExpectedPage(PersonDetailsUpdatedEventChanges originalChanges, PersonDetailsUpdatedEventChanges newChanges, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Female));

        var originalFirstName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var originalMiddleName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var originalLastName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var originalDateOfBirth = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var originalEmailAddress = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var originalNationalInsuranceNumber = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var originalGender = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Male : person.Gender;

        var newFirstName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var newMiddleName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var newLastName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var newDateOfBirth = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var newEmailAddress = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var newNationalInsuranceNumber = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var newGender = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Male : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = originalFirstName;
                        s.MiddleName = originalMiddleName;
                        s.LastName = originalLastName;
                        s.DateOfBirth = originalDateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(originalEmailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(originalNationalInsuranceNumber);
                        s.Gender = originalGender;
                    });

        if (originalChanges.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (originalChanges.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var content = new EditDetailsPostRequestContentBuilder()
            .WithFirstName(newFirstName)
            .WithMiddleName(newMiddleName)
            .WithLastName(newLastName)
            .WithDateOfBirth(newDateOfBirth)
            .WithEmailAddress(newEmailAddress)
            .WithNationalInsuranceNumber(newNationalInsuranceNumber)
            .WithGender(newGender);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-details?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{string.Format(CultureInfo.InvariantCulture, expectedNextPageUrl, Uri.EscapeDataString(checkAnswersUrl))}{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }
}
