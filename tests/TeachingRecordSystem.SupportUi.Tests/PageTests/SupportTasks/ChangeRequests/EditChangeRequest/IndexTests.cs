using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ChangeRequests.EditChangeRequest;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.RecordManager));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_UserWithNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}");

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
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{nonExistentSupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithSupportTaskReferenceForClosedSupportTask_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)).WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}");

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
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b =>
            {
                var builder = b
                    .WithFirstName(hasNewFirstName ? TestData.GenerateChangedFirstName(person.FirstName) : person.FirstName)
                    .WithMiddleName(hasNewMiddleName ? TestData.GenerateChangedMiddleName(person.MiddleName) : person.MiddleName)
                    .WithLastName(hasNewLastName ? TestData.GenerateChangedLastName(person.LastName) : person.LastName)
                    .WithEvidenceFileName(evidenceIsPdf ? "evidence.pdf" : "evidence.jpg");
                if (!hasRequestEmail)
                {
                    builder = builder.WithoutEmailAddress();
                }
            });

        var changeNameRequestData = (ChangeNameRequestData)supportTask.Data;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(supportTask.GetSubject(), doc.GetElementsByTagName("h1")!.First().TrimmedText());
        Assert.Equal("Change name request", doc.QuerySelector(".govuk-caption-m")!.TrimmedText());
        Assert.Equal(supportTask.SupportTaskReference, doc.QuerySelector(".govuk-caption-l")!.TrimmedText());

        var firstNameRow = doc.GetElementByTestId("first-name");
        if (hasNewFirstName)
        {
            Assert.NotNull(firstNameRow);
            Assert.Equal(person.FirstName, firstNameRow.GetElementByTestId("first-name-current")!.TrimmedText());
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
            Assert.Equal(person.MiddleName, middleNameRow.GetElementByTestId("middle-name-current")!.TrimmedText());
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
            Assert.Equal(person.LastName, lastNameRow.GetElementByTestId("last-name-current")!.TrimmedText());
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
            Assert.Equal(person.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }

        Assert.NotNull(doc.GetElementByTestId("linked-record"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithSupportTaskReferenceForOpenChangeDateOfBirthRequestSupportTask_RendersExpectedContent(bool requestHasEmail)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b =>
            {
                var builder = b
                    .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value));
                if (!requestHasEmail)
                {
                    builder = builder.WithoutEmailAddress();
                }
            });

        var changeDateOfBirthRequestData = (ChangeDateOfBirthRequestData)supportTask.Data;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(supportTask.GetSubject(), doc.GetElementsByTagName("h1")!.First().TrimmedText());
        Assert.Equal("Change date of birth request", doc.QuerySelector(".govuk-caption-m")!.TrimmedText());
        Assert.Equal(supportTask.SupportTaskReference, doc.QuerySelector(".govuk-caption-l")!.TrimmedText());

        var dateOfBirthRow = doc.GetElementByTestId("date-of-birth");
        Assert.NotNull(dateOfBirthRow);
        Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), dateOfBirthRow.GetElementByTestId("date-of-birth-current")!.TrimmedText());
        Assert.Equal(changeDateOfBirthRequestData.DateOfBirth.ToString(WebConstants.DateDisplayFormat), dateOfBirthRow.GetElementByTestId("date-of-birth-new")!.TrimmedText());

        var imageDocument = doc.GetElementByTestId($"image-{changeDateOfBirthRequestData.EvidenceFileId}");
        Assert.NotNull(imageDocument);

        if (requestHasEmail)
        {
            Assert.Equal(changeDateOfBirthRequestData.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }
        else
        {
            Assert.Equal(person.EmailAddress, doc.GetElementByTestId("email-value")?.InnerHtml);
        }

        Assert.NotNull(doc.GetElementByTestId("linked-record"));
    }
}
