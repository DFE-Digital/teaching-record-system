namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class NotesTests : TestBase
{
    public NotesTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_PersonDoesNotExists_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/notes");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonWithoutNotes_ReturnsExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}/notes");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noNotesMessage = doc.GetElementByTestId("no-dqt-notes");
        Assert.NotNull(noNotesMessage);
        Assert.Equal("There are no notes associated with this record", noNotesMessage.TrimmedText());
    }

    [Fact]
    public async Task Get_NoteWithoutAttachment_ReturnsExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b.WithTrn());
        var expectedNoteText = "Note without attachment";
        var expectedCreatedBy = Faker.Name.FullName();
        var createdByUserId = Guid.NewGuid();
        var note = await TestData.CreateNoteAsync(createPersonResult.ContactId, expectedNoteText, createdByUserId, expectedCreatedBy, null, null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}/notes");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noNotesMessage = doc.GetElementByTestId("no-dqt-notes");
        Assert.Null(noNotesMessage);
        var createdby = doc.GetElementByTestId($"{note.NoteId}-dqt-note-created-by");
        Assert.NotNull(createdby);
        Assert.Equal(expectedCreatedBy, createdby.TrimmedText());
        var createddate = doc.GetElementByTestId($"{note.NoteId}-dqt-note-created-on");
        Assert.NotNull(createddate);
        Assert.Equal(note.CreatedOn.ToString("dd MMMM yyyy 'at' HH:mm"), createddate.TrimmedText());
        var noteText = doc.GetElementByTestId($"{note.NoteId}-dqt-note-text");
        Assert.NotNull(noteText);
        Assert.Equal(expectedNoteText, noteText.TrimmedText());
        var originalFileName = doc.GetElementByTestId($"{note.NoteId}-dqt-note-file-name");
        Assert.Null(originalFileName);
    }

    [Fact]
    public async Task Get_NoteWithAttachment_ReturnsExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b.WithTrn());
        var expectedNoteText = "Note without attachment";
        var expectedCreatedBy = Faker.Name.FullName();
        var createdByUserId = Guid.NewGuid();
        var expectedOriginalFileName = "file.png";
        var note = await TestData.CreateNoteAsync(createPersonResult.ContactId, expectedNoteText, createdByUserId, expectedCreatedBy, expectedOriginalFileName, Guid.NewGuid());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}/notes");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noNotesMessage = doc.GetElementByTestId("no-dqt-notes");
        Assert.Null(noNotesMessage);
        var createdby = doc.GetElementByTestId($"{note.NoteId}-dqt-note-created-by");
        Assert.NotNull(createdby);
        Assert.Equal(expectedCreatedBy, createdby.TrimmedText());
        var createddate = doc.GetElementByTestId($"{note.NoteId}-dqt-note-created-on");
        Assert.NotNull(createddate);
        Assert.Equal(note.CreatedOn.ToString("dd MMMM yyyy 'at' HH:mm"), createddate.TrimmedText());
        var noteText = doc.GetElementByTestId($"{note.NoteId}-dqt-note-text");
        Assert.NotNull(noteText);
        Assert.Equal(expectedNoteText, noteText.TrimmedText());
        var originalFileName = doc.GetElementByTestId($"{note.NoteId}-dqt-note-file-name");
        Assert.NotNull(originalFileName);
        Assert.Equal(expectedOriginalFileName, originalFileName.TrimmedText());
    }

    [Fact]
    public async Task Get_NoteWithHtml_ReturnsPlaintext()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b.WithTrn());
        var expectedNoteText = "Note without attachment";
        var htmlNote = $"<html><b>{expectedNoteText}<b><html>";
        var expectedCreatedBy = Faker.Name.FullName();
        var createdByUserId = Guid.NewGuid();
        var expectedOriginalFileName = "file.png";
        var note = await TestData.CreateNoteAsync(createPersonResult.ContactId, htmlNote, createdByUserId, expectedCreatedBy, expectedOriginalFileName, null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}/notes");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noteText = doc.GetElementByTestId($"{note.NoteId}-dqt-note-text");
        Assert.NotNull(noteText);
        Assert.Equal(expectedNoteText, noteText.TrimmedText());
    }

    [Fact]
    public async Task Get_MultipleNotes_ReturnsContentInCorrectOrder()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(b => b.WithTrn());
        var expectedNoteText1 = "Note without attachment";
        var expectedCreatedBy1 = Faker.Name.FullName();
        var createdByUserId1 = Guid.NewGuid();
        var expectedOriginalFileName1 = "file.png";
        var expectedCreatedDate1 = Clock.UtcNow.AddMonths(-1);
        var expectedNoteText2 = "A second note that means something";
        var expectedCreatedBy2 = Faker.Name.FullName();
        var createdByUserId2 = Guid.NewGuid();
        var expectedOriginalFileName2 = "file2.png";
        var expectedCreatedDate2 = Clock.UtcNow;
        var note1 = await TestData.CreateNoteAsync(createPersonResult.ContactId, expectedNoteText1, createdByUserId1, expectedCreatedBy1, expectedOriginalFileName1, Guid.NewGuid(), expectedCreatedDate1);
        var note2 = await TestData.CreateNoteAsync(createPersonResult.ContactId, expectedNoteText2, createdByUserId2, expectedCreatedBy2, expectedOriginalFileName2, Guid.NewGuid(), expectedCreatedDate2);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{createPersonResult.ContactId}/notes");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var notes = doc.GetAllElementsByTestId("dqtnote");
        Assert.Collection(notes,
            item1 =>
            {
                var createdby = item1.GetElementByTestId($"{note2.NoteId}-dqt-note-created-by");
                Assert.NotNull(createdby);
                Assert.Equal(expectedCreatedBy2, createdby.TrimmedText());
                var createddate = item1.GetElementByTestId($"{note2.NoteId}-dqt-note-created-on");
                Assert.NotNull(createddate);
                Assert.Equal(note2.CreatedOn.ToString("dd MMMM yyyy 'at' HH:mm"), createddate.TrimmedText());
                var noteText = item1.GetElementByTestId($"{note2.NoteId}-dqt-note-text");
                Assert.NotNull(noteText);
                Assert.Equal(expectedNoteText2, noteText.TrimmedText());
                var originalFileName = item1.GetElementByTestId($"{note2.NoteId}-dqt-note-file-name");
                Assert.NotNull(originalFileName);
                Assert.Equal(expectedOriginalFileName2, originalFileName.TrimmedText());
            },
            item2 =>
            {
                var createdby = item2.GetElementByTestId($"{note1.NoteId}-dqt-note-created-by");
                Assert.NotNull(createdby);
                Assert.Equal(expectedCreatedBy1, createdby.TrimmedText());
                var createddate = item2.GetElementByTestId($"{note1.NoteId}-dqt-note-created-on");
                Assert.NotNull(createddate);
                Assert.Equal(note1.CreatedOn.ToString("dd MMMM yyyy 'at' HH:mm"), createddate.TrimmedText());
                var noteText = item2.GetElementByTestId($"{note1.NoteId}-dqt-note-text");
                Assert.NotNull(noteText);
                Assert.Equal(expectedNoteText1, noteText.TrimmedText());
                var originalFileName = item2.GetElementByTestId($"{note1.NoteId}-dqt-note-file-name");
                Assert.NotNull(originalFileName);
                Assert.Equal(expectedOriginalFileName1, originalFileName.TrimmedText());
            });
    }
}
