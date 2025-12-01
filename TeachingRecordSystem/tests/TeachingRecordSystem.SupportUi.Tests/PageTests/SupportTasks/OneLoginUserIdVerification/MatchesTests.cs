using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class MatchesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private OneLoginUser[]? OneLoginUsers { get; set; }
    private SupportTask[]? SupportTasks { get; set; }

    private async Task CreateSupportTasksWithOneLoginUsersAsync()
    {
        OneLoginUsers = new OneLoginUser[]
        {
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null),
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null)
        };

        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[0].Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[1].Subject);

        SupportTasks = new SupportTask[]
        {
            supportTask1,
            supportTask2
        };
    }

    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{taskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();
        var supportTask = SupportTasks![0];
        supportTask.Status = SupportTaskStatus.Closed;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();
        var supportTask = SupportTasks![0];
        var oneLoginRequestData = supportTask.Data as OneLoginUserIdVerificationData;
        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', oneLoginRequestData!.StatedFirstName, oneLoginRequestData.StatedLastName), requestDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(oneLoginRequestData.StatedDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(OneLoginUsers![0].EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(oneLoginRequestData.StatedNationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(oneLoginRequestData.StatedTrn, requestDetails.GetSummaryListValueByKey("TRN"));
    }

    //[Fact]
    //public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    //{
    //    // Arrange
    //    await CreateSupportTasksWithOneLoginUsersAsync();
    //    var supportTask = SupportTasks![0];
    //    var oneLoginRequestData = supportTask.Data as OneLoginUserIdVerificationData;
    //    var journeyInstance = await CreateJourneyInstance(supportTask);

    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await response.GetDocumentAsync();
    //    var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
    //    Assert.NotNull(firstMatchDetails);
    //    Assert.Equal(matchedPerson.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
    //    Assert.Equal(matchedPerson.MiddleName, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
    //    Assert.Equal(matchedPerson.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
    //    Assert.Equal(matchedPerson.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueByKey("Date of birth"));
    //    Assert.Equal(matchedPerson.EmailAddress, firstMatchDetails.GetSummaryListValueByKey("Email address"));
    //    Assert.Equal(matchedPerson.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueByKey("NI number"));
    //    Assert.Equal(matchedPerson.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueByKey("Gender"));
    //}

}
