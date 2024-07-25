namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class EmailInUseTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutEmailAddress_RedirectsToEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/emailinuse?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithEmailAddress_RendersExpectedContent()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var state = CreateNewState(email);
        var journeyInstance = await CreateJourneyInstance(state);
        var person = await TestData.CreatePerson();
        await TestData.CreateCrmTask(x =>
        {
            x.WithPersonId(person.ContactId);
            x.WithEmailAddress(email);
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/emailinuse?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
    }
}
