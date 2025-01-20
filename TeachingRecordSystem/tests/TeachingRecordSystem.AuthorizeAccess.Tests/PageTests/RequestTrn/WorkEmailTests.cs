namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class WorkEmailTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var state = CreateNewState();
        state.WorkEmail = email;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(email, doc.GetElementById("WorkEmail")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_EmptyEmailAddressEntered_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "WorkEmail", "Enter your work email address");
    }

    [Fact]
    public async Task Post_InvalidFormatEmailAddress_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);
        var invalidEmail = "invalid-email";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "WorkEmail", invalidEmail }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "WorkEmail", "Enter an email address in the correct format, like name@example.com");
    }

    [Fact]
    public async Task Post_ValidRequestWithValidData_RedirectsToPersonalEmailPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var email = Faker.Internet.Email();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "WorkEmail", email }
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ValidRequestWithValidDataAndFromCheckAnswersTrue_RedirectsToCheckAnswers()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var email = Faker.Internet.Email();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}&FromCheckAnswers=true")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "WorkEmail", email }
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }
}
