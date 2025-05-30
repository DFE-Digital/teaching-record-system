using System.Diagnostics;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

[Collection(nameof(DisableParallelization))]
public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
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
        var randomEmail = TestData.GenerateUniqueEmail();
        if (!EmailAddress.TryParse(randomEmail, out var email))
        {
            Assert.Fail($@"Randomly generated email address ""{randomEmail}"" is invalid.");
        }
        var randomMobile = TestData.GenerateUniqueMobileNumber();
        if (!MobileNumber.TryParse(randomMobile, out var mobileNumber))
        {
            Assert.Fail($@"Randomly generated mobile number ""{randomMobile}"" is invalid.");
        }
        var updatedFirstName = TestData.GenerateFirstName();
        var updatedMiddleName = TestData.GenerateMiddleName();
        var updatedLastName = TestData.GenerateLastName();
        var previousMiddleNameChangedOn = new DateOnly(2022, 02, 02);
        var createPersonResult = await TestData.CreatePersonAsync(b => b
            .WithTrn()
            .WithEmail((string?)email)
            .WithMobileNumber((string?)mobileNumber)
            .WithNationalInsuranceNumber());

        await TestData.UpdatePersonAsync(b => b
            .WithPersonId(createPersonResult.ContactId)
            .WithUpdatedName(updatedFirstName, updatedMiddleName, createPersonResult.LastName));
        await Task.Delay(2000);
        await TestData.UpdatePersonAsync(b => b
            .WithPersonId(createPersonResult.ContactId)
            .WithUpdatedName(updatedFirstName, updatedMiddleName, updatedLastName));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", doc.GetElementByTestId("page-title")!.TrimmedText());
        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", doc.GetSummaryListValueForKey("Name"));
        var previousNames = doc.GetSummaryListValueElementForKey("Previous name(s)")?.QuerySelectorAll("li");
        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {createPersonResult.LastName}", previousNames?.First().TrimmedText());
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", previousNames?.Last().TrimmedText());
        Assert.Equal(createPersonResult.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(createPersonResult.Gender, doc.GetSummaryListValueForKey("Gender"));
        Assert.Equal(createPersonResult.Trn, doc.GetSummaryListValueForKey("TRN"));
        Assert.Equal(createPersonResult.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(createPersonResult.Email, doc.GetSummaryListValueForKey("Email"));
        Assert.Equal(createPersonResult.MobileNumber, doc.GetSummaryListValueForKey("Mobile number"));
    }

    [Fact]
    public async Task Get_AfterContactsMigrated_WithPersonIdForExistingPersonWithAllPropertiesSet_ReturnsExpectedContent()
    {
        // Arrange
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);

        var randomEmail = TestData.GenerateUniqueEmail();
        if (!EmailAddress.TryParse(randomEmail, out var email))
        {
            Assert.Fail($@"Randomly generated email address ""{randomEmail}"" is invalid.");
        }
        var randomMobile = TestData.GenerateUniqueMobileNumber();
        if (!MobileNumber.TryParse(randomMobile, out var mobileNumber))
        {
            Assert.Fail($@"Randomly generated mobile number ""{randomMobile}"" is invalid.");
        }
        var updatedFirstName = TestData.GenerateFirstName();
        var updatedMiddleName = TestData.GenerateMiddleName();
        var updatedLastName = TestData.GenerateLastName();
        var previousMiddleNameChangedOn = new DateOnly(2022, 02, 02);
        var createPersonResult = await TestData.CreatePersonAsync(b => b
            .WithTrn()
            .WithEmail((string?)email)
            .WithMobileNumber((string?)mobileNumber)
            .WithNationalInsuranceNumber());

        await TestData.UpdatePersonAsync(b => b
            .WithPersonId(createPersonResult.ContactId)
            .WithUpdatedName(updatedFirstName, updatedMiddleName, createPersonResult.LastName)
            .AfterContactsMigrated());
        Clock.Advance();
        await TestData.UpdatePersonAsync(b => b
            .WithPersonId(createPersonResult.ContactId)
            .WithUpdatedName(updatedFirstName, updatedMiddleName, updatedLastName)
            .AfterContactsMigrated());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", doc.GetElementByTestId("page-title")!.TrimmedText());
        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", doc.GetSummaryListValueForKey("Name"));
        var previousNames = doc.GetSummaryListValueElementForKey("Previous name(s)")?.QuerySelectorAll("li");
        Assert.Equal($"{updatedFirstName} {updatedMiddleName} {createPersonResult.LastName}", previousNames?.First().TrimmedText());
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", previousNames?.Last().TrimmedText());
        Assert.Equal(createPersonResult.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Date of birth"));
        // TODO: expected createPersonResult.Gender when Gender migration issue is resolved
        Assert.Equal("Not provided", doc.GetSummaryListValueForKey("Gender"));
        Assert.Equal(createPersonResult.Trn, doc.GetSummaryListValueForKey("TRN"));
        Assert.Equal(createPersonResult.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(createPersonResult.Email, doc.GetSummaryListValueForKey("Email"));
        Assert.Equal(createPersonResult.MobileNumber, doc.GetSummaryListValueForKey("Mobile number"));
    }

    [Fact]
    public async Task Get_WithPersonIdForExistingPersonWithMissingProperties_ReturnsExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b.WithoutTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetElementByTestId("page-title")!.TrimmedText());
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetSummaryListValueForKey("Name"));
        Assert.Equal(createPersonResult.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("TRN"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Email"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Mobile number"));
    }

    [Fact]
    public async Task Get_PersonHasOpenAlert_ShowsAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("OpenAlertNotification"));
    }

    [Fact]
    public async Task Get_PersonHasNoAlert_DoesNotShowAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        Debug.Assert(person.Alerts.Count == 0);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("OpenAlertNotification"));
    }

    [Fact]
    public async Task Get_PersonHasClosedAlertButNoOpenAlert_DoesNotShowAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("OpenAlertNotification"));
    }

    [Fact]
    public async Task Get_DqtNotesTab_IsRendered()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.DqtNotes);
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noMandatoryQualifications = doc.GetElementByTestId("dqtnotes-tab");
        Assert.NotNull(noMandatoryQualifications);
    }
}
