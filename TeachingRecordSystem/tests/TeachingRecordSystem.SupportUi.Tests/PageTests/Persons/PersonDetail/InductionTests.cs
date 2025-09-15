using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class InductionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoQts_DisplaysExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var expectedWarning = "This teacher does not hold QTS ";

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
        var holdsFromDate = Clock.Today;
        var routeWithExemption = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.RouteToProfessionalStatusTypeId == routeId)
            .Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(InductionStatus.Exempt)
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(routeWithExemption.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromDate)
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

    [Theory]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public async Task Get_WithPersonIdForPersonWithInductionStatusRequiringNoStartDate_DisplaysExpected(InductionStatus setInductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(setInductionStatus));

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
            ? TestUsers.GetUser(UserRoles.RecordManager)
            : TestUsers.GetUser(UserRoles.Viewer));

        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);

            // Force status to `None` so that the SetCpdInductionStatus() call below always has a change to status
            person.Person.UnsafeSetInductionStatus(
                InductionStatus.None,
                InductionStatus.None,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: []);

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
        SetCurrentUser(TestUsers.GetUser(UserRoles.RecordManager));

        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts());
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

    [Theory]
    [InlineData(UserRoles.RecordManager, true)]
    [InlineData(UserRoles.Viewer, false)]
    public async Task Get_InductionExemption_UserRole_ShowsActionsAsExpected(string? userRole, bool canSeeActions)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: userRole));

        var holdsFromDate = Clock.Today;
        var routeWithExemption = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.ScotlandRId)
            .Single();
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(InductionStatus.Exempt)
                .WithRouteToProfessionalStatus(r => r
                    .WithRouteType(routeWithExemption.RouteToProfessionalStatusTypeId)
                    .WithStatus(RouteToProfessionalStatusStatus.Holds)
                    .WithHoldsFrom(holdsFromDate)
                    .WithInductionExemption(true)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeLinks = doc.GetAllElementsByTestId(
            "change-induction-status",
            "change-induction-exemption-reason",
            "change-induction-exempted-route"
        );

        if (canSeeActions)
        {
            Assert.Equal(3, changeLinks.Count);
        }
        else
        {
            Assert.Empty(changeLinks);
        }
    }

    [Theory]
    [InlineData(UserRoles.RecordManager, true)]
    [InlineData(UserRoles.Viewer, false)]
    public async Task Get_InductionStartAndEndDate_UserRole_ShowsActionsAsExpected(string? userRole, bool canSeeActions)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: userRole));

        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(b => b.WithStatus(InductionStatus.Passed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeLinks = doc.GetAllElementsByTestId(
            "change-induction-status",
            "change-induction-start-date",
            "change-induction-completed-date"
        );

        if (canSeeActions)
        {
            Assert.Equal(3, changeLinks.Count);
        }
        else
        {
            Assert.Empty(changeLinks);
        }
    }

    [Theory]
    [InlineData(PersonStatus.Active, true)]
    [InlineData(PersonStatus.Deactivated, false)]
    public async Task Get_InductionExemption_PersonStatus_ShowsActionsAsExpected(PersonStatus personStatus, bool canSeeActions)
    {
        // Arrange
        var holdsFromDate = Clock.Today;
        var routeWithExemption = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.ScotlandRId)
            .Single();
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(InductionStatus.Exempt)
                .WithRouteToProfessionalStatus(r => r
                    .WithRouteType(routeWithExemption.RouteToProfessionalStatusTypeId)
                    .WithStatus(RouteToProfessionalStatusStatus.Holds)
                    .WithHoldsFrom(holdsFromDate)
                    .WithInductionExemption(true)));

        if (personStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeLinks = doc.GetAllElementsByTestId(
            "change-induction-status",
            "change-induction-exemption-reason",
            "change-induction-exempted-route"
        );

        if (canSeeActions)
        {
            Assert.Equal(3, changeLinks.Count);
        }
        else
        {
            Assert.Empty(changeLinks);
        }
    }

    [Theory]
    [InlineData(PersonStatus.Active, true)]
    [InlineData(PersonStatus.Deactivated, false)]
    public async Task Get_InductionStartAndEndDate_PersonStatus_ShowsActionsAsExpected(PersonStatus personStatus, bool canSeeActions)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            builder => builder
                .WithQts()
                .WithInductionStatus(b => b.WithStatus(InductionStatus.Passed)));

        if (personStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeLinks = doc.GetAllElementsByTestId(
            "change-induction-status",
            "change-induction-start-date",
            "change-induction-completed-date"
        );

        if (canSeeActions)
        {
            Assert.Equal(3, changeLinks.Count);
        }
        else
        {
            Assert.Empty(changeLinks);
        }
    }
}
