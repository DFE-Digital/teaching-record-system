using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class PersonalEmailTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_RequestForEmailWithOpenTasks_RedirectsToEmailInUse()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);
        var person = await TestData.CreatePersonAsync();

        await TestData.CreateNpqTrnRequestSupportTaskAsync(ApplicationUser.NPQApplicationUserGuid, configure => configure
            .WithEmailAddress(email)
            .WithStatus(SupportTaskStatus.Open));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "PersonalEmail", email }
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/emailinuse?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);
    }

    [Fact]
    public async Task Post_EmptyPersonalEmailAddressEntered_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "PersonalEmail", "Enter your personal email address");
    }

    [Fact]
    public async Task Post_InvalidFormatPersonalEmailAddress_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);
        var invalidEmail = "invalid-email";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "PersonalEmail", invalidEmail }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "PersonalEmail", "Enter an email address in the correct format, like name@example.com");
    }

    [Fact]
    public async Task Post_ValidRequestWithValidData_RedirectsToNamePage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var email = Faker.Internet.Email();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "PersonalEmail", email }
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ValidRequestWithValidDataAndFromCheckAnswersTrue_RedirectsToNamePage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var email = Faker.Internet.Email();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}&FromCheckAnswers=true")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "PersonalEmail", email }
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }
}
