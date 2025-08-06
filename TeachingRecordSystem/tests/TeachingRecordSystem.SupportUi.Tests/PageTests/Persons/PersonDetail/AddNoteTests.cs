namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class AddNoteTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_ContentIsEmpty_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Text", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Text", "Enter text for the note");
    }

    [Fact]
    public async Task Post_ContentWithoutFile_CreatesNoteAndEventAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var text = Faker.Lorem.Paragraph();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/notes/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Text", text }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/notes", response.Headers.Location?.OriginalString);

        var note = await WithDbContext(dbContext => dbContext.Notes.SingleOrDefaultAsync(n => n.PersonId == person.PersonId));
        Assert.NotNull(note);
        Assert.Equal(text, note.Content);
        Assert.Null(note.FileId);

        EventPublisher.AssertEventsSaved(e =>
        {
            var noteCreatedEvent = Assert.IsType<NoteCreatedEvent>(e);
            Assert.Equal(note.NoteId, noteCreatedEvent.Note.NoteId);
            Assert.Equal(person.PersonId, noteCreatedEvent.Note.PersonId);
            Assert.Equal(GetCurrentUserId(), noteCreatedEvent.RaisedBy);
            Assert.Equal(text, noteCreatedEvent.Note.Content);
        });
    }

    [Fact]
    public async Task Post_ContentWithFile_CreatesNoteAndEventAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var text = Faker.Lorem.Paragraph();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/notes/add")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "Text", text },
                { "File", CreateEvidenceFileBinaryContent(), "Attachment.jpeg" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/notes", response.Headers.Location?.OriginalString);

        var note = await WithDbContext(dbContext => dbContext.Notes.SingleOrDefaultAsync(n => n.PersonId == person.PersonId));
        Assert.NotNull(note);
        Assert.Equal(text, note.Content);
        Assert.NotNull(note.FileId);

        EventPublisher.AssertEventsSaved(e =>
        {
            var noteCreatedEvent = Assert.IsType<NoteCreatedEvent>(e);
            Assert.Equal(note.NoteId, noteCreatedEvent.Note.NoteId);
            Assert.Equal(person.PersonId, noteCreatedEvent.Note.PersonId);
            Assert.Equal(GetCurrentUserId(), noteCreatedEvent.RaisedBy);
            Assert.Equal(text, noteCreatedEvent.Note.Content);
            Assert.NotNull(noteCreatedEvent.Note.File);
        });
    }
}
