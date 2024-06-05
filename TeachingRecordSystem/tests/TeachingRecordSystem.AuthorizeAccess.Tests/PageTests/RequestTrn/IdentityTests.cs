namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class IdentityTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_DateOfBirthMissingFromState_RedirectsToDateOfBirth()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
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
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{state.EvidenceFileName} ({state.EvidenceFileSizeDescription})", uploadedEvidenceLink!.TextContent);
    }

    [Fact]
    public async Task Post_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_DateOfBirthMissingFromState_RedirectsToDateOfBirth()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var multipartContent = CreateFormFileUpload(".png");
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoEvidenceFileIsSelected_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = false;
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = false;
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        var journeyInstance = await CreateJourneyInstance(state);

        var multipartContent = CreateFormFileUpload(".cs");
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a PDF, JPG, JPEG or PNG");
    }


    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToNextPage()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.HasPreviousName = false;
        state.DateOfBirth = new DateOnly(1980, 3, 1);
        var journeyInstance = await CreateJourneyInstance(state);

        var multipartContent = CreateFormFileUpload(".png");
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.NotNull(reloadedJourneyInstance.State.EvidenceFileId);
        Assert.Equal("evidence.png", reloadedJourneyInstance.State.EvidenceFileName);
    }

    private MultipartFormDataContent CreateFormFileUpload(string fileExtension)
    {
        var byteArrayContent = new ByteArrayContent(new byte[] { });
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");

        var multipartContent = new MultipartFormDataContent
        {
            { byteArrayContent, "EvidenceFile", $"evidence{fileExtension}" }
        };

        return multipartContent;
    }
}
