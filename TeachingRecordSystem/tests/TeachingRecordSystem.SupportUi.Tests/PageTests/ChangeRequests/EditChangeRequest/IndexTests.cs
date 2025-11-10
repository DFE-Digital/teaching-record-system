using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests.EditChangeRequest;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.RecordManager));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
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

    [Theory]
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

    [Fact]
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

    [Fact]
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

    [Theory]
    [InlineData(true, false, false, false, false)]
    [InlineData(false, true, false, true, true)]
    [InlineData(false, false, true, false, false)]
    [InlineData(true, true, false, true, true)]
    [InlineData(true, false, true, false, false)]
    [InlineData(false, true, true, true, true)]
    [InlineData(true, true, true, true, false)]
    public async Task Get_WithSupportTaskReferenceForOpenChangeNameRequestSupportTask_RendersExpectedContent(bool hasNewFirstName, bool hasNewMiddleName, bool hasNewLastName, bool evidenceIsPdf, bool hasRequestEmail)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b =>
            {
                var builder = b
                    .WithFirstName(hasNewFirstName ? TestData.GenerateChangedFirstName(createPersonResult.FirstName) : createPersonResult.FirstName)
                    .WithMiddleName(hasNewMiddleName ? TestData.GenerateChangedMiddleName(createPersonResult.MiddleName) : createPersonResult.MiddleName)
                    .WithLastName(hasNewLastName ? TestData.GenerateChangedLastName(createPersonResult.LastName) : createPersonResult.LastName)
                    .WithEvidenceFileName(evidenceIsPdf ? "evidence.pdf" : "evidence.jpg");
                if (!hasRequestEmail)
                {
                    builder = builder.WithoutEmailAddress();
                }
            });

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

        if (hasRequestEmail)
        {
            Assert.Equal(changeNameRequestData.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }
        else
        {
            Assert.Equal(createPersonResult.Person.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }

        Assert.NotNull(doc.GetElementByTestId("linked-record"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithSupportTaskReferenceForOpenChangeDateOfBirthRequestSupportTask_RendersExpectedContent(bool requestHasEmail)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b =>
            {
                var builder = b
                    .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth));
                if (!requestHasEmail)
                {
                    builder = builder.WithoutEmailAddress();
                }
            });

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

        if (requestHasEmail)
        {
            Assert.Equal(changeDateOfBirthRequestData.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }
        else
        {
            Assert.Equal(createPersonResult.Person.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }

        Assert.NotNull(doc.GetElementByTestId("linked-record"));
    }
}
