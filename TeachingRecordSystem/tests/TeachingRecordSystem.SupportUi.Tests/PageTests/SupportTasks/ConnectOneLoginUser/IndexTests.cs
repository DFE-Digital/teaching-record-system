using ConnectOneLoginUserIndexModel = TeachingRecordSystem.SupportUi.Pages.SupportTasks.ConnectOneLoginUser.IndexModel;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ConnectOneLoginUser;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/TRS-000");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    [Skip("Waiting for another support task type")]
    public Task Get_SupportTaskIsNotConnectOneLoginUserType_ReturnsNotFound()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task Get_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContext(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var statedTrn = person.Trn;
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject, statedNationalInsuranceNumber: statedNationalInsuranceNumber, statedTrn: statedTrn);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(oneLoginUser.Email, doc.GetSummaryListValueForKey("Email address"));
        Assert.Equal($"{person.FirstName} {person.LastName}", doc.GetSummaryListValueForKey("Name"));
        Assert.Equal(person.DateOfBirth.ToString("dd/MM/yyyy"), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(statedNationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(statedTrn, doc.GetSummaryListValueForKey("TRN"));
    }

    [Test]
    public async Task Post_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/TRS-000")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    [Skip("Waiting for another support task type")]
    public Task Post_SupportTaskIsNotConnectOneLoginUserType_ReturnsNotFound()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task Post_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContext(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", person.Trn! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_NoSuggestionChosen_RendersError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Select the record you wish to connect");
    }

    [Test]
    public async Task Post_ValidRequestFromSuggestion_RedirectsToConnectPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", person.Trn! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn={person.Trn!}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_ValidRequestWithOverridenTrn_RedirectsToConnectPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", ConnectOneLoginUserIndexModel.NoneOfTheAboveTrnSentinel },
                { "TrnOverride", person.Trn! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn={person.Trn!}", response.Headers.Location?.OriginalString);
    }
}
