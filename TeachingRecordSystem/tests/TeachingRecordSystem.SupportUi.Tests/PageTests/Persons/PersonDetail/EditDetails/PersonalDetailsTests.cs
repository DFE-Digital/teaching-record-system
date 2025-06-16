using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

[Collection(nameof(DisableParallelization))]
public class PersonalDetailsTests : TestBase
{
    public PersonalDetailsTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
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
                .WithInitializedState(person)
                .WithName("A", "New", "Name")
                .Build());

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
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
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

        Assert.Equal("Alfred", firstName.Value.Trim());
        Assert.Equal("The", middleName.Value.Trim());
        Assert.Equal("Great", lastName.Value.Trim());
        Assert.Collection(dateOfBirth,
            day => Assert.Equal("1", day.Value.Trim()),
            month => Assert.Equal("2", month.Value.Trim()),
            year => Assert.Equal("1980", year.Value.Trim()));
        Assert.Equal("test@test.com", emailAddress.Value.Trim());
        Assert.Equal("07891234567", mobileNumber.Value.Trim());
        Assert.Equal("AB 12 34 56 C", nationalInsuranceNumber.Value.Trim());
    }

    [Fact]
    public async Task Get_PersonDataIsInvalid_JourneyInitializationDoesNotError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithMobileNumber("invalid")
            .WithEmail("invalid")
            .WithNationalInsuranceNumber("invalid"));

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
        var mobileNumber = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-mobile-number", "input");
        var emailAddress = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-email-address", "input");
        var nationalInsuranceNumber = GetChildElementOfTestId<IHtmlInputElement>(doc, "edit-details-national-insurance-number", "input");

        // Mobile number is normalized during TRS Sync process so invalid numbers are converted to null
        Assert.Equal("", mobileNumber.Value.Trim());
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
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmail("test@test.com")
                .WithMobileNumber("07891 234567")
                .WithNationalInsuranceNumber("AB 12 34 56 C")
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

        Assert.Equal("Alfred", firstName.Value.Trim());
        Assert.Equal("The", middleName.Value.Trim());
        Assert.Equal("Great", lastName.Value.Trim());
        Assert.Collection(dateOfBirth,
            day => Assert.Equal("1", day.Value.Trim()),
            month => Assert.Equal("2", month.Value.Trim()),
            year => Assert.Equal("1980", year.Value.Trim()));
        Assert.Equal("test@test.com", emailAddress.Value.Trim());
        Assert.Equal("07891234567", mobileNumber.Value.Trim());
        Assert.Equal("AB 12 34 56 C", nationalInsuranceNumber.Value.Trim());
    }

    [Fact]
    public async Task Post_FirstNameMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.FirstName), "Enter the person\u2019s first name");
    }

    [Fact]
    public async Task Post_FirstNameWhiteSpace_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.FirstName), "Enter the person\u2019s first name");
    }

    [Fact]
    public async Task Post_FirstNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.FirstName), "Person\u2019s first name must be 100 characters or less");
    }

    [Fact]
    public async Task Post_MiddleNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.MiddleName), "Person\u2019s middle name must be 100 characters or less");
    }

    [Fact]
    public async Task Post_LastNameMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.LastName), "Enter the person\u2019s last name");
    }

    [Fact]
    public async Task Post_LastNameWhiteSpace_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.LastName), "Enter the person\u2019s last name");
    }

    [Fact]
    public async Task Post_LastNameMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.LastName), "Person\u2019s last name must be 100 characters or less");
    }

    [Fact]
    public async Task Post_DateOfBirthMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.DateOfBirth), "Enter the person\u2019s date of birth");
    }

    [Fact]
    public async Task Post_DateOfBirthInTheFuture_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.DateOfBirth), "Person\u2019s date of birth must be in the past");
    }

    [Fact]
    public async Task Post_EmailAddressMoreThan100Characters_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.EmailAddress), "Person\u2019s email address must be 100 characters or less");
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
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.EmailAddress), "Enter a valid email address");
        }
    }

    [Theory]
    [InlineData("test", false)]
    [InlineData("07891 234567", true)]
    [InlineData("08891 234567", false)]
    [InlineData("44 7891 234567", true)]
    [InlineData("44 6891 234567", false)]
    [InlineData("37 9891 234567", false)]
    [InlineData("20 12345", false)]
    [InlineData("20 123456", true)]
    [InlineData("20 1234567891234", true)]
    [InlineData("20 12345678912345", false)]
    // Ignore whitespace and symbols
    [InlineData("(07891) 234567", true)]
    [InlineData("+44 78-91-23-45-67", true)]
    [InlineData("   (078 91) 234 567   ", true)]
    [InlineData("07891234567", true)]
    [InlineData("447891234567", true)]
    public async Task Post_ValidatesMobileNumber_ShowsPageErrorIfInvalid(string mobileNumber, bool shouldBeValid)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithMobileNumber(mobileNumber)
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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.MobileNumber), "Enter a valid UK or international mobile phone number");
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
    // The characters D, F, I, Q, U, and V are not used as either the first or second letter of a NINO prefix.
    [InlineData("DA 12 34 56 A", false)]
    [InlineData("FA 12 34 56 A", false)]
    [InlineData("IA 12 34 56 A", false)]
    [InlineData("QA 12 34 56 A", false)]
    [InlineData("UA 12 34 56 A", false)]
    [InlineData("VA 12 34 56 A", false)]
    [InlineData("AD 12 34 56 A", false)]
    [InlineData("AF 12 34 56 A", false)]
    [InlineData("AI 12 34 56 A", false)]
    [InlineData("AQ 12 34 56 A", false)]
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
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
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
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(PersonalDetailsModel.NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }
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
            .WithEmail("test@test.com")
            .WithMobileNumber("447891234567")
            .WithNationalInsuranceNumber("AB123456C"));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .Build());

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
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmail("test@test.com")
                .WithMobileNumber("07891 234567")
                .WithNationalInsuranceNumber("AB 12 34 56 C")
                .Build());

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
        Assert.Equal("447987654321", journeyInstance.State.MobileNumber.Parsed?.ToString());
        Assert.Equal("AB654321D", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
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
            .WithEmail("original@email.com")
            .WithMobileNumber("447891234567")
            .WithNationalInsuranceNumber("AB123456C"));

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Megan", "Thee", "Stallion")
                .WithDateOfBirth(DateOnly.Parse("3 Aug 1999"))
                .WithEmail("new@email.com")
                .WithMobileNumber("447987654321")
                .WithNationalInsuranceNumber("AB654321D")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership)
                .WithNameChangeUploadEvidenceChoice(false, nameEvidenceFileId, "name-evidence.pdf", "2.4 MB")
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason")
                .WithOtherDetailsChangeUploadEvidenceChoice(true, otherEvidenceFileId, "other-evidence.png", "1.3 KB")
                .Build());

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
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);

        // Assert
        Assert.Equal("Alfred", journeyInstance.State.FirstName);
        Assert.Equal("The", journeyInstance.State.MiddleName);
        Assert.Equal("Great", journeyInstance.State.LastName);
        Assert.Null(journeyInstance.State.NameChangeReason);
        Assert.Null(journeyInstance.State.NameChangeUploadEvidence);
        Assert.Null(journeyInstance.State.NameChangeEvidenceFileId);
        Assert.Null(journeyInstance.State.NameChangeEvidenceFileName);
        Assert.Null(journeyInstance.State.NameChangeEvidenceFileSizeDescription);

        Assert.Equal(DateOnly.Parse("3 Aug 1999"), journeyInstance.State.DateOfBirth);
        Assert.Equal("new@email.com", journeyInstance.State.EmailAddress.Parsed?.ToString());
        Assert.Equal("447987654321", journeyInstance.State.MobileNumber.Parsed?.ToString());
        Assert.Equal("AB654321D", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
        Assert.Equal(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, journeyInstance.State.OtherDetailsChangeReason);
        Assert.Equal("Some reason", journeyInstance.State.OtherDetailsChangeReasonDetail);
        Assert.Equal(true, journeyInstance.State.OtherDetailsChangeUploadEvidence);
        Assert.Equal(otherEvidenceFileId, journeyInstance.State.OtherDetailsChangeEvidenceFileId);
        Assert.Equal("other-evidence.png", journeyInstance.State.OtherDetailsChangeEvidenceFileName);
        Assert.Equal("1.3 KB", journeyInstance.State.OtherDetailsChangeEvidenceFileSizeDescription);
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
            .WithEmail("original@email.com")
            .WithMobileNumber("447891234567")
            .WithNationalInsuranceNumber("AB123456C"));

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Megan", "Thee", "Stallion")
                .WithDateOfBirth(DateOnly.Parse("3 Aug 1999"))
                .WithEmail("new@email.com")
                .WithMobileNumber("447987654321")
                .WithNationalInsuranceNumber("AB654321D")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership)
                .WithNameChangeUploadEvidenceChoice(true, nameEvidenceFileId, "name-evidence.pdf", "2.4 MB")
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, "Some reason")
                .WithOtherDetailsChangeUploadEvidenceChoice(true, otherEvidenceFileId, "other-evidence.png", "1.3 KB")
                .Build());

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
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);

        // Assert
        Assert.Equal("Megan", journeyInstance.State.FirstName);
        Assert.Equal("Thee", journeyInstance.State.MiddleName);
        Assert.Equal("Stallion", journeyInstance.State.LastName);
        Assert.Equal(EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership, journeyInstance.State.NameChangeReason);
        Assert.Equal(true, journeyInstance.State.NameChangeUploadEvidence);
        Assert.Equal(nameEvidenceFileId, journeyInstance.State.NameChangeEvidenceFileId);
        Assert.Equal("name-evidence.pdf", journeyInstance.State.NameChangeEvidenceFileName);
        Assert.Equal("2.4 MB", journeyInstance.State.NameChangeEvidenceFileSizeDescription);

        Assert.Equal(DateOnly.Parse("1 Feb 1980"), journeyInstance.State.DateOfBirth);
        Assert.Equal("original@email.com", journeyInstance.State.EmailAddress.Parsed?.ToString());
        Assert.Equal("447891234567", journeyInstance.State.MobileNumber.Parsed?.ToString());
        Assert.Equal("AB123456C", journeyInstance.State.NationalInsuranceNumber.Parsed?.ToString());
        Assert.Null(journeyInstance.State.OtherDetailsChangeReason);
        Assert.Null(journeyInstance.State.OtherDetailsChangeReasonDetail);
        Assert.Null(journeyInstance.State.OtherDetailsChangeUploadEvidence);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidenceFileId);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidenceFileName);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidenceFileSizeDescription);
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
