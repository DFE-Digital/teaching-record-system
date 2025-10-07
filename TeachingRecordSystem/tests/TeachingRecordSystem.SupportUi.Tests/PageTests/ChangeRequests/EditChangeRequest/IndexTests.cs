using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests.EditChangeRequest;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.RecordManager));

    [Test]
    public async Task Get_UserWithNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Get_UserWithoutSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithSupportTaskReferenceForNonExistentSupportTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistentSupportTaskReference = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{nonExistentSupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithSupportTaskReferenceForClosedSupportTask_ReturnsNotFound()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)).WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    [Arguments(true, false, false, false)]
    [Arguments(false, true, false, true)]
    [Arguments(false, false, true, false)]
    [Arguments(true, true, false, true)]
    [Arguments(true, false, true, false)]
    [Arguments(false, true, true, true)]
    [Arguments(true, true, true, true)]
    public async Task Get_WithSupportTaskReferenceForOpenChangeNameRequestSupportTask_RendersExpectedContent(bool hasNewFirstName, bool hasNewMiddleName, bool hasNewLastName, bool evidenceIsPdf)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithFirstName(hasNewFirstName ? TestData.GenerateChangedFirstName(createPersonResult.FirstName) : createPersonResult.FirstName)
                .WithMiddleName(hasNewMiddleName ? TestData.GenerateChangedMiddleName(createPersonResult.MiddleName) : createPersonResult.MiddleName)
                .WithLastName(hasNewLastName ? TestData.GenerateChangedLastName(createPersonResult.LastName) : createPersonResult.LastName)
                .WithEvidenceFileName(evidenceIsPdf ? "evidence.pdf" : "evidence.jpg"));

        var changeNameRequestData = (ChangeNameRequestData)supportTask.Data;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"Change of Name - {createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetElementByTestId("heading-caption")!.TrimmedText());

        var firstNameRow = doc.GetElementByTestId("first-name");
        if (hasNewFirstName)
        {
            Assert.NotNull(firstNameRow);
            Assert.Equal(createPersonResult.FirstName, firstNameRow.GetElementByTestId("first-name-current")!.TrimmedText());
            Assert.Equal(changeNameRequestData.FirstName, firstNameRow.GetElementByTestId("first-name-new")!.TrimmedText());
        }
        else
        {
            Assert.Null(firstNameRow);
        }

        var middleNameRow = doc.GetElementByTestId("middle-name");
        if (hasNewMiddleName)
        {
            Assert.NotNull(middleNameRow);
            Assert.Equal(createPersonResult.MiddleName, middleNameRow.GetElementByTestId("middle-name-current")!.TrimmedText());
            Assert.Equal(changeNameRequestData.MiddleName, middleNameRow.GetElementByTestId("middle-name-new")!.TrimmedText());
        }
        else
        {
            Assert.Null(middleNameRow);
        }

        var lastNameRow = doc.GetElementByTestId("last-name");
        if (hasNewLastName)
        {
            Assert.NotNull(lastNameRow);
            Assert.Equal(createPersonResult.LastName, lastNameRow.GetElementByTestId("last-name-current")!.TrimmedText());
            Assert.Equal(changeNameRequestData.LastName, lastNameRow.GetElementByTestId("last-name-new")!.TrimmedText());
        }
        else
        {
            Assert.Null(lastNameRow);
        }

        if (evidenceIsPdf)
        {
            Assert.NotNull(doc.GetElementByTestId($"pdf-{changeNameRequestData.EvidenceFileId}"));
            Assert.Null(doc.GetElementByTestId($"image-{changeNameRequestData.EvidenceFileId}"));
        }
        else
        {
            Assert.NotNull(doc.GetElementByTestId($"image-{changeNameRequestData.EvidenceFileId}"));
            Assert.Null(doc.GetElementByTestId($"pdf-{changeNameRequestData.EvidenceFileId}"));
        }
    }

    [Test]
    public async Task Get_WithSupportTaskReferenceForOpenChangeDateOfBirthRequestSupportTask_RendersExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));

        var changeDateOfBirthRequestData = (ChangeDateOfBirthRequestData)supportTask.Data;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"Change of Date of Birth - {createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetElementByTestId("heading-caption")!.TrimmedText());

        var dateOfBirthRow = doc.GetElementByTestId("date-of-birth");
        Assert.NotNull(dateOfBirthRow);
        Assert.Equal(createPersonResult.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), dateOfBirthRow.GetElementByTestId("date-of-birth-current")!.TrimmedText());
        Assert.Equal(changeDateOfBirthRequestData.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), dateOfBirthRow.GetElementByTestId("date-of-birth-new")!.TrimmedText());

        var imageDocument = doc.GetElementByTestId($"image-{changeDateOfBirthRequestData.EvidenceFileId}");
        Assert.NotNull(imageDocument);
    }
}
