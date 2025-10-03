using TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests;

public class DetailsTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Test]
    public async Task Get_ShowsExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));
        var metadata = supportTask.TrnRequestMetadata!;

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        doc.AssertRowContentMatches("Name", $"{metadata.FirstName} {metadata.MiddleName} {metadata.LastName}");
        doc.AssertRowContentMatches("Date of birth", metadata.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Working in school or educational setting", metadata.NpqWorkingInEducationalSetting.HasValue ? metadata.NpqWorkingInEducationalSetting.Value ? "Yes" : "No" : UiDefaults.EmptyDisplayContent);
        doc.AssertRowContentMatches("NPQ application ID", metadata.NpqApplicationId);
        doc.AssertRowContentMatches("NPQ name", metadata.NpqName);
        doc.AssertRowContentMatches("NPQ training provider", metadata.NpqTrainingProvider);
        doc.AssertRowContentMatches("Evidence", $"{metadata.NpqEvidenceFileName} (opens in a new tab)");
    }

    [Test]
    public async Task Post_CreateARecordSelected_HasMatches_RedirectsToExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["CreateRecord"] = "true"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_CreateARecordSelected_NoMatches_RedirectsToExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["CreateRecord"] = "true"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/no-matches/check-answers",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_CreateNewRecordFalse_RedirectsToExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["CreateRecord"] = "false"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/reason",
            response.Headers.Location?.OriginalString);
    }
}
