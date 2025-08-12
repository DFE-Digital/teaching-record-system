namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasWorkEmailMissingFromState_RedirectsToWorkEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.WorkEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasPersonalEmailMissingFromState_RedirectsToPersonalEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.PersonalEmail = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.LastName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasHasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPreviousName = null;
        state.PreviousName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasDateOfBirthMissingFromState_RedirectsToDateOfBirth()
    {
        // Arrange
        var state = CreateNewState();
        state.DateOfBirth = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasEvidenceFileIdMissingFromState_RedirectsToIdentity()
    {
        // Arrange
        var state = CreateNewState();
        state.EvidenceFileId = null;
        state.EvidenceFileName = null;
        state.EvidenceFileSizeDescription = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasHasNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = null;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNationalInsuranceNumberTrueAndNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNationalInsuranceNumberFalseAndAddressLine1MissingFromState_RedirectsToAddress()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = false;
        state.NationalInsuranceNumber = null;
        state.AddressLine1 = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_PopulatesModelFromJourneyState(bool hasNationalInsuranceNumber)
    {
        // Arrange
        var state = CreateNewState();

        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;

        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = TestData.GenerateCountry();
            state.PostalCode = Faker.Address.ZipCode();
        }
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.WorkEmail, doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(state.PersonalEmail, doc.GetSummaryListValueForKey("Personal email"));
        Assert.Equal(StringHelper.JoinNonEmpty(' ', new string?[] { state.FirstName, state.MiddleName, state.LastName }), doc.GetSummaryListValueForKey("Name"));
        Assert.Equal(state.PreviousName, doc.GetSummaryListValueForKey("Previous name"));
        Assert.Equal(state.DateOfBirth?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(state.EvidenceFileName, doc.GetSummaryListValueForKey("Proof of identity"));
        if (hasNationalInsuranceNumber)
        {
            Assert.Equal(state.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        }
        else
        {
            Assert.Equal("None", doc.GetSummaryListValueForKey("National Insurance number"));
            var address = doc.GetSummaryListValueForKey("Address");
            Assert.Contains(state.AddressLine1!, address);
            Assert.Contains(state.AddressLine2!, address);
            Assert.Contains(state.TownOrCity!, address);
            Assert.Contains(state.PostalCode!, address);
            Assert.Contains(state.Country!, address);
        }
    }

    [Fact]
    public async Task Get_GetValidRequestWithNpqNameAndProvider_ShowsNpqNameAndProviderAndHidesNpqApplicationId()
    {
        // Arrange
        var npqApplicationId = default(string?);
        var npqName = "Some NPQ Name";
        var npqTrainingProvider = "NPQ TRAINING PROVIDER";
        var state = CreateNewState();
        state.NpqApplicationId = npqApplicationId;
        state.NpqName = npqName;
        state.NpqTrainingProvider = npqTrainingProvider;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Equal(npqName, doc.GetSummaryListValueForKey("NPQ"));
        Assert.Equal(npqTrainingProvider, doc.GetSummaryListValueForKey("NPQ provider"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_GetValidRequestNpqApplicationIdProvided_HidesNpqNameAndProvider()
    {
        // Arrange
        var npqApplicationId = "ApplicationId-12345";
        var state = CreateNewState();
        state.NpqApplicationId = npqApplicationId;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.NpqApplicationId, doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ Provider"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_GetValidRequestWorkEmailIsNull_HidesWorkEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.NpqApplicationId, doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Null(doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasWorkEmailMissingFromState_RedirectsToWorkEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasPersonalEmailMissingFromState_RedirectsToPersonalEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.PersonalEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.LastName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPreviousName = true;
        state.PreviousName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasDateOfBirthMissingFromState_RedirectsToDateOfBirth()
    {
        // Arrange
        var state = CreateNewState();
        state.DateOfBirth = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasEvidenceFileIdMissingFromState_RedirectsToIdentity()
    {
        // Arrange
        var state = CreateNewState();
        state.EvidenceFileId = null;
        state.EvidenceFileName = null;
        state.EvidenceFileSizeDescription = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasHasNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = null;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNationalInsuranceNumberTrueAndNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNationalInsuranceNumberFalseAndAddressLine1MissingFromState_RedirectsToAddress()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = false;
        state.NationalInsuranceNumber = null;
        state.AddressLine1 = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_ValidRequest_UpdatesStateAndAndRedirectsToSubmitted(bool hasNationalInsuranceNumber)
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = TestData.GenerateCountry();
            state.PostalCode = Faker.Address.ZipCode();
        }
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(reloadedJourneyInstance.State.HasPendingTrnRequest);
    }
}
