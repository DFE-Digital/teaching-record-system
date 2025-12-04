using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/TRS-000/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContextAsync(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");
        // Act
        var response = await HttpClient.SendAsync(request);
        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{requestData.StatedFirstName} {person.StatedLastName}", doc.GetSummaryListValueByKey("Name"));
        Assert.Equal(requestData.StatedDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(oneLoginUser.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(requestData.StatedTrn, doc.GetSummaryListValueByKey("TRN"));
        Assert.Equal(requestData.StatedNationalInsuranceNumber, doc.GetSummaryListValueByKey("National Insurance number"));

    }
}
