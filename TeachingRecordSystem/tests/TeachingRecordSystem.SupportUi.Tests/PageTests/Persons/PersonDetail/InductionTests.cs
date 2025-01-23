using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class InductionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_FeatureFlagOff_NoInductionTabShown()
    {
        // Arrange
        FeatureProvider.Features.Clear();
        var person = await TestData.CreatePersonAsync();

        // Act
        var response = await HttpClient.GetAsync($"/persons/{person.ContactId}");

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("induction-tab"));
    }

    [Fact]
    public async Task Get_FeatureFlagOn_InductionTabShown()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var response = await HttpClient.GetAsync($"/persons/{person.ContactId}");

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("induction-tab"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoQTS_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var expectedWarning = "This teacher has not been awarded QTS ";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusNone_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(builder =>
            builder
                .WithInductionStatus(InductionStatus.None)
                .WithQtlsDate(Clock.Today));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("induction-status-warning"));
        Assert.Null(doc.GetElementByTestId("induction-card"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Theory]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringNoStartDate_DisplaysExpected(InductionStatus setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(setInductionStatus)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(setInductionStatus.GetTitle(), inductionStatus!.TextContent);
        Assert.Null(doc.GetElementByTestId("induction-start-date"));
        Assert.Null(doc.GetElementByTestId("induction-end-date"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusExempt_DisplaysExpected()
    {
        // Arrange
        var exemptionReasonId = InductionExemptionReason.PassedInWalesId;

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(InductionStatus.Exempt)
                    .WithExemptionReasons(exemptionReasonId)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var allExemptionReasons = await ReferenceDataCache.GetInductionExemptionReasonsAsync();
        var expectedExemptionReasonText =
            allExemptionReasons.Single(r => r.InductionExemptionReasonId == exemptionReasonId).Name;

        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(InductionStatus.Exempt.GetTitle(), inductionStatus!.TextContent);
        var exemptionReason = doc.GetElementByTestId("induction-exemption-reasons")!.Children[1].TextContent;
        Assert.Contains(expectedExemptionReasonText, exemptionReason);
        Assert.Null(doc.GetElementByTestId("induction-start-date"));
        Assert.Null(doc.GetElementByTestId("induction-end-date"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringStartDate_DisplaysExpectedContent(InductionStatus setInductionStatus)
    {
        var setStartDate = Clock.Today.AddMonths(-1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(setInductionStatus.GetTitle(), inductionStatus!.TextContent);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.Contains(setStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), startDate);
        Assert.Null(doc.GetElementByTestId("induction-exemption-reasons"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringStartDateButStartDateIsNull_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                    .WithQts()
                    .WithInductionStatus(inductionBuilder =>
                        inductionBuilder.WithStatus(InductionStatus.InProgress)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.True(string.IsNullOrWhiteSpace(startDate.Trim()));
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringCompletedDate_DisplaysExpectedCompletedDate(InductionStatus setInductionStatus)
    {
        // Arrange
        var setStartDate = Clock.Today.AddMonths(-1);
        var setCompletedDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)
                    .WithCompletedDate(setCompletedDate)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(setInductionStatus.GetTitle(), inductionStatus!.TextContent);
        var completedDate = doc.GetElementByTestId("induction-completed-date")!.Children[1].TextContent;
        Assert.Contains(setCompletedDate.ToString(UiDefaults.DateOnlyDisplayFormat), completedDate);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusManagedByCPD_ShowsWarning()
    {
        //Arrange
        var expectedWarning = "To change this teacher’s induction status ";
        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1).AddDays(-1);

        var person = await TestData.CreatePersonAsync();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: lessThanSevenYearsAgo.AddYears(-1),
                completedDate: lessThanSevenYearsAgo,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.Children[1].TextContent);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusNotManagedByCPD_NoWarning()
    {
        //Arrange
        var underSevenYearsAgo = Clock.Today.AddYears(-6);

        var person = await TestData.CreatePersonAsync(
            builder => builder.WithQtlsDate(Clock.Today));

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: underSevenYearsAgo.AddYears(-1),
                completedDate: underSevenYearsAgo,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("induction-status-warning"));
    }

    [Fact]
    public async Task Get_WithPersonId_ShowsLinkToChangeStatus()
    {
        // Arrange
        var setInductionStatus = InductionStatus.RequiredToComplete;
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(setInductionStatus)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains("Change", inductionStatus!.GetElementsByTagName("a")[0].TextContent);
    }

    [Fact]
    public async Task Get_UserHasInductionReadWritePermission_ShowsActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.InductionReadWrite));

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(b => b.WithStatus(InductionStatus.Passed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("change-induction-completed-date"));
        Assert.NotNull(doc.GetElementByTestId("change-induction-start-date"));
        Assert.NotNull(doc.GetElementByTestId("change-induction-status"));
    }

    [Fact]
    public async Task Get_UserDoesNotHaveInductionReadWritePermission_DeosNotShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(b => b.WithStatus(InductionStatus.Passed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("change-induction-completed-date"));
        Assert.Null(doc.GetElementByTestId("change-induction-start-date"));
        Assert.Null(doc.GetElementByTestId("change-induction-status"));
    }
}
