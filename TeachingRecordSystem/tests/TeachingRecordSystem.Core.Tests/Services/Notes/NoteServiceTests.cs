using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notes;

namespace TeachingRecordSystem.Core.Tests.Services.Notes;

public class NoteServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreateNoteAsync_AddsNoteToDbAndCreatesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var content = Faker.Lorem.Paragraph();
        var fileId = Guid.NewGuid();
        var fileName = "document.pdf";

        var options = new CreateNoteOptions
        {
            PersonId = person.PersonId,
            Content = content,
            CreatedByUserId = user.UserId,
            FileId = fileId,
            OriginalFileName = fileName
        };

        var processContext = new ProcessContext(ProcessType.NoteCreating, Clock.UtcNow, user.UserId);

        // Act
        var note = await WithServiceAsync<NoteService, Note>(service => service.CreateNoteAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbNote = await dbContext.Notes.FindAsync(note.NoteId);
            Assert.NotNull(dbNote);
            Assert.Equal(options.PersonId, dbNote.PersonId);
            Assert.Equal(options.Content, dbNote.Content);
            Assert.Equal(options.CreatedByUserId, dbNote.CreatedByUserId);
            Assert.Equal(options.FileId, dbNote.FileId);
            Assert.Equal(options.OriginalFileName, dbNote.OriginalFileName);
        });

        Events.AssertEventsPublished(x =>
        {
            var noteCreatedEvent = Assert.IsType<NoteCreatedEvent>(x.Event);
            Assert.Equal(person.PersonId, noteCreatedEvent.PersonId);
            Assert.Equal(note.NoteId, noteCreatedEvent.Note.NoteId);
            Assert.Equal(content, noteCreatedEvent.Note.Content);
            Assert.Equal(fileId, noteCreatedEvent.Note.File?.FileId);
            Assert.Equal(fileName, noteCreatedEvent.Note.File?.Name);
        });
    }
}
