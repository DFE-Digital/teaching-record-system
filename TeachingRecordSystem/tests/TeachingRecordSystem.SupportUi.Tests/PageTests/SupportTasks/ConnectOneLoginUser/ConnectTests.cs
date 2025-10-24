using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ConnectOneLoginUser;

public class ConnectTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/TRS-000/connect?trn=0000000");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn={person.Trn}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_TrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn=0000000");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequest_ReturnsExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn={person.Trn}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(oneLoginUser.Email, doc.GetSummaryListValueByKey("Email address"));
    }

    [Test]
    public async Task Post_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/TRS-000/connect?trn=0000000");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn={person.Trn}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_TrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn=0000000");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_ValidRequest_ClosesTaskConnectsUserToPersonAndRedirectsToSupportTaskList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect?trn={person.Trn}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks", response.Headers.Location?.OriginalString);

        await WithDbContext(async dbContext =>
        {
            supportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, supportTask.Status);
            var data = (ConnectOneLoginUserData)supportTask.Data;
            Assert.Equal(person.PersonId, data.PersonId);

            oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject);
            Assert.Equal(person.PersonId, oneLoginUser.PersonId);
            Assert.Equal(OneLoginUserMatchRoute.Support, oneLoginUser.MatchRoute);
            Assert.NotEmpty(oneLoginUser.MatchedAttributes!);
        });
    }
}
