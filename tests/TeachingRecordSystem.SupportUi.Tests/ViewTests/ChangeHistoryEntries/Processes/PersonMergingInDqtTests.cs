using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonMergingInDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly_ForDeactivatedRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var mergedWithPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            // Note Person.MergedWithPersonId isn't set for merges that happened in DQT
            await dbContext.SaveChangesAsync();
        });

        var @event = new PersonDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
            MergedWithPersonId = mergedWithPerson.PersonId,
            DateOfDeath = null,
        };

        var user = SystemUser.Instance;
        var updatedEvent = new PersonUpdatedInDqtEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = mergedWithPerson.PersonId,
            Changes = PersonUpdatedInDqtEventChanges.FirstName,
            Details = new EventModels.DqtPersonDetails
            {
                FirstName = mergedWithPerson.FirstName + "-new",
                MiddleName = mergedWithPerson.MiddleName,
                LastName = mergedWithPerson.LastName,
                DateOfBirth = mergedWithPerson.DateOfBirth,
                EmailAddress = mergedWithPerson.EmailAddress,
                NationalInsuranceNumber = mergedWithPerson.NationalInsuranceNumber,
                Gender = mergedWithPerson.Gender,
                Trn = mergedWithPerson.Trn,
                DateOfDeath = null,
                QtsDate = null,
                EytsDate = null,
                QtlsDate = null,
                QtlsStatus = QtlsStatus.Active,
                InductionStatus = InductionStatus.Passed,
                DqtInductionStatus = "Pass"
            },
            OldDetails = new EventModels.DqtPersonDetails
            {
                FirstName = mergedWithPerson.FirstName,
                MiddleName = mergedWithPerson.MiddleName,
                LastName = mergedWithPerson.LastName,
                DateOfBirth = mergedWithPerson.DateOfBirth,
                EmailAddress = mergedWithPerson.EmailAddress,
                NationalInsuranceNumber = mergedWithPerson.NationalInsuranceNumber,
                Gender = mergedWithPerson.Gender,
                Trn = mergedWithPerson.Trn,
                DateOfDeath = null,
                QtsDate = null,
                EytsDate = null,
                QtlsDate = null,
                QtlsStatus = QtlsStatus.Active,
                InductionStatus = InductionStatus.Passed,
                DqtInductionStatus = "Pass"
            }
        };

        var process = await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event, updatedEvent);


        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Record merged in DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.NotNull(bodyText);
        Assert.Contains($"Record merged with TRN {mergedWithPerson.Trn}", bodyText);
        Assert.Contains("and deactivated", bodyText);

        var mergedWithLink = entry.GetElementByTestId("merged-with-person-link");
        Assert.NotNull(mergedWithLink);
        Assert.Contains(mergedWithPerson.Trn, mergedWithLink!.TextContent);
        var mergedWithHref = mergedWithLink.GetAttribute("href")!;
        Assert.Contains(mergedWithPerson.PersonId.ToString(), mergedWithHref);

        // Even if an updated event exists, when viewing the deactivated record we should not render the details
        Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProcessRendersCorrectly_ForRetainedRecord(bool hasUpdatedEvent)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var mergedWithPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            // Note Person.MergedWithPersonId isn't set for merges that happened in DQT
            await dbContext.SaveChangesAsync();
        });

        var @event = new PersonDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
            MergedWithPersonId = mergedWithPerson.PersonId,
            DateOfDeath = null,
        };

        var user = SystemUser.Instance;
        PersonUpdatedInDqtEvent? updatedEvent = null;
        if (hasUpdatedEvent)
        {
            updatedEvent = new PersonUpdatedInDqtEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = mergedWithPerson.PersonId,
                Changes = PersonUpdatedInDqtEventChanges.FirstName,
                Details = new EventModels.DqtPersonDetails
                {
                    FirstName = mergedWithPerson.FirstName + "-new",
                    MiddleName = mergedWithPerson.MiddleName,
                    LastName = mergedWithPerson.LastName,
                    DateOfBirth = mergedWithPerson.DateOfBirth,
                    EmailAddress = mergedWithPerson.EmailAddress,
                    NationalInsuranceNumber = mergedWithPerson.NationalInsuranceNumber,
                    Gender = mergedWithPerson.Gender,
                    Trn = mergedWithPerson.Trn,
                    DateOfDeath = null,
                    QtsDate = null,
                    EytsDate = null,
                    QtlsDate = null,
                    QtlsStatus = QtlsStatus.Active,
                    InductionStatus = InductionStatus.Passed,
                    DqtInductionStatus = "Pass"
                },
                OldDetails = new EventModels.DqtPersonDetails
                {
                    FirstName = mergedWithPerson.FirstName,
                    MiddleName = mergedWithPerson.MiddleName,
                    LastName = mergedWithPerson.LastName,
                    DateOfBirth = mergedWithPerson.DateOfBirth,
                    EmailAddress = mergedWithPerson.EmailAddress,
                    NationalInsuranceNumber = mergedWithPerson.NationalInsuranceNumber,
                    Gender = mergedWithPerson.Gender,
                    Trn = mergedWithPerson.Trn,
                    DateOfDeath = null,
                    QtsDate = null,
                    EytsDate = null,
                    QtlsDate = null,
                    QtlsStatus = QtlsStatus.Active,
                    InductionStatus = InductionStatus.Passed,
                    DqtInductionStatus = "Pass"
                }
            };
        }

        var process = hasUpdatedEvent
            ? await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event, updatedEvent!)
            : await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, mergedWithPerson.PersonId);

        // Assert
        AssertTitle(entry, "Record merged in DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.NotNull(bodyText);
        Assert.Contains($"Record merged with TRN {person.Trn}", bodyText);
        Assert.DoesNotContain("and deactivated", bodyText);

        var mergedWithLink = entry.GetElementByTestId("merged-with-person-link");
        Assert.NotNull(mergedWithLink);
        Assert.Contains(person.Trn, mergedWithLink!.TextContent);
        var mergedWithHref = mergedWithLink.GetAttribute("href")!;
        Assert.Contains(person.PersonId.ToString(), mergedWithHref);

        if (hasUpdatedEvent)
        {
            // When viewing the retained person and an updated event exists, the details partial should be rendered
            Assert.NotEmpty(entry.QuerySelectorAll(".govuk-summary-list"));

            var expectedNewName = $"{mergedWithPerson.FirstName}-new {mergedWithPerson.MiddleName} {mergedWithPerson.LastName}";
            var expectedOldName = $"{mergedWithPerson.FirstName} {mergedWithPerson.MiddleName} {mergedWithPerson.LastName}";

            entry.AssertSummaryListRowValueContentMatches("Previous name", expectedOldName);
            entry.AssertSummaryListRowValueContentMatches("Name", expectedNewName);
        }
        else
        {
            Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
        }
    }

    [Fact]
    public async Task ProcessRendersCorrectly_WhenMergedWithPersonNoLongerExists_ForDeactivatedRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        // Note the merged-with person id doesn't correspond to a row in the Person table
        // (e.g. it may have subsequently been deleted); the view should degrade gracefully.
        var mergedWithPersonId = Guid.NewGuid();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var @event = new PersonDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
            MergedWithPersonId = mergedWithPersonId,
            DateOfDeath = null,
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Record merged in DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.NotNull(bodyText);
        Assert.Contains("Record merged with another record", bodyText);
        Assert.Contains("and deactivated", bodyText);

        Assert.Null(entry.GetElementByTestId("merged-with-person-link"));
    }

    [Fact]
    public async Task ProcessRendersCorrectly_WhenMergedWithPersonNoLongerExists_ForRetainedRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        // Note the merged-with person id doesn't correspond to a row in the Person table
        // (e.g. it may have subsequently been deleted); the view should degrade gracefully.
        var mergedWithPersonId = Guid.NewGuid();

        var @event = new PersonDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = mergedWithPersonId,
            Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
            MergedWithPersonId = person.PersonId,
            DateOfDeath = null,
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Record merged in DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.NotNull(bodyText);
        Assert.Contains("Record merged with another record", bodyText);
        Assert.DoesNotContain("and deactivated", bodyText);

        Assert.Null(entry.GetElementByTestId("merged-with-person-link"));
    }
}
