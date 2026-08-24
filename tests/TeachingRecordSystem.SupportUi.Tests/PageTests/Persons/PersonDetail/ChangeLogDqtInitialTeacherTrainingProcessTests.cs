namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogDqtInitialTeacherTrainingProcessTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Person_WithInitialTeacherTrainingCreatingInDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.InitialTeacherTrainingCreatingInDqt,
            user.UserId,
            changeReason: null,
            new DqtInitialTeacherTrainingCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
                {
                    InitialTeacherTrainingId = Guid.NewGuid(),
                    Result = "Pass"
                }
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "DQT initial teacher training created",
            user.Name,
            process.CreatedOn,
            [("Result", "Pass")]);
    }

    [Fact]
    public async Task Person_WithInitialTeacherTrainingUpdatingInDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var ittId = Guid.NewGuid();

        var process = await TestData.CreateProcessAsync(
            ProcessType.InitialTeacherTrainingUpdatingInDqt,
            user.UserId,
            changeReason: null,
            new DqtInitialTeacherTrainingUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
                {
                    InitialTeacherTrainingId = ittId,
                    Result = "Pass"
                },
                OldInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
                {
                    InitialTeacherTrainingId = ittId,
                    Result = "InTraining"
                },
                Changes = DqtInitialTeacherTrainingUpdatedEventChanges.Result
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "DQT initial teacher training updated",
            user.Name,
            process.CreatedOn,
            [("Result", "Pass")],
            [("Result", "InTraining")]);
    }
}
