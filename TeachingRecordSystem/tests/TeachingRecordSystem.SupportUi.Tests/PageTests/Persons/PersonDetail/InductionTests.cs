using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class InductionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private static Dictionary<InductionStatus, string> StatusStrings = new() {
        {InductionStatus.RequiredToComplete, "Required to complete" },
        {InductionStatus.Exempt, "Exempt" },
        {InductionStatus.InProgress, "In progress" },
        {InductionStatus.Passed, "Passed" },
        {InductionStatus.Failed, "Failed" },
        {InductionStatus.FailedInWales, "Failed in Wales"}
    };

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
                    .WithStatus(setInductionStatus)
                    )
                );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        Assert.Null(doc.GetElementByTestId("induction-start-date"));
        Assert.Null(doc.GetElementByTestId("induction-end-date"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Theory]
    [InlineData(InductionStatus.Exempt)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusExempt_DisplaysExpected(InductionStatus setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(setInductionStatus)
                    .WithExemptionReasons(InductionExemptionReasons.SomethingMadeUpForNow)
                    )
                );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        var exemptionReason = doc.GetElementByTestId("induction-exemption-reasons")!.Children[1].TextContent;
        Assert.Contains("SomethingMadeUpForNow", exemptionReason); // CML TODO - needs proper mapping to string
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
        // Arrange
        //var expectedWarning = "To change a teacher's induction status ";
        var setStartDate = Clock.Today.AddMonths(-1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)
                ));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.Contains(setStartDate.ToString("d MMMM yyyy"), startDate);
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
                        inductionBuilder.WithStatus(InductionStatus.InProgress))
                );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.True(String.IsNullOrWhiteSpace(startDate.Trim()));
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringCompletionDate_DisplaysExpectedCompletionDate(InductionStatus setInductionStatus)
    {
        // Arrange
        var setStartDate = Clock.Today.AddMonths(-1);
        var setCompletionDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)
                    .WithCompletedDate(setCompletionDate)
                ));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        var completionDate = doc.GetElementByTestId("induction-completion-date")!.Children[1].TextContent;
        Assert.Contains(setCompletionDate.ToString("d MMMM yyyy"), completionDate);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusManagedByCPD_ShowsWarning()
    {
        //Arrange
        var expectedWarning = "To change a teacher’s induction status ";
        var overSevenYearsAgo = Clock.Today.AddYears(-7).AddDays(-1);

        var person = await TestData.CreatePersonAsync();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: Clock.Today.AddYears(-7).AddMonths(-6),
                completedDate: overSevenYearsAgo,
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
}
