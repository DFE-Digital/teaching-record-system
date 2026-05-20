using TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests;

public class DetailsTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, metadata, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        doc.AssertSummaryListRowValueContentMatches("Name", $"{metadata.FirstName} {metadata.MiddleName} {metadata.LastName}");
        doc.AssertSummaryListRowValueContentMatches("Date of birth", metadata.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Email address", metadata.EmailAddress);
        doc.AssertSummaryListRowValueContentMatches("Working in school or educational setting", metadata.NpqWorkingInEducationalSetting.HasValue ? metadata.NpqWorkingInEducationalSetting.Value ? "Yes" : "No" : WebConstants.EmptyFallbackContent);
        doc.AssertSummaryListRowValueContentMatches("NPQ application ID", metadata.NpqApplicationId);
        doc.AssertSummaryListRowValueContentMatches("NPQ name", metadata.NpqName);
        doc.AssertSummaryListRowValueContentMatches("NPQ training provider", metadata.NpqTrainingProvider);
        doc.AssertSummaryListRowValueContentMatches("Evidence", $"{metadata.NpqEvidenceFileName} (opens in new tab)");
    }

    [Fact]
    public async Task Post_CreateARecordSelected_HasMatches_RedirectsToExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_CreateARecordSelected_NoMatches_RedirectsToExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, _) = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/no-matches",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_CreateNewRecordFalse_RedirectsToExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject",
            response.Headers.Location?.OriginalString);
    }
}
