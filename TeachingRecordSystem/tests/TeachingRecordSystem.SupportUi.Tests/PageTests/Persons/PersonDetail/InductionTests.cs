namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class InductionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private static Dictionary<InductionStatus, string> StatusStrings = new() {
        {InductionStatus.None, "Required to complete" },
        {InductionStatus.Exempt, "Exempt" },
        {InductionStatus.InProgress, "In progress" },
        {InductionStatus.Passed, "Passed" },
        {InductionStatus.Failed, "Failed" }
    };

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoQTS_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var expectedWarning = "This teacher doesn’t have QTS ";

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
        var person = await TestData.CreatePersonAsync(builder => builder.WithInductionStatus(InductionStatus.None));

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
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusNotNone_DisplaysInductionCard(InductionStatus setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(builder => builder.WithInductionStatus(setInductionStatus));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("induction-card"));
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Theory]
    [InlineData(InductionStatus.RequiredToComplete)]
    public async Task Get_WithPersonIdForPersonWithNoInductionStartDate_NoStartDateDisplayed(InductionStatus setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithInductionStatus(setInductionStatus));

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
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringStartDate_DisplaysExpectedStartDate(InductionStatus setInductionStatus)
    {
        // Arrange
        var expectedWarning = "To change a teacher's induction status ";
        var setStartDate = DateOnly.FromDateTime(DateTime.Now);
        var person = await TestData.CreatePersonAsync(
                x => x
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)
                ));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.Contains(setStartDate.ToString("dd MMMM yyyy"), startDate);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusMangedByCPD_DisplaysWarning(InductionStatus setInductionStatus)
    {
        // Arrange
        var expectedWarning = "To change a teacher's induction status ";
        var setStartDate = DateOnly.FromDateTime(DateTime.Now);
        var person = await TestData.CreatePersonAsync(
                x => x
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)
                ));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent);
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    // CML method name - completed vs completion -when the decision comes in
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringEndDate_DisplaysExpectedCompletedDate(InductionStatus setInductionStatus)
    {
        // Arrange
        var setStartDate = DateOnly.FromDateTime(DateTime.Now);
        var person = await TestData.CreatePersonAsync(
                x => x
                .WithQts()
                .WithInductionStatus(builder => builder
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
        var completionDate = doc.GetElementByTestId("induction-completion-date")!.Children[1].TextContent;
        Assert.Contains(setStartDate.ToString("dd MMMM yyyy"), completionDate);
    }
}
