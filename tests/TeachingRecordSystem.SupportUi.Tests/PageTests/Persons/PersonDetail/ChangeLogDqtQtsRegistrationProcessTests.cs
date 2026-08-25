namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogDqtQtsRegistrationProcessTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.QtsDate)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EytsDate)]
    public async Task Person_WithQtsRegistrationCreatingInDqtProcess_RendersExpectedContent(DqtQtsRegistrationUpdatedEventChanges populatedField)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var teacherStatusName = "Trainee teacher";
        var earlyYearsStatusName = "Early Years Trainee";
        var qtsDate = TimeProvider.Today.AddDays(-100);
        var eytsDate = TimeProvider.Today.AddDays(-50);

        var process = await TestData.CreateProcessAsync(
            ProcessType.QtsRegistrationCreatingInDqt,
            user.UserId,
            changeReason: null,
            new DqtQtsRegistrationCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                QtsRegistration = new EventModels.DqtQtsRegistration
                {
                    TeacherStatusName = populatedField == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue ? teacherStatusName : null,
                    EarlyYearsStatusName = populatedField == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue ? earlyYearsStatusName : null,
                    QtsDate = populatedField == DqtQtsRegistrationUpdatedEventChanges.QtsDate ? qtsDate : null,
                    EytsDate = populatedField == DqtQtsRegistrationUpdatedEventChanges.EytsDate ? eytsDate : null
                }
            });

        // Only the fields the event carries a value for are rendered.
        var expectedRows = new List<(string Key, string? Value)>();
        if (populatedField == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)
        {
            expectedRows.Add(("Teacher status name", teacherStatusName));
        }
        if (populatedField == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)
        {
            expectedRows.Add(("Early years status name", earlyYearsStatusName));
        }
        if (populatedField == DqtQtsRegistrationUpdatedEventChanges.QtsDate)
        {
            expectedRows.Add(("QTS date", qtsDate.ToString(WebConstants.DateDisplayFormat)));
        }
        if (populatedField == DqtQtsRegistrationUpdatedEventChanges.EytsDate)
        {
            expectedRows.Add(("EYTS date", eytsDate.ToString(WebConstants.DateDisplayFormat)));
        }

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "DQT QTS registration created",
            user.Name,
            process.CreatedOn,
            expectedRows,
            expectedPreviousDataSummaryListRows: []);
    }

    [Theory]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.QtsDate)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EytsDate)]
    public async Task Person_WithQtsRegistrationUpdatingInDqtProcess_RendersExpectedContent(DqtQtsRegistrationUpdatedEventChanges changes)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var oldTeacherStatusName = "Trainee teacher";
        var oldEarlyYearsStatusName = "Early Years Trainee";
        var oldQtsDate = TimeProvider.Today.AddDays(-100);
        var oldEytsDate = TimeProvider.Today.AddDays(-50);
        var teacherStatusName = "Qualified Teacher (trained)";
        var earlyYearsStatusName = "Early Years Teacher Status";
        var qtsDate = oldQtsDate.AddDays(1);
        var eytsDate = oldEytsDate.AddDays(1);

        var process = await TestData.CreateProcessAsync(
            ProcessType.QtsRegistrationUpdatingInDqt,
            user.UserId,
            changeReason: null,
            new DqtQtsRegistrationUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                QtsRegistration = new EventModels.DqtQtsRegistration
                {
                    TeacherStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue ? teacherStatusName : null,
                    EarlyYearsStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue ? earlyYearsStatusName : null,
                    QtsDate = changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate ? qtsDate : null,
                    EytsDate = changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate ? eytsDate : null
                },
                OldQtsRegistration = new EventModels.DqtQtsRegistration
                {
                    TeacherStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue ? oldTeacherStatusName : null,
                    EarlyYearsStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue ? oldEarlyYearsStatusName : null,
                    QtsDate = changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate ? oldQtsDate : null,
                    EytsDate = changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate ? oldEytsDate : null
                },
                Changes = changes
            });

        // A row is rendered for each changed field, in both the current and the previous data lists.
        var expectedRows = new List<(string Key, string? Value)>();
        var expectedPreviousRows = new List<(string Key, string? Value)>();
        if (changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)
        {
            expectedRows.Add(("Teacher status name", teacherStatusName));
            expectedPreviousRows.Add(("Teacher status name", oldTeacherStatusName));
        }
        if (changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)
        {
            expectedRows.Add(("Early years status name", earlyYearsStatusName));
            expectedPreviousRows.Add(("Early years status name", oldEarlyYearsStatusName));
        }
        if (changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate)
        {
            expectedRows.Add(("QTS date", qtsDate.ToString(WebConstants.DateDisplayFormat)));
            expectedPreviousRows.Add(("QTS date", oldQtsDate.ToString(WebConstants.DateDisplayFormat)));
        }
        if (changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate)
        {
            expectedRows.Add(("EYTS date", eytsDate.ToString(WebConstants.DateDisplayFormat)));
            expectedPreviousRows.Add(("EYTS date", oldEytsDate.ToString(WebConstants.DateDisplayFormat)));
        }

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "DQT QTS registration updated",
            user.Name,
            process.CreatedOn,
            expectedRows,
            expectedPreviousRows);
    }
}
