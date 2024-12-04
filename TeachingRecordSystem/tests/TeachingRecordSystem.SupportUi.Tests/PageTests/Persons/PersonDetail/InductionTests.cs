namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class InductionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("none")]
    public async Task Get_WithPersonIdForPersonWithInductionStatus_DisplaysExpectedContent(string setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x
            .WithMandatoryQualification(q => q
                .WithInduction(setInductionStatus)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Equal(setInductionStatus,inductionStatus!.TextContent);
    }

    [Theory]
    [InlineData("none")]
    public async Task Get_WithPersonIdForPersonWithInductionStatus_DisplaysExpectedContent(string setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x
            .WithMandatoryQualification(q => q
                .WithInduction(builder => builder.WithStatus()));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Equal(setInductionStatus, inductionStatus!.TextContent);
    }
}
