using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = SupportTask.GenerateSupportTaskReference();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTaskReference}/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync(SupportTaskStatus.Closed);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = SupportTask.GenerateSupportTaskReference();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTaskReference}/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync(SupportTaskStatus.Closed);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_ClosesTaskResolvesTrnRequestAndRedirectsToListPage()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/manual-checks-needed/{supportTask.SupportTaskReference}/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/manual-checks-needed", response.Headers.Location?.ToString());

        var updatedSupportTask = await WithDbContext(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(TrnRequestStatus.Completed, updatedSupportTask.TrnRequestMetadata!.Status);
    }

    private async Task<SupportTask> CreateSupportTaskAsync(SupportTaskStatus status = SupportTaskStatus.Open)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEmail(TestData.GenerateUniqueEmail()).WithAlert().WithQts().WithEyts());

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var apiSupportTask = await TestData.CreateResolvedApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            matchedPerson.Person,
            t => t.WithTrnRequestStatus(TrnRequestStatus.Pending));

        return await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
            apiSupportTask.TrnRequestMetadata!.ApplicationUserId,
            apiSupportTask.TrnRequestMetadata.RequestId,
            status);
    }
}
