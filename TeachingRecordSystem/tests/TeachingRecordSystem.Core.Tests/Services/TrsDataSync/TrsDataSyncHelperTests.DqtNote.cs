using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Fact]
    public async Task SyncNoteAsync_NewRecord_WritesNewRowToDb()
    {
        // Arrange
        var createdByDqtUserName = Faker.Name.First();
        var ct = new CancellationTokenSource();
        var noteText = "this is a note";
        var annotationId = Guid.NewGuid();
        var attachmentFileName = default(string?);
        var mimeType = default(string?);
        var createPersonResult = await TestData.CreatePersonAsync();
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = default(Guid?);
        var updatedOn = default(DateTime?);
        var note = CreateAnnotationEntity(annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            updatedBy,
            updatedOn,
            createdByDqtUserName,
            null,
            null);

        // Act
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtNote = await dbContext.Notes.SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.NotNull(dqtNote);
            Assert.Null(dqtNote.FileName);
            Assert.Equal(createPersonResult.PersonId, dqtNote.PersonId);
            Assert.Equal(noteText, dqtNote.ContentHtml);
            Assert.Equal(createdOn, dqtNote.CreatedOn);
            Assert.Equal(createdOn, dqtNote.UpdatedOn);

            //updatedon is defaulted to createdon
            Assert.Equal(note.CreatedBy.Id, dqtNote.CreatedByDqtUserId);
            Assert.Equal(createdByDqtUserName, dqtNote.CreatedByDqtUserName);
            Assert.Null(dqtNote.UpdatedByDqtUserId);
            Assert.Null(dqtNote.UpdatedByDqtUserName);
            Assert.Null(dqtNote.OriginalFileName);
            BlobStorageFileService.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), note.Id), Times.Never());
            BlobStorageFileService.Verify(x => x.DeleteFileAsync(note.Id), Times.Once());
        });
    }

    [Fact]
    public async Task Annotation_UpdatedRecord_WritesUpdatedNotesRecordToDatabase()
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var createdByDqtUserName = Faker.Name.First();
        var updatedByUserName = Faker.Name.First();
        var noteText = "this is a note";
        var annotationId = Guid.NewGuid();
        var attachmentFileName = default(string?);
        var mimeType = default(string?);
        var createPersonResult = await TestData.CreatePersonAsync();
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = Guid.NewGuid();
        var updatedOn = default(DateTime?);
        var note = CreateAnnotationEntity(annotationId: annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            updatedBy,
            updatedOn,
            createdByDqtUserName,
            updatedByUserName,
            null);
        var updatedNoteText = "THIS IS UPDATED";
        var updatedDate = createdOn.AddHours(1);
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);
        note.NoteText = updatedNoteText;
        note.ModifiedOn = updatedDate;

        // Act
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtNote = await dbContext.Notes.SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.NotNull(dqtNote);
            Assert.Null(dqtNote.FileName);
            Assert.Equal(createPersonResult.PersonId, dqtNote.PersonId);
            Assert.Equal(updatedNoteText, dqtNote.ContentHtml);
            Assert.Equal(updatedDate, dqtNote.UpdatedOn);
            Assert.Equal(note.CreatedBy.Id, dqtNote.CreatedByDqtUserId);
            Assert.Equal(createdByDqtUserName, dqtNote.CreatedByDqtUserName);
            Assert.Null(dqtNote.OriginalFileName);
        });
    }

    [Fact]
    public async Task Annotation_UpdatedBeforeTrsLastModifiedOn_DoesNotUpdateRecord()
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var createdByDqtUserName = Faker.Name.First();
        var updatedByDqtUserName = Faker.Name.First();
        var noteText = "this is a note";
        var annotationId = Guid.NewGuid();
        var attachmentFileName = default(string?);
        var mimeType = default(string?);
        var createPersonResult = await TestData.CreatePersonAsync();
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = default(Guid?);
        var updatedOn = default(DateTime?);
        var note = CreateAnnotationEntity(annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            updatedBy,
            updatedOn,
            createdByDqtUserName,
            updatedByDqtUserName,
            null);
        var updatedNoteText = "THIS WILL NOT BE UPDATED";

        // Act
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);
        updatedOn = createdOn.AddDays(-1);
        note.NoteText = updatedNoteText;
        note.ModifiedOn = updatedOn;
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtNote = await dbContext.Notes.SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.NotNull(dqtNote);
            Assert.Null(dqtNote.FileName);
            Assert.Equal(createPersonResult.PersonId, dqtNote.PersonId);
            Assert.Equal(createdOn, dqtNote.CreatedOn);
            Assert.Equal(noteText, dqtNote.ContentHtml);
            Assert.Equal(createdOn, dqtNote.UpdatedOn);
            Assert.Equal(note.CreatedBy.Id, dqtNote.CreatedByDqtUserId);
            Assert.Equal(createdByDqtUserName, dqtNote.CreatedByDqtUserName);
            Assert.Null(dqtNote.OriginalFileName);
        });
    }

    [Fact]
    public async Task Annotation_DeletedRecord_DeletesNoteRecordFromDatabase()
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var createdByDqtUserName = Faker.Name.First();
        var updatedByDqtUserName = Faker.Name.First();
        var noteText = "this is a note";
        var annotationId = Guid.NewGuid();
        var attachmentFileName = default(string?);
        var mimeType = default(string?);
        var createPersonResult = await TestData.CreatePersonAsync();
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = Guid.NewGuid();
        var updatedOn = Clock.UtcNow.AddDays(1);
        var note = CreateAnnotationEntity(annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            updatedBy,
            updatedOn,
            createdByDqtUserName,
            updatedByDqtUserName,
            null);
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Act
        await Helper.DeleteRecordsAsync(TrsDataSyncHelper.ModelTypes.DqtNote, new[] { note.Id });

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtnote = await dbContext.Notes.SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.Null(dqtnote);
        });
    }

    [Fact]
    public async Task SyncNoteAsync_NewRecordWithAttachment_WritesNewRowToDb()
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var createdByDqtUserName = Faker.Name.First();
        var updatedByDqtUserName = Faker.Name.First();
        var noteText = "this is a note";
        var annotationId = Guid.NewGuid();
        var attachmentFileName = "2x2.png";
        var mimeType = "image/png";
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = Guid.NewGuid();
        var updatedOn = default(DateTime?);
        var attachmentbase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFUlEQVQI12NgYGBg+M+ABBgAEJ8C/lL9enUAAAAASUVORK5CYII="; //2x2 red pixel image base64
        var createPersonResult = await TestData.CreatePersonAsync();
        var note = CreateAnnotationEntity(annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            updatedBy,
            updatedOn,
            createdByDqtUserName,
            updatedByDqtUserName,
            null);
        note.DocumentBody = attachmentbase64;

        // Act
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtNote = await dbContext.Notes.SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.NotNull(dqtNote);
            Assert.NotNull(dqtNote.FileName);
            Assert.Equal(createPersonResult.PersonId, dqtNote.PersonId);
            Assert.Equal(noteText, dqtNote.ContentHtml);
            //updatedon defaults to createdon
            Assert.Equal(note.CreatedOn, dqtNote.UpdatedOn);
            Assert.Equal(note.CreatedOn, dqtNote.CreatedOn);
            Assert.Equal(note.CreatedBy.Id, dqtNote.CreatedByDqtUserId);
            Assert.NotNull(dqtNote.OriginalFileName);
            Assert.Equal(attachmentFileName, dqtNote.OriginalFileName);
            Assert.Equal(createdByDqtUserName, dqtNote.CreatedByDqtUserName);
            BlobStorageFileService.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), note.Id), Times.Once());
            BlobStorageFileService.Verify(x => x.DeleteFileAsync(note.Id), Times.Never());
        });
    }

    [Fact]
    public async Task SyncNoteAsync_ExistingNoteRemovesAttachment_WritesNewRowToDb()
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var createdByDqtUserName = Faker.Name.First();
        var updatedByDqtUserName = Faker.Name.First();
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = Guid.NewGuid();
        var updatedOn = Clock.UtcNow.AddDays(1);
        var noteText = "this is a note";
        var annotationId = Guid.NewGuid();
        var attachmentFileName = "2x2.png";
        var mimeType = "image/png";
        var attachmentbase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFUlEQVQI12NgYGBg+M+ABBgAEJ8C/lL9enUAAAAASUVORK5CYII="; //2x2 red pixel image base64
        var createPersonResult = await TestData.CreatePersonAsync();
        var note = CreateAnnotationEntity(
            annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            null,
            null,
            createdByDqtUserName,
            updatedByDqtUserName,
            null);
        note.DocumentBody = attachmentbase64;
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);
        note.DocumentBody = null;
        note.MimeType = null;
        note.FileName = null;
        note.ModifiedBy = GetUserReference(updatedBy, updatedByDqtUserName);
        note.ModifiedOn = note.CreatedOn!.Value.AddHours(1);

        // Act
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtNote = await dbContext.Notes.AsNoTracking().SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.NotNull(dqtNote);
            Assert.Equal(createPersonResult.PersonId, dqtNote.PersonId);
            Assert.Equal(noteText, dqtNote.ContentHtml);
            Assert.Equal(note.ModifiedOn, dqtNote.UpdatedOn);
            Assert.Equal(note.CreatedOn, dqtNote.CreatedOn);
            Assert.Equal(note.CreatedBy.Id, dqtNote.CreatedByDqtUserId);
            Assert.Equal(note.ModifiedBy!.Id, dqtNote.UpdatedByDqtUserId);
            Assert.Null(dqtNote.OriginalFileName);
            Assert.Null(dqtNote.FileName);
            Assert.Equal(createdByDqtUserName, dqtNote.CreatedByDqtUserName);
            Assert.Equal(updatedByDqtUserName, dqtNote.UpdatedByDqtUserName);
            BlobStorageFileService.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), note.Id), Times.Once());
            BlobStorageFileService.Verify(x => x.DeleteFileAsync(note.Id), Times.Once());
        });
    }

    private Annotation CreateAnnotationEntity(
        Guid annotationId,
        Guid personId,
        string noteText,
        string? originalFileName,
        string? mimeType,
        Guid createdByDqtUserId,
        DateTime createdOn,
        Guid? updatedByDqtUserId,
        DateTime? updatedOn,
        string createdByDqtUserName,
        string? updatedByDqtUserName,
        string? subject
        )
    {
        var newAnnoation = new Annotation()
        {
            Id = annotationId,
            ObjectId = personId.ToEntityReference(Contact.EntityLogicalName),
            NoteText = noteText,
            FileName = originalFileName,
            MimeType = mimeType,
            CreatedBy = GetUserReference(createdByDqtUserId, createdByDqtUserName),
            CreatedOn = createdOn,
            ModifiedBy = GetUserReference(updatedByDqtUserId, updatedByDqtUserName),
            ModifiedOn = updatedOn,
            Subject = subject
        };
        return newAnnoation;
    }

    [Theory]
    [InlineData(".")]
    [InlineData("ManPay1205 letter suppressed")]
    [InlineData("Name amended in error by TP Update, name corrected to previous entry held by GTC")]
    [InlineData("\u00A3")] //£
    [InlineData("CC received")]
    [InlineData("DD mandate")]
    [InlineData("dereg, action")]
    [InlineData("fee paid")]
    [InlineData("PAYMENT")]
    [InlineData("ReQUESt ReFUnD")]
    [InlineData("THIS WILL MATCH ON ReQUESt ReFUnD")]
    public async Task SyncNoteAsync_NoteTextContainsIgnoredTerm_DoesNotInsertNote(string noteText)
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var createdByDqtUserName = Faker.Name.First();
        var updatedByDqtUserName = Faker.Name.First();
        var createdBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId;
        var createdOn = Clock.UtcNow;
        var updatedBy = Guid.NewGuid();
        var updatedOn = Clock.UtcNow.AddDays(1);
        var annotationId = Guid.NewGuid();
        var attachmentFileName = "2x2.png";
        var mimeType = "image/png";
        var attachmentbase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFUlEQVQI12NgYGBg+M+ABBgAEJ8C/lL9enUAAAAASUVORK5CYII="; //2x2 red pixel image base64
        var createPersonResult = await TestData.CreatePersonAsync();
        var note = CreateAnnotationEntity(
            annotationId,
            createPersonResult.PersonId,
            noteText,
            attachmentFileName,
            mimeType,
            createdBy,
            createdOn,
            null,
            null,
            createdByDqtUserName,
            updatedByDqtUserName,
            "Entered by REG");
        note.DocumentBody = attachmentbase64;
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);
        note.DocumentBody = null;
        note.MimeType = null;
        note.FileName = null;
        note.ModifiedBy = GetUserReference(updatedBy, updatedByDqtUserName);
        note.ModifiedOn = note.CreatedOn!.Value.AddHours(1);

        // Act
        await Helper.SyncAnnotationsAsync(new[] { note }, ignoreInvalid: true, dryRun: false, ct.Token);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var dqtNote = await dbContext.Notes.AsNoTracking().SingleOrDefaultAsync(p => p.NoteId == note.Id);
            Assert.Null(dqtNote);
        });
    }

    private EntityReference? GetUserReference(Guid? id, string? username)
    {
        if (id.HasValue)
        {
            var reference = new EntityReference(SystemUser.EntityLogicalName, id.Value)
            {
                Name = username
            };
            return reference;
        }
        return null;
    }
}
