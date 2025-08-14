namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class EmailInUseTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutPersonalEmailAddress_RedirectsToEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.PersonalEmail = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/emailinuse?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonalEmailAddress_RendersExpectedContent()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var state = CreateNewState();
        state.PersonalEmail = email;
        var journeyInstance = await CreateJourneyInstance(state);
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateCrmTaskAsync(x =>
        {
            x.WithPersonId(person.ContactId);
            x.WithEmailAddress(email);
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/emailinuse?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
    }
}
