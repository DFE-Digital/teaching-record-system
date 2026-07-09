using Optional;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithNonExistentOneLoginUserSubject_ReturnsNotFound()
    {
        // Arrange
        var nonExistentSubject = "non-existent-subject";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{nonExistentSubject}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithOneLoginUserNotConnectedAndOneLoginVerificationRoute_DisplaysOneLoginDetailsAndVerifiedDetails()
    {
        // Arrange
        var email = TestData.GenerateUniqueEmail();
        var verifiedName = new[] { "John", "Doe" };
        var verifiedDateOfBirth = new DateOnly(1990, 1, 1);

        var user = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(email),
            verifiedInfo: (verifiedName, verifiedDateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(email, doc.GetElementByTestId("page-title")!.TrimmedText());

        var oneLoginDetailsCard = doc.GetElementByTestId("one-login-details");
        Assert.NotNull(oneLoginDetailsCard);
        oneLoginDetailsCard.AssertSummaryListRowValue("Email address", v => Assert.Equal(email, v.TrimmedText()));
        oneLoginDetailsCard.AssertSummaryListRowValue("Name", v => Assert.Equal("John Doe", v.TrimmedText()));
        oneLoginDetailsCard.AssertSummaryListRowValue("Date of birth", v => Assert.Equal(verifiedDateOfBirth.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));

        var verifiedDetailsCard = doc.GetElementByTestId("verified-details");
        Assert.Null(verifiedDetailsCard);

        var connectedRecordCard = doc.GetElementByTestId("connected-record-details");
        Assert.Null(connectedRecordCard);

        var connectButton = doc.GetElementByTestId("connect-to-record-button");
        Assert.NotNull(connectButton);
        var disconnectButton = doc.GetElementByTestId("disconnect-record-button");
        Assert.Null(disconnectButton);
    }

    [Theory]
    [InlineData(OneLoginUserVerificationRoute.Support)]
    [InlineData(OneLoginUserVerificationRoute.External)]
    public async Task Get_WithOneLoginUserNotConnectedAndManualVerificationRoute_DisplaysOneLoginDetailsAndSeparateVerifiedDetailsCard(OneLoginUserVerificationRoute verificationRoute)
    {
        // Arrange
        var email = TestData.GenerateUniqueEmail();
        var verifiedName = new[] { "Jane", "Smith" };
        var verifiedDateOfBirth = new DateOnly(1985, 5, 15);
        Guid? verifiedByApplicationUserId = null;

        if (verificationRoute == OneLoginUserVerificationRoute.External)
        {
            var applicationUser = await TestData.CreateApplicationUserAsync();
            verifiedByApplicationUserId = applicationUser.UserId;
        }

        var user = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(email),
            verifiedInfo: (verifiedName, verifiedDateOfBirth));

        await WithDbContextAsync(async dbContext =>
        {
            var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            oneLoginUser.SetVerified(
                TimeProvider.UtcNow,
                verificationRoute,
                verifiedByApplicationUserId: verifiedByApplicationUserId,
                [verifiedName],
                [verifiedDateOfBirth],
                coreIdentityClaimVc: null);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var oneLoginDetailsCard = doc.GetElementByTestId("one-login-details");
        Assert.NotNull(oneLoginDetailsCard);
        oneLoginDetailsCard.AssertSummaryListRowValue("Email address", v => Assert.Equal(email, v.TrimmedText()));

        var verifiedDetailsCard = doc.GetElementByTestId("verified-details");
        Assert.NotNull(verifiedDetailsCard);
        verifiedDetailsCard.AssertSummaryListRowValue("Name", v => Assert.Equal("Jane Smith", v.TrimmedText()));
        verifiedDetailsCard.AssertSummaryListRowValue("Date of birth", v => Assert.Equal(verifiedDateOfBirth.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
    }

    [Fact]
    public async Task Get_WithOneLoginUserConnectedToPerson_DisplaysConnectedRecordDetailsAndDisconnectButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithEmailAddress()
            .WithNationalInsuranceNumber()
            .WithGender());

        var email = TestData.GenerateUniqueEmail();
        var user = await TestData.CreateOneLoginUserAsync(
            personId: person.PersonId,
            email: Option.Some<string?>(email),
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var oneLoginDetailsCard = doc.GetElementByTestId("one-login-details");
        Assert.NotNull(oneLoginDetailsCard);
        oneLoginDetailsCard.AssertSummaryListRowValue("Email address", v => Assert.Equal(email, v.TrimmedText()));

        var verifiedDetailsCard = doc.GetElementByTestId("verified-details");
        Assert.Null(verifiedDetailsCard);
        var verifiedDetailsOneLoginCard = doc.GetElementByTestId("verified-details-one-login");
        Assert.Null(verifiedDetailsOneLoginCard);

        var connectedRecordCard = doc.GetElementByTestId("connected-record-details");
        Assert.NotNull(connectedRecordCard);

        var expectedName = string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
        connectedRecordCard.AssertSummaryListRowValue("Name", v => Assert.Equal(expectedName, v.TrimmedText()));
        connectedRecordCard.AssertSummaryListRowValue("Email address", v => Assert.Equal(person.EmailAddress, v.TrimmedText()));
        connectedRecordCard.AssertSummaryListRowValue("Date of birth", v => Assert.Equal(person.DateOfBirth.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
        connectedRecordCard.AssertSummaryListRowValue("TRN", v => Assert.Equal(person.Trn, v.TrimmedText()));
        connectedRecordCard.AssertSummaryListRowValue("National Insurance number", v => Assert.Equal(person.NationalInsuranceNumber, v.TrimmedText()));
        connectedRecordCard.AssertSummaryListRowValue("Gender", v => Assert.Equal(person.Gender?.GetDisplayName(), v.TrimmedText()));

        var viewRecordLink = connectedRecordCard.GetElementByTestId("view-record-link");
        Assert.NotNull(viewRecordLink);
        Assert.Contains($"/persons/{person.PersonId}", viewRecordLink.GetAttribute("href"));

        var disconnectButton = doc.GetElementByTestId("disconnect-record-button");
        Assert.NotNull(disconnectButton);
        var connectButton = doc.GetElementByTestId("connect-to-record-button");
        Assert.Null(connectButton);
    }

    [Theory]
    [InlineData(PersonStatus.Active)]
    [InlineData(PersonStatus.Deactivated)]
    public async Task Get_WithOneLoginUserConnectedToPerson_DisplaysPersonStatus(PersonStatus personStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithEmailAddress());

        var email = TestData.GenerateUniqueEmail();
        var user = await TestData.CreateOneLoginUserAsync(
            personId: person.PersonId,
            email: Option.Some<string?>(email),
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        // Set the person status
        if (personStatus == PersonStatus.Deactivated)
        {
            await WithDbContextAsync(async dbContext =>
            {
                var personEntity = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
                personEntity.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var expectedVerifiedName = $"{person.FirstName} {person.LastName}";
        Assert.Equal(expectedVerifiedName, doc.GetElementByTestId("page-title")!.TrimmedText());

        var connectedRecordCard = doc.GetElementByTestId("connected-record-details");
        Assert.NotNull(connectedRecordCard);

        var statusRow = connectedRecordCard.GetSummaryListValueElementByKey("Status");
        Assert.NotNull(statusRow);

        if (personStatus == PersonStatus.Deactivated)
        {
            Assert.Contains("Deactivated", statusRow.TrimmedText());
        }
        else
        {
            Assert.Contains("Active", statusRow.TrimmedText());
        }
    }
}
