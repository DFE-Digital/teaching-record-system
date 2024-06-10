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
    public async Task Get_HasEmailMissingFromState_RedirectsToEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = null;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = null;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
        state.HasNationalInsuranceNumber = null;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
        state.HasNationalInsuranceNumber = false;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = Faker.Address.Country();
            state.PostalCode = Faker.Address.ZipCode();
        }
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(state.Email, doc.GetSummaryListValueForKey("Email"));
        Assert.Equal(state.Name, doc.GetSummaryListValueForKey("Name"));
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
    public async Task Post_HasEmailMissingFromState_RedirectsToEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasHasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = null;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
        state.HasNationalInsuranceNumber = null;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = true;
        state.PreviousName = Faker.Name.FullName();
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
        state.HasNationalInsuranceNumber = false;
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
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = false;
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = Faker.Address.Country();
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
