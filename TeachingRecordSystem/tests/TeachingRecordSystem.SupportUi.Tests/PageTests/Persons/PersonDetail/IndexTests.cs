namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange        
        var nonExistentPersonId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForExistingPersonWithAllPropertiesSet_ReturnsExpectedContent()
    {
        // Arrange
        var email = TestData.GenerateUniqueEmail();
        var mobileNumber = TestData.GenerateUniqueMobileNumber();
        var createPersonResult = await TestData.CreatePerson(b => b.WithEmail(email).WithMobileNumber(mobileNumber));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();

        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.LastName}", doc.GetElementByTestId("page-title")!.TextContent);
        var summaryList = doc.GetElementByTestId("personal-details");
        Assert.NotNull(summaryList);
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", summaryList.GetElementByTestId("personal-details-name")!.TextContent);
        Assert.Equal(createPersonResult.DateOfBirth.ToString("dd/MM/yyyy"), summaryList.GetElementByTestId("personal-details-date-of-birth")!.TextContent);
        Assert.Equal(createPersonResult.Trn, summaryList.GetElementByTestId("personal-details-trn")!.TextContent);
        Assert.Equal(createPersonResult.Email, summaryList.GetElementByTestId("personal-details-email")!.TextContent);
        Assert.Equal(createPersonResult.MobileNumber, summaryList.GetElementByTestId("personal-details-mobile-number")!.TextContent);
    }

    [Fact]
    public async Task Get_WithPersonIdForExistingPersonWithMissingProperties_ReturnsExpectedContent()
    {
        // Arrange
        var email = TestData.GenerateUniqueEmail();
        var mobileNumber = TestData.GenerateUniqueMobileNumber();
        var createPersonResult = await TestData.CreatePerson(b => b.WithTrn(hasTrn: false));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();

        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.LastName}", doc.GetElementByTestId("page-title")!.TextContent);
        var summaryList = doc.GetElementByTestId("personal-details");
        Assert.NotNull(summaryList);
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", summaryList.GetElementByTestId("personal-details-name")!.TextContent);
        Assert.Equal(createPersonResult.DateOfBirth.ToString("dd/MM/yyyy"), summaryList.GetElementByTestId("personal-details-date-of-birth")!.TextContent);
        Assert.Equal("Not provided", summaryList.GetElementByTestId("personal-details-trn")!.TextContent);
        Assert.Equal("Not provided", summaryList.GetElementByTestId("personal-details-email")!.TextContent);
        Assert.Equal("Not provided", summaryList.GetElementByTestId("personal-details-mobile-number")!.TextContent);
    }
}
