namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class NationalInsuranceNumberTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ProofOfIdentityMissingFromState_RedirectsToIdentity()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
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
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(state.NationalInsuranceNumber, doc.GetElementById("NationalInsuranceNumber")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ProofOfIdentityMissingFromState_RedirectsToIdentity()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenHasNationalInsuranceNumberHasNoSelection_ReturnsError()
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
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", Faker.Identification.UkNationalInsuranceNumber() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasNationalInsuranceNumber", "Select yes if you have a National Insurance number");
    }

    [Fact]
    public async Task Post_WhenHasNationalInsuranceNumberIsTrueAndNationalInsuranceNumberIsEmpty_ReturnsError()
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
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HasNationalInsuranceNumber", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "NationalInsuranceNumber", "Enter your National Insurance number");
    }

    [Fact]
    public async Task Post_WhenHasNationalInsuranceNumberIsTrueAndNationalInsuranceNumberIsInvalid_ReturnsError()
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
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HasNationalInsuranceNumber", "True" },
                { "NationalInsuranceNumber", "invalid-national-insurance-number" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "NationalInsuranceNumber", "Enter a National Insurance number in the correct format");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToNextPage(bool hasNationalInsuranceNumber)
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
        var journeyInstance = await CreateJourneyInstance(state);

        var content = new FormUrlEncodedContentBuilder();
        if (hasNationalInsuranceNumber)
        {
            content.Add("HasNationalInsuranceNumber", "True");
            content.Add("NationalInsuranceNumber", Faker.Identification.UkNationalInsuranceNumber());
        }
        else
        {
            content.Add("HasNationalInsuranceNumber", "False");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (hasNationalInsuranceNumber)
        {
            Assert.Equal($"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal($"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        }
    }
}
