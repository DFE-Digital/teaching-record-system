using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks;

public class SupportTaskServiceNoteTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreateNoteAsync_AddsNoteToDb()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var content = Faker.Lorem.Paragraph();

        var options = new CreateSupportTaskNoteOptions
        {
            SupportTaskReference = supportTask.SupportTask.SupportTaskReference,
            Content = content,
            CreatedByUserId = user.UserId
        };

        // Act
        var note = await WithServiceAsync<SupportTaskService, SupportTaskNote>(service => service.CreateNoteAsync(options));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbNote = await dbContext.SupportTaskNotes.FindAsync(note.SupportTaskNoteId);
            Assert.NotNull(dbNote);
            Assert.Equal(options.SupportTaskReference, dbNote.SupportTaskReference);
            Assert.Equal(options.Content, dbNote.Content);
            Assert.Equal(options.CreatedByUserId, dbNote.CreatedByUserId);
            Assert.True((TimeProvider.UtcNow - dbNote.CreatedOn).TotalSeconds < 1);
        });
    }

    [Fact]
    public async Task CreateNoteAsync_ContentMaxLength_SavesSuccessfully()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var content = new string('a', 4000);

        var options = new CreateSupportTaskNoteOptions
        {
            SupportTaskReference = supportTask.SupportTask.SupportTaskReference,
            Content = content,
            CreatedByUserId = user.UserId
        };

        // Act
        var note = await WithServiceAsync<SupportTaskService, SupportTaskNote>(service => service.CreateNoteAsync(options));

        // Assert
        Assert.NotNull(note);
        Assert.Equal(content, note.Content);
    }

    [Fact]
    public async Task CreateNoteAsync_MultipleNotes_AllSaved()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var content1 = "First note";
        var content2 = "Second note";

        var options1 = new CreateSupportTaskNoteOptions
        {
            SupportTaskReference = supportTask.SupportTask.SupportTaskReference,
            Content = content1,
            CreatedByUserId = user.UserId
        };

        var options2 = new CreateSupportTaskNoteOptions
        {
            SupportTaskReference = supportTask.SupportTask.SupportTaskReference,
            Content = content2,
            CreatedByUserId = user.UserId
        };

        // Act
        var note1 = await WithServiceAsync<SupportTaskService, SupportTaskNote>(service => service.CreateNoteAsync(options1));
        var note2 = await WithServiceAsync<SupportTaskService, SupportTaskNote>(service => service.CreateNoteAsync(options2));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var notes = await dbContext.SupportTaskNotes
                .Where(n => n.SupportTaskReference == supportTask.SupportTask.SupportTaskReference)
                .ToListAsync();

            Assert.Equal(2, notes.Count);
            Assert.Contains(notes, n => n.Content == content1);
            Assert.Contains(notes, n => n.Content == content2);
        });
    }
}
