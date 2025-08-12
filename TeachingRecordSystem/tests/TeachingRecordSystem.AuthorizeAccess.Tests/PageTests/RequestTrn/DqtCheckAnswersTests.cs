namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class DqtCheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = null;
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = null;
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = null;
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = null;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = true;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = null;
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = "qq 12 34 56 56 c";

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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = null;
        state.EvidenceFileName = null;
        state.EvidenceFileSizeDescription = null;
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = "qq 12 34 56 56 c";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOMEID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = true;
        state.PreviousName = TestData.GenerateName();
        state.DateOfBirth = new DateOnly(1999, 01, 01);
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
    public async Task Get_GetValidRequestWithNpqNameAndProvider_ShowsNpqNameAndProviderAndHidesNpqApplicationId()
    {
        // Arrange
        var npqApplicationId = default(string?);
        var npqName = "Some NPQ Name";
        var npqTrainingProvider = "NPQ TRAINING PROVIDER";
        var state = CreateNewState();
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = false;
        state.NpqApplicationId = npqApplicationId;
        state.NpqName = npqName;
        state.NpqTrainingProvider = npqTrainingProvider;
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = "qq 12 34 56 56 c";
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Equal(state.WorkEmail, doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(state.PersonalEmail, doc.GetSummaryListValueForKey("Personal email"));
        Assert.Equal(state.Name, doc.GetSummaryListValueForKey("Name"));
        Assert.Null(doc.GetSummaryListValueForKey("Previous name"));
        Assert.Equal(state.DateOfBirth?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(state.EvidenceFileName, doc.GetSummaryListValueForKey("Proof of identity"));
        Assert.Equal(state.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = npqApplicationId;
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = "qq 12 34 56 56 c";
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.NpqApplicationId, doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Equal(state.WorkEmail, doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(state.PersonalEmail, doc.GetSummaryListValueForKey("Personal email"));
        Assert.Equal(state.Name, doc.GetSummaryListValueForKey("Name"));
        Assert.Null(doc.GetSummaryListValueForKey("Previous name"));
        Assert.Equal(state.DateOfBirth?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(state.EvidenceFileName, doc.GetSummaryListValueForKey("Proof of identity"));
        Assert.Equal(state.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ Provider"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_GetValidRequestWorkingInSchoolOrEducationalSettingFalse_HidesWorkEmail()
    {
        // Arrange
        var npqApplicationId = "ApplicationId-12345";
        var state = CreateNewState();
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = npqApplicationId;
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = null;
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.NpqApplicationId, doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Null(doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(state.PersonalEmail, doc.GetSummaryListValueForKey("Personal email"));
        Assert.Equal(state.Name, doc.GetSummaryListValueForKey("Name"));
        Assert.Null(doc.GetSummaryListValueForKey("Previous name"));
        Assert.Equal(state.DateOfBirth?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(state.EvidenceFileName, doc.GetSummaryListValueForKey("Proof of identity"));
        Assert.Equal(state.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ Provider"));
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = null;
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "Filename.jpg";
        state.EvidenceFileSizeDescription = "1.1 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = null;
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = null;
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = null;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1999, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = true;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "Filename.jpg";
        state.EvidenceFileSizeDescription = "1.1 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = null;
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "evidence-file-name.jpg";
        state.EvidenceFileSizeDescription = "1.2 MB";
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = null;
        state.EvidenceFileName = null;
        state.EvidenceFileSizeDescription = null;
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "Filename.jpg";
        state.EvidenceFileSizeDescription = "1.1 MB";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "Filename.jpg";
        state.EvidenceFileSizeDescription = "1.1 MB";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "Filename.jpg";
        state.EvidenceFileSizeDescription = "1.1 MB";
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
        state.IsTakingNpq = true;
        state.HaveRegisteredForAnNpq = true;
        state.NpqApplicationId = "SOME ID";
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = Faker.Internet.Email();
        state.PersonalEmail = Faker.Internet.Email();
        state.Name = TestData.GenerateName();
        state.HasPreviousName = false;
        state.PreviousName = null;
        state.DateOfBirth = new DateOnly(1998, 01, 01);
        state.EvidenceFileId = Guid.NewGuid();
        state.EvidenceFileName = "Filename.jpg";
        state.EvidenceFileSizeDescription = "1.1 MB";
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
