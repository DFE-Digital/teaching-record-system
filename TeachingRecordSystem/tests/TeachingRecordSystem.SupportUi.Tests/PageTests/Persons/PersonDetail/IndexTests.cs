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
        var updatedFirstName = TestData.GenerateFirstName();
        var updatedMiddleName = TestData.GenerateMiddleName();
        var updatedLastName = TestData.GenerateLastName();
        var previousMiddleNameChangedOn = new DateOnly(2022, 02, 02);
        var createPersonResult = await TestData.CreatePerson(
            b => b.WithEmail(email)
             .WithMobileNumber(mobileNumber)
             .WithNationalInsuranceNumber());

        await TestData.UpdatePerson(b => b.WithPersonId(createPersonResult.ContactId).WithUpdatedName(updatedFirstName, updatedMiddleName, createPersonResult.LastName));
        await Task.Delay(2000);
        await TestData.UpdatePerson(b => b.WithPersonId(createPersonResult.ContactId).WithUpdatedName(updatedFirstName, updatedMiddleName, updatedLastName));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();

        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", doc.GetElementByTestId("page-title")!.TextContent);
        var summaryList = doc.GetElementByTestId("personal-details");
        Assert.NotNull(summaryList);
        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", summaryList.GetElementByTestId("personal-details-name")!.TextContent);
        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {createPersonResult.LastName}", summaryList.GetElementByTestId("personal-details-previous-names-0")!.TextContent);
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", summaryList.GetElementByTestId("personal-details-previous-names-1")!.TextContent);
        Assert.Equal(createPersonResult.DateOfBirth.ToString("dd/MM/yyyy"), summaryList.GetElementByTestId("personal-details-date-of-birth")!.TextContent);
        Assert.Equal(createPersonResult.Gender, summaryList.GetElementByTestId("personal-details-gender")!.TextContent);
        Assert.Equal(createPersonResult.Trn, summaryList.GetElementByTestId("personal-details-trn")!.TextContent);
        Assert.Equal(createPersonResult.NationalInsuranceNumber, summaryList.GetElementByTestId("personal-details-nino")!.TextContent);
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

        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetElementByTestId("page-title")!.TextContent);
        var summaryList = doc.GetElementByTestId("personal-details");
        Assert.NotNull(summaryList);
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", summaryList.GetElementByTestId("personal-details-name")!.TextContent);
        Assert.Equal(createPersonResult.DateOfBirth.ToString("dd/MM/yyyy"), summaryList.GetElementByTestId("personal-details-date-of-birth")!.TextContent);
        Assert.Equal("-", summaryList.GetElementByTestId("personal-details-trn")!.TextContent);
        Assert.Equal("-", summaryList.GetElementByTestId("personal-details-email")!.TextContent);
        Assert.Equal("-", summaryList.GetElementByTestId("personal-details-nino")!.TextContent);
        Assert.Equal("-", summaryList.GetElementByTestId("personal-details-mobile-number")!.TextContent);
    }
}
