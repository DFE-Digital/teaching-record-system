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
        throw new NotImplementedException();
    }

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
        //Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent); // to be covered by other test (below)
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.Contains(setStartDate.ToString("d MMMM yyyy"), startDate);
        Assert.Null(doc.GetElementByTestId("induction-exemption-reasons"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringStartDateButStartDateIsNull_DisplaysExpectedContent(InductionStatus setInductionStatus)
    {
        // Arrange
        //var expectedWarning = "To change a teacher's induction status ";
        var person = await TestData.CreatePersonAsync(
                x => x
                .WithQts()
                .WithInductionStatus(setInductionStatus)
                );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        //Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent); // to be covered by other test (below)
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(StatusStrings[setInductionStatus], inductionStatus!.TextContent);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TextContent;
        Assert.True(String.IsNullOrWhiteSpace(startDate.Trim()));
        Assert.Null(doc.GetElementByTestId("induction-exemption-reasons"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    // CML TODO - method name - completed vs completion -when the decision comes in
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringCompletionDate_DisplaysExpectedCompletionDate(InductionStatus setInductionStatus)
    {
        // Arrange
        var setStartDate = Clock.Today.AddMonths(-1);
        var setCompletionDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(
                x => x
                .WithQts()
                .WithInductionStatus(builder => builder
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

    //[Theory]
    //[InlineData(InductionStatus.InProgress)]
    //[InlineData(InductionStatus.Passed)]
    //[InlineData(InductionStatus.Failed)]
    // CL - what I had inferred from page designs
    //public async Task Get_WithPersonIdForPersonWithInductionStatusMangedByCPD_DisplaysWarning(InductionStatus setInductionStatus)
    //{
    //    // Arrange
    //    var expectedWarning = "To change a teacher's induction status ";
    //    var setStartDate = DateOnly.FromDateTime(DateTime.Now);
    //    var person = await TestData.CreatePersonAsync(
    //            x => x
    //            .WithQts()
    //            .WithInductionStatus(builder => builder
    //                .WithStatus(setInductionStatus)
    //                .WithStartDate(setStartDate)
    //            ));

    //    var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await AssertEx.HtmlResponseAsync(response);
    //    Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent);
    //}

    //[Fact]
    //// replacement for above, when TestData builder methods are there - plus add tests for these conditions not met
    //public async Task Get_WithPersonIdForPersonWithInductionStatusMangedByCPD_DisplaysWarning()
    //{
    //    //Arrange
    //    var expectedWarning = "To change a teacher's induction status ";
    //    var sevenYearsAgo = Clock.Today.AddYears(-7).AddDays(-1); // more than 7 years ago
    //    var setCompletionDate = DateOnly.FromDateTime(sevenYearsAgo);
    //    var person = await TestData.CreatePersonAsync(
    //            x => x
    //        .WithQts()
    //        .WithCpdInductionStatus(builder => builder
    //            .WithStatus(It.IsAny<CpdInductionStatus>)
    //            .WithCompletionDate(setCompletionDate)
    //        ));

    //    var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await AssertEx.HtmlResponseAsync(response);
    //    Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TextContent);
    //}
}
