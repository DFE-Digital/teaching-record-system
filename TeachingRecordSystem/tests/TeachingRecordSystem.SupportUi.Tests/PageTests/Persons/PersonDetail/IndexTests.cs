using System.Diagnostics;
using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
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

    [Test]
    public async Task Get_WithPersonIdForExistingPersonWithAllPropertiesSet_ReturnsExpectedContent()
    {
        // Arrange
        var randomEmail = TestData.GenerateUniqueEmail();
        if (!EmailAddress.TryParse(randomEmail, out var email))
        {
            Assert.Fail($@"Randomly generated email address ""{randomEmail}"" is invalid.");
        }
        var updatedFirstName = TestData.GenerateFirstName();
        var updatedMiddleName = TestData.GenerateMiddleName();
        var updatedLastName = TestData.GenerateLastName();
        var createPersonResult = await TestData.CreatePersonAsync(b => b
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithEmail((string?)email)
            .WithNationalInsuranceNumber()
            .WithGender());

        await TestData.UpdatePersonAsync(b => b
            .WithPersonId(createPersonResult.ContactId)
            .WithUpdatedName(updatedFirstName, updatedMiddleName, createPersonResult.LastName));
        Clock.Advance();
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
        Assert.Equal(createPersonResult.Trn, doc.GetSummaryListValueForKey("TRN"));
        Assert.Equal(createPersonResult.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(createPersonResult.Email, doc.GetSummaryListValueForKey("Email"));
        Assert.Equal(createPersonResult.Gender?.GetDisplayName(), doc.GetSummaryListValueForKey("Gender"));
    }

    [Test]
    [Skip("Flaky on CI")]
    public async Task Get_AfterContactsMigrated_WithPersonIdForExistingPersonWithAllPropertiesSet_ReturnsExpectedContent()
    {
        // Arrange
        var randomEmail = TestData.GenerateUniqueEmail();
        if (!EmailAddress.TryParse(randomEmail, out var email))
        {
            Assert.Fail($@"Randomly generated email address ""{randomEmail}"" is invalid.");
        }
        var updatedFirstName = TestData.GenerateFirstName();
        var updatedMiddleName = TestData.GenerateMiddleName();
        var updatedLastName = TestData.GenerateLastName();
        var createPersonResult = await TestData.CreatePersonAsync(b => b
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithEmail((string?)email)
            .WithNationalInsuranceNumber()
            .WithGender());

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
        Assert.Equal(createPersonResult.Gender?.GetDisplayName(), doc.GetSummaryListValueForKey("Gender"));
        Assert.Equal(createPersonResult.Trn, doc.GetSummaryListValueForKey("TRN"));
        Assert.Equal(createPersonResult.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(createPersonResult.Email, doc.GetSummaryListValueForKey("Email"));
    }

    [Test]
    public async Task Get_WithPersonIdForExistingPersonWithMissingProperties_ReturnsExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetElementByTestId("page-title")!.TrimmedText());
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}", doc.GetSummaryListValueForKey("Name"));
        Assert.Equal(createPersonResult.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Email"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Gender"));
    }

    [Test]
    public async Task Get_PersonHasOpenAlert_ShowsAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithAlert(a => a.WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("open-alert-notification"));
    }

    [Test]
    public async Task Get_PersonHasNoAlert_DoesNotShowAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));
        Debug.Assert(person.Alerts.Count == 0);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("open-alert-notification"));
    }

    [Test]
    public async Task Get_PersonHasClosedAlertButNoOpenAlert_DoesNotShowAlertNotification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithAlert(a => a.WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("open-alert-notification"));
    }

    [Test]
    public async Task Get_NotesTab_IsRendered()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var notes = doc.GetElementByTestId("notes-tab");
        Assert.NotNull(notes);
    }

    [Test]
    public async Task Get_PersonHasNoProfessionalStatusDetails_NoSummaryCardShown()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetAllElementsByTestId("professional-status-details"));
    }

    [Test]
    public async Task Get_PersonHasQts_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithQts(awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("Holds", doc.GetSummaryListValueForKey("Qualified teacher status (QTS)"));
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified Teacher Learning and Skills status (QTLS)"));
        Assert.Equal("Required to complete", doc.GetSummaryListValueForKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("PQTS held since"));
    }

    [Test]
    public async Task Get_PersonHasQtls_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithQtls(awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("Holds", doc.GetSummaryListValueForKey("Qualified teacher status (QTS)"));
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("QTS held since"));
        Assert.Equal("Active", doc.GetSummaryListValueForKey("Qualified Teacher Learning and Skills status (QTLS)"));
        Assert.Equal("Required to complete", doc.GetSummaryListValueForKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("PQTS held since"));
    }

    [Test]
    public async Task Get_PersonHasEyts_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus, awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified teacher status (QTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified Teacher Learning and Skills status (QTLS)"));
        Assert.Null(doc.GetSummaryListValueForKey("Induction status"));
        Assert.Equal("Holds", doc.GetSummaryListValueForKey("Early years teacher status (EYTS)"));
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("PQTS held since"));
    }

    [Test]
    public async Task Get_PersonHasEyps_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsProfessionalStatus));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified teacher status (QTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified Teacher Learning and Skills status (QTLS)"));
        Assert.Null(doc.GetSummaryListValueForKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("EYTS held since"));
        Assert.Equal("Holds", doc.GetSummaryListValueForKey("Early years professional status (EYPS)"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Partial qualified teacher status (PQTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("PQTS held since"));
    }

    [Test]
    public async Task Get_PersonHasPqts_ShowsDetails()
    {
        // Arrange
        var awardDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus, awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("professional-status-details"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified teacher status (QTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("QTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Qualified Teacher Learning and Skills status (QTLS)"));
        Assert.Null(doc.GetSummaryListValueForKey("Induction status"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years teacher status (EYTS)"));
        Assert.Null(doc.GetSummaryListValueForKey("EYTS held since"));
        Assert.Equal("No", doc.GetSummaryListValueForKey("Early years professional status (EYPS)"));
        Assert.Equal("Holds", doc.GetSummaryListValueForKey("Partial qualified teacher status (PQTS)"));
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("PQTS held since"));
    }

    [Test]
    [Arguments(UserRoles.Viewer, false)]
    [Arguments(UserRoles.RecordManager, true)]
    [Arguments(UserRoles.AlertsManagerTra, true)]
    [Arguments(UserRoles.AlertsManagerTraDbs, true)]
    [Arguments(UserRoles.AccessManager, true)]
    public async Task Get_UserRolesWithViewOrEditPersonDataPermissions_ChangeDetailsLinkShownAsExpected(string userRole, bool canSeeChangeDetailsLink)
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: userRole);
        SetCurrentUser(user);

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithQts());

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

    [Test]
    public async Task Get_PersonIsDeactivated_ChangeDetailsLinkIsHidden()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithQts());

        await WithDbContext(async dbContext =>
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

    [Test]
    public async Task Get_PersonIsDeactivated_DoesNotShowMergeButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

        await WithDbContext(async dbContext =>
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

    [Test]
    public async Task Get_PersonHasOpenAlert_DoesNotShowMergeButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithAlert(a => a.WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("merge-button"));
    }

    [Test]
    public async Task Get_PersonDoesNotHaveOpenAlert_ShowsMergeButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("merge-button"));
    }

    [Test]
    [Arguments(InductionStatus.InProgress, false)]
    [Arguments(InductionStatus.Passed, false)]
    [Arguments(InductionStatus.Failed, false)]
    [Arguments(InductionStatus.None, true)]
    [Arguments(InductionStatus.Exempt, true)]
    [Arguments(InductionStatus.FailedInWales, true)]
    [Arguments(InductionStatus.RequiredToComplete, true)]
    public async Task Get_PersonWithInductionStatus_ShowsMergeButtonAsExpected(InductionStatus status, bool expectMergeButtonToBeShown)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
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

    [Test]
    [Arguments(PersonStatus.Active, true)]
    [Arguments(PersonStatus.Deactivated, false)]
    public async Task Get_PersonStatus_ShowsMergeButtonAsExpected(PersonStatus currentStatus, bool expectMergeButtonToBeShown)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
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

    [Test]
    [Arguments(PersonStatus.Active, false)]
    [Arguments(PersonStatus.Deactivated, true)]
    public async Task Get_ShowsPersonStatusAsExpected(PersonStatus currentStatus, bool expectPersonToBeMarkedAsDeactivated)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink")
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
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
            doc.AssertRowContentMatches("Status", "DEACTIVATED");
        }
        else
        {
            Assert.Equal("Lily The Pink", pageTitle.TrimmedText());
            doc.AssertRowContentMatches("Status", "ACTIVE");
        }
    }

    [Test]
    [Arguments(UserRoles.Viewer, false)]
    [Arguments(UserRoles.RecordManager, true)]
    [Arguments(UserRoles.AlertsManagerTra, false)]
    [Arguments(UserRoles.AlertsManagerTraDbs, false)]
    [Arguments(UserRoles.AccessManager, true)]
    [Arguments(UserRoles.Administrator, true)]
    [Arguments(null, false)]
    public async Task Get_UserDoesNotHavePermission_DoesNotShowSetStatusButton(string? role, bool expectButtonToBeVisible)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

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

    [Test]
    [Arguments(PersonStatus.Active, "Deactivate record", "Deactivated")]
    [Arguments(PersonStatus.Deactivated, "Reactivate record", "Active")]
    public async Task Get_PersonStatus_RendersSetStatusButtonAsExpected(PersonStatus currentStatus, string expectedButtonText, string expectedTargetStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
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

    [Test]
    public async Task Get_PersonWasDeactivatedAsPartOfAMerge_DoesNotShowSetStatusButton()
    {
        // Arrange
        var primaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));
        var secondaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithMergedWithPersonId(primaryPerson.PersonId));

        await WithDbContext(async dbContext =>
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
}
