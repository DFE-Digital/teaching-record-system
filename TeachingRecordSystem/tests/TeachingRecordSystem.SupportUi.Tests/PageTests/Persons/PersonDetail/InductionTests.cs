using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class InductionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_FeatureFlagOff_NoInductionTabShown()
    {
        // Arrange
        FeatureProvider.Features.Clear();
        var person = await TestData.CreatePersonAsync();

        // Act
        var response = await HttpClient.GetAsync($"/persons/{person.ContactId}");

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("induction-tab"));
    }

    [Fact]
    public async Task Get_FeatureFlagOn_InductionTabShown()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var response = await HttpClient.GetAsync($"/persons/{person.ContactId}");

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("induction-tab"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoQts_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var expectedWarning = "This teacher has not been awarded QTS ";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(expectedWarning, doc.GetElementByTestId("induction-status-warning")!.TrimmedText());
        Assert.Null(doc.GetElementByTestId("induction-card"));
    }

    public static IEnumerable<object[]> InductionExemptedRoutes()
    {
        yield return new object[] { RouteToProfessionalStatusType.ScotlandRId, true };
        yield return new object[] { RouteToProfessionalStatusType.NiRId, true };
        yield return new object[] { RouteToProfessionalStatusType.QtlsAndSetMembershipId, true };
        yield return new object[] { RouteToProfessionalStatusType.ScotlandRId, false };
        yield return new object[] { RouteToProfessionalStatusType.NiRId, false };
        yield return new object[] { RouteToProfessionalStatusType.QtlsAndSetMembershipId, false };
    }
    [Theory]
    [MemberData(nameof(InductionExemptedRoutes))]
    public async Task Get_ForPersonWithRouteInductionExemption_RoutesFeatureFlagOn_DisplaysExpectedRowContent(Guid routeId, bool hasExemption)
    {
        // Arrange
        var awardedDate = Clock.Today;
        var routeWithExemption = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.RouteToProfessionalStatusTypeId == routeId)
            .Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(InductionStatus.Exempt)
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(routeWithExemption.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(hasExemption)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        if (hasExemption)
        {
            var expected = $"{routeWithExemption.InductionExemptionReason?.Name} - {routeWithExemption.Name}";
            var routeExemptionRowValue = doc.GetSummaryListValueElementForKey("Route induction exemption reason");
            Assert.Equal(expected, routeExemptionRowValue?.TextContent.Trim());
        }
        else
        {
            Assert.Null(doc.GetSummaryListValueElementForKey("Route induction exemption reason"));
        }
    }

    [Fact]
    public async Task Get_ForPersonWithRouteInductionExemption_FeatureFlagOff_RouteInductionExemptionNotDisplayed()
    {
        // Arrange
        FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);
        var awardedDate = Clock.Today;
        var routeWithExemption = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.ScotlandRId)
            .Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(InductionStatus.Exempt)
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(routeWithExemption.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(true)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetSummaryListValueElementForKey("Route induction exemption reason"));
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
                    .WithStatus(setInductionStatus)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(setInductionStatus.GetTitle(), inductionStatus!.TrimmedText());
        Assert.Null(doc.GetElementByTestId("induction-start-date"));
        Assert.Null(doc.GetElementByTestId("induction-end-date"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusExempt_DisplaysExpected()
    {
        // Arrange
        var exemptionReasonId = InductionExemptionReason.PassedInWalesId;

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(InductionStatus.Exempt)
                    .WithExemptionReasons(exemptionReasonId)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var allExemptionReasons = await ReferenceDataCache.GetInductionExemptionReasonsAsync();
        var expectedExemptionReasonText =
            allExemptionReasons.Single(r => r.InductionExemptionReasonId == exemptionReasonId).Name;

        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(InductionStatus.Exempt.GetTitle(), inductionStatus!.TrimmedText());
        var exemptionReason = doc.GetElementByTestId("induction-exemption-reasons")!.Children[1].TrimmedText();
        Assert.Contains(expectedExemptionReasonText, exemptionReason);
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
        var setStartDate = Clock.Today.AddMonths(-1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(setInductionStatus.GetTitle(), inductionStatus!.TrimmedText());
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TrimmedText();
        Assert.Contains(setStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), startDate);
        Assert.Null(doc.GetElementByTestId("induction-exemption-reasons"));
        Assert.NotNull(doc.GetAllElementsByTestId("induction-backlink"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringStartDateButStartDateIsNull_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                    .WithQts()
                    .WithInductionStatus(inductionBuilder =>
                        inductionBuilder.WithStatus(InductionStatus.InProgress)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var startDate = doc.GetElementByTestId("induction-start-date")!.Children[1].TrimmedText();
        Assert.True(string.IsNullOrWhiteSpace(startDate.Trim()));
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringCompletedDate_DisplaysExpectedCompletedDate(InductionStatus setInductionStatus)
    {
        // Arrange
        var setStartDate = Clock.Today.AddMonths(-1);
        var setCompletedDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(setInductionStatus)
                    .WithStartDate(setStartDate)
                    .WithCompletedDate(setCompletedDate)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains(setInductionStatus.GetTitle(), inductionStatus!.TrimmedText());
        var completedDate = doc.GetElementByTestId("induction-completed-date")!.Children[1].TrimmedText();
        Assert.Contains(setCompletedDate.ToString(UiDefaults.DateOnlyDisplayFormat), completedDate);
    }

    [Theory]
    [InlineData(InductionStatus.RequiredToComplete, "passed, failed, or in progress", true)]
    [InlineData(InductionStatus.InProgress, "required to complete, passed, or failed", true)]
    [InlineData(InductionStatus.Passed, "required to complete, failed, or in progress", true)]
    [InlineData(InductionStatus.Failed, "required to complete, passed, or in progress", true)]
    [InlineData(InductionStatus.InProgress, "required to complete, passed, or failed", false)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusManagedByCpd_ShowsExpectedWarning(
        InductionStatus status,
        string expectedWarningMessage,
        bool hasReadWriteAccess)
    {
        // Arrange
        SetCurrentUser(hasReadWriteAccess
            ? TestUsers.GetUser(UserRoles.SupportOfficer)
            : TestUsers.GetUser(role: null));

        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);

        var person = await TestData.CreatePersonAsync(p => p.WithTrn().WithQts());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                status,
                startDate: status.RequiresStartDate() ? lessThanSevenYearsAgo.AddYears(-1) : null,
                completedDate: status.RequiresCompletedDate() ? lessThanSevenYearsAgo : null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        if (hasReadWriteAccess)
        {
            var warningMessage = doc.GetElementByTestId("induction-status-warning")!.Children[1].TrimmedText();
            Assert.Contains(expectedWarningMessage, warningMessage);
        }
        else
        {
            Assert.Null(doc.GetElementByTestId("induction-status-warning"));
        }
    }

    [Theory]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusManagedByCpd_ShowsNoWarning(InductionStatus trsInductionStatus)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.SupportOfficer));

        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn().WithQts());
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            person.Person.SetInductionStatus(
                trsInductionStatus,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("induction-status-warning"));
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithInductionStatusNotManagedByCpd_DoesNotShowWarning()
    {
        //Arrange
        var underSevenYearsAgo = Clock.Today.AddYears(-6);

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(InductionStatus.InProgress)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("induction-status-warning"));
    }

    [Fact]
    public async Task Get_WithPersonId_ShowsLinkToChangeStatus()
    {
        // Arrange
        var setInductionStatus = InductionStatus.RequiredToComplete;
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(builder => builder
                    .WithStatus(setInductionStatus)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionStatus = doc.GetElementByTestId("induction-status");
        Assert.Contains("Change", inductionStatus!.GetElementsByTagName("a")[0].TrimmedText());
    }

    [Fact]
    public async Task Get_UserHasInductionReadWritePermission_ShowsActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.SupportOfficer));

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(b => b.WithStatus(InductionStatus.Passed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("change-induction-completed-date"));
        Assert.NotNull(doc.GetElementByTestId("change-induction-start-date"));
        Assert.NotNull(doc.GetElementByTestId("change-induction-status"));
    }

    [Fact]
    public async Task Get_UserDoesNotHaveInductionReadWritePermission_DoesNotShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(b => b.WithStatus(InductionStatus.Passed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("change-induction-completed-date"));
        Assert.Null(doc.GetElementByTestId("change-induction-start-date"));
        Assert.Null(doc.GetElementByTestId("change-induction-status"));
    }
}
