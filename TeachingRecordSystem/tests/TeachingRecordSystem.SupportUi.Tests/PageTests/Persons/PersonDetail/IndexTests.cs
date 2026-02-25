using System.Diagnostics;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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
            Assert.Fail($"""Randomly generated email address "{randomEmail}" is invalid.""");
        }

        var person = await TestData.CreatePersonAsync(b => b
            .WithEmailAddress((string?)email)
            .WithNationalInsuranceNumber()
            .WithGender());

        var previousFirstName1 = TestData.GenerateChangedFirstName(person.FirstName);
        var previousMiddleName1 = TestData.GenerateChangedMiddleName(person.MiddleName);
        var previousLastName1 = TestData.GenerateChangedLastName(person.LastName);

        var previousFirstName2 = TestData.GenerateChangedFirstName(person.FirstName);
        var previousMiddleName2 = TestData.GenerateChangedMiddleName(person.MiddleName);
        var previousLastName2 = TestData.GenerateChangedLastName(person.LastName);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = person.PersonId,
                CreatedOn = Clock.UtcNow.AddDays(-1),
                UpdatedOn = Clock.UtcNow.AddDays(-1),
                FirstName = previousFirstName1,
                MiddleName = previousMiddleName1,
                LastName = previousLastName1
            });

            dbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = person.PersonId,
                CreatedOn = Clock.UtcNow.AddDays(-2),
                UpdatedOn = Clock.UtcNow.AddDays(-2),
                FirstName = previousFirstName2,
                MiddleName = previousMiddleName2,
                LastName = previousLastName2
            });

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", doc.GetElementByTestId("page-title")!.TrimmedText());
        Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", doc.GetSummaryListValueByKey("Name"));
        var previousNames = doc.GetSummaryListValueElementByKey("Previous name(s)")?.QuerySelectorAll("li");
        Assert.Equal($"{previousFirstName1} {previousMiddleName1} {previousLastName1}", previousNames?.First().TrimmedText());
        Assert.Equal($"{previousFirstName2} {previousMiddleName2} {previousLastName2}", previousNames?.Last().TrimmedText());
        Assert.Equal(person.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(person.Trn, doc.GetSummaryListValueByKey("TRN"));
        Assert.Equal(person.NationalInsuranceNumber, doc.GetSummaryListValueByKey("National Insurance number"));
        Assert.Equal(person.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(person.Gender?.GetDisplayName(), doc.GetSummaryListValueByKey("Gender"));
    }

    [Fact]
    public async Task Get_WithPersonIdForExistingPersonWithMissingProperties_ReturnsExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetElementByTestId("page-title")!.TrimmedText());
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetSummaryListValueByKey("Name"));
        Assert.Equal(createPersonResult.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(WebConstants.EmptyFallbackContent, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(WebConstants.EmptyFallbackContent, doc.GetSummaryListValueByKey("National Insurance number"));
        Assert.Equal(WebConstants.EmptyFallbackContent, doc.GetSummaryListValueByKey("Gender"));
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
        Assert.NotNull(doc.GetElementByTestId("open-alert-notification"));
    }

    [Fact]
    public async Task Get_PersonHasNoAlert_DoesNotShowAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        Debug.Assert(person.Alerts.Count == 0);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("open-alert-notification"));
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
        Assert.Null(doc.GetElementByTestId("open-alert-notification"));
    }

    [Fact]
    public async Task Get_NotesTab_IsRendered()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var notes = doc.GetElementByTestId("notes-tab");
        Assert.NotNull(notes);
    }

    [Fact]
    public async Task Get_PersonHasNoProfessionalStatusDetails_NoSummaryCardShown()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetAllElementsByTestId("professional-status-details"));
    }

    [Fact]
    public async Task Get_PersonHasQts_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p

            .WithQts(awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("Holds", doc.GetSummaryListValueByKey("Qualified teacher status (QTS)"));
        Assert.Equal(awardDate.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher learning and skills status (QTLS)"));
        Assert.Equal("Required to complete", doc.GetSummaryListValueByKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("PQTS held since"));
    }

    [Fact]
    public async Task Get_PersonHasQtls_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p

            .WithQtls(awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("Holds", doc.GetSummaryListValueByKey("Qualified teacher status (QTS)"));
        Assert.Equal(awardDate.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("QTS held since"));
        Assert.Equal("Active", doc.GetSummaryListValueByKey("Qualified teacher learning and skills status (QTLS)"));
        Assert.Equal("Required to complete", doc.GetSummaryListValueByKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("PQTS held since"));
    }

    [Fact]
    public async Task Get_PersonHasEyts_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p

            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus, awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher status (QTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher learning and skills status (QTLS)"));
        Assert.Null(doc.GetSummaryListValueByKey("Induction status"));
        Assert.Equal("Holds", doc.GetSummaryListValueByKey("Early years teacher status (EYTS)"));
        Assert.Equal(awardDate.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("PQTS held since"));
    }

    [Fact]
    public async Task Get_PersonHasEyps_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p

            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsProfessionalStatus));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher status (QTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher learning and skills status (QTLS)"));
        Assert.Null(doc.GetSummaryListValueByKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("EYTS held since"));
        Assert.Equal("Holds", doc.GetSummaryListValueByKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("PQTS held since"));
    }

    [Fact]
    public async Task Get_PersonHasPqts_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p

            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus, awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher status (QTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Qualified teacher learning and skills status (QTLS)"));
        Assert.Null(doc.GetSummaryListValueByKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueByKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueByKey("Early years professional status (EYPS)"));
        Assert.Equal("Holds", doc.GetSummaryListValueByKey("Partial qualified teacher status (PQTS)"));
        Assert.Equal(awardDate.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("PQTS held since"));
    }

    [Theory]
    [InlineData(UserRoles.Viewer, false)]
    [InlineData(UserRoles.RecordManager, true)]
    [InlineData(UserRoles.AlertsManagerTra, true)]
    [InlineData(UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(UserRoles.AccessManager, true)]
    public async Task Get_UserRolesWithViewOrEditPersonDataPermissions_ChangeDetailsLinkShownAsExpected(string userRole, bool canSeeChangeDetailsLink)
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: userRole);
        SetCurrentUser(user);

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeDetailsLink = doc.GetElementByTestId("change-details-link");
        if (canSeeChangeDetailsLink)
        {
            Assert.NotNull(changeDetailsLink);
        }
        else
        {
            Assert.Null(changeDetailsLink);
        }
    }

    [Fact]
    public async Task Get_PersonIsDeactivated_ChangeDetailsLinkIsHidden()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeDetailsLink = doc.GetElementByTestId("change-details-link");
        Assert.Null(changeDetailsLink);
    }

    // TODO: test user permission

    [Fact]
    public async Task Get_PersonIsDeactivated_DoesNotShowMergeButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("merge-button"));
    }

    [Fact]
    public async Task Get_PersonHasOpenAlert_DoesNotShowMergeButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("merge-button"));
    }

    [Fact]
    public async Task Get_PersonDoesNotHaveOpenAlert_ShowsMergeButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("merge-button"));
    }

    [Theory]
    [InlineData(InductionStatus.InProgress, false)]
    [InlineData(InductionStatus.Passed, false)]
    [InlineData(InductionStatus.Failed, false)]
    [InlineData(InductionStatus.None, true)]
    [InlineData(InductionStatus.Exempt, true)]
    [InlineData(InductionStatus.FailedInWales, true)]
    [InlineData(InductionStatus.RequiredToComplete, true)]
    public async Task Get_PersonWithInductionStatus_ShowsMergeButtonAsExpected(InductionStatus status, bool expectMergeButtonToBeShown)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var mergeButton = doc.GetElementByTestId("merge-button");
        if (expectMergeButtonToBeShown)
        {
            Assert.NotNull(mergeButton);
        }
        else
        {
            Assert.Null(mergeButton);
        }
    }

    [Theory]
    [InlineData(PersonStatus.Active, true)]
    [InlineData(PersonStatus.Deactivated, false)]
    public async Task Get_PersonStatus_ShowsMergeButtonAsExpected(PersonStatus currentStatus, bool expectMergeButtonToBeShown)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var mergeButton = doc.GetElementByTestId("merge-button");
        if (expectMergeButtonToBeShown)
        {
            Assert.NotNull(mergeButton);
        }
        else
        {
            Assert.Null(mergeButton);
        }
    }

    [Theory]
    [InlineData(PersonStatus.Active, false)]
    [InlineData(PersonStatus.Deactivated, true)]
    public async Task Get_ShowsPersonStatusAsExpected(PersonStatus currentStatus, bool expectPersonToBeMarkedAsDeactivated)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink"));

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var pageTitle = doc.GetElementByTestId("page-title");
        Assert.NotNull(pageTitle);
        if (expectPersonToBeMarkedAsDeactivated)
        {
            Assert.Equal("Lily The Pink (deactivated)", pageTitle.TrimmedText());
            doc.AssertSummaryListRowValueContentMatches("Status", "Deactivated");
        }
        else
        {
            Assert.Equal("Lily The Pink", pageTitle.TrimmedText());
            doc.AssertSummaryListRowValueContentMatches("Status", "Active");
        }
    }

    [Theory]
    [InlineData(UserRoles.Viewer, false)]
    [InlineData(UserRoles.RecordManager, true)]
    [InlineData(UserRoles.AlertsManagerTra, false)]
    [InlineData(UserRoles.AlertsManagerTraDbs, false)]
    [InlineData(UserRoles.AccessManager, true)]
    [InlineData(UserRoles.Administrator, true)]
    [InlineData(null, false)]
    public async Task Get_UserDoesNotHavePermission_DoesNotShowSetStatusButton(string? role, bool expectButtonToBeVisible)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var setStatusButton = doc.GetElementByTestId("set-status-button") as IHtmlAnchorElement;

        if (expectButtonToBeVisible)
        {
            Assert.NotNull(setStatusButton);
        }
        else
        {
            Assert.Null(setStatusButton);
        }
    }

    [Theory]
    [InlineData(PersonStatus.Active, "Deactivate record", "Deactivated")]
    [InlineData(PersonStatus.Deactivated, "Reactivate record", "Active")]
    public async Task Get_PersonStatus_RendersSetStatusButtonAsExpected(PersonStatus currentStatus, string expectedButtonText, string expectedTargetStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var setStatusButton = doc.GetElementByTestId("set-status-button") as IHtmlAnchorElement;
        Assert.NotNull(setStatusButton);
        Assert.Equal(expectedButtonText, setStatusButton.TrimmedText());
        Assert.Contains($"/persons/{person.PersonId}/set-status/{expectedTargetStatus}", setStatusButton.Href);
    }

    [Fact]
    public async Task Get_PersonWasDeactivatedAsPartOfAMerge_DoesNotShowSetStatusButton()
    {
        // Arrange
        var primaryPerson = await TestData.CreatePersonAsync();
        var secondaryPerson = await TestData.CreatePersonAsync(p => p
            .WithMergedWithPersonId(primaryPerson.PersonId));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(secondaryPerson.Person);
            secondaryPerson.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{secondaryPerson.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var setStatusButton = doc.GetElementByTestId("set-status-button") as IHtmlAnchorElement;
        Assert.Null(setStatusButton);
    }

    [Fact]
    public async Task Get_WithoutOneLoginUser_DoesNotDisplayOneLoginSummaryCard()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var person = await TestData.CreatePersonAsync(b => b.WithDateOfBirth(dateOfBirth).WithEmailAddress());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginSummary = doc.GetElementByTestId("associated-one-login-users");
        Assert.Null(oneLoginSummary);
    }

    [Fact]
    public async Task Get_WithoutOneLoginUser_DisplaysOneLoginSummaryCard()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var person = await TestData.CreatePersonAsync(b => b.WithDateOfBirth(dateOfBirth).WithEmailAddress());
        var user = await TestData.CreateOneLoginUserAsync(personId: person.PersonId, email: Option.Some(person.EmailAddress), verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginSummary = doc.GetElementByTestId("associated-one-login-users");
        Assert.NotNull(oneLoginSummary);
        oneLoginSummary.AssertSummaryListRowValue("Email address", v => Assert.Equal(person.EmailAddress, v.TrimmedText()));
    }

    [Fact]
    public async Task Get_WithMultipleOneLoginUsers_DisplaysAllOneLoginSummaryRows()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var person = await TestData.CreatePersonAsync(b => b.WithDateOfBirth(dateOfBirth).WithEmailAddress());
        var email2 = TestData.GenerateUniqueEmail();
        await TestData.CreateOneLoginUserAsync(personId: person.PersonId, email: Option.Some(person.EmailAddress), verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        Clock.Advance(TimeSpan.FromDays(1));
        await TestData.CreateOneLoginUserAsync(personId: person.PersonId, email: Option.Some<string?>(email2), verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var oneLoginSummary = doc.GetElementByTestId("associated-one-login-users");
        Assert.NotNull(oneLoginSummary);
        oneLoginSummary.AssertSummaryListHasRows(
            ("Email address", person.EmailAddress),
            ("Email address", email2)
        );
    }
}
