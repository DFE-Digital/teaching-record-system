using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<Note> CreateNoteAsync(Action<CreateNoteBuilder>? configure)
    {
        var builder = new CreateNoteBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateNoteBuilder
    {
        private Guid? _personId;
        private string? _content;

        public CreateNoteBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null && _personId != personId)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public CreateNoteBuilder WithContent(string content)
        {
            if (_content is not null && _content != content)
            {
                throw new InvalidOperationException("WithDescription has already been set");
            }

            _content = content;
            return this;
        }

        public Task<Note> ExecuteAsync(TestData testData)
        {
            if (_personId is null)
            {
                throw new InvalidOperationException("WithPersonId has not been set");
            }

            var content = _content ?? Faker.Lorem.Paragraph();

            return testData.WithDbContextAsync(async dbContext =>
            {
                var now = testData.Clock.UtcNow;

                var note = new Note
                {
                    NoteId = Guid.NewGuid(),
                    PersonId = _personId.Value,
                    Content = content,
                    ContentHtml = null,
                    UpdatedOn = now,
                    CreatedOn = now,
                    CreatedByUserId = SystemUser.SystemUserId,
                    CreatedByDqtUserId = null,
                    CreatedByDqtUserName = null,
                    UpdatedByDqtUserId = null,
                    UpdatedByDqtUserName = null,
                    FileId = null,
                    OriginalFileName = null
                };

                dbContext.Notes.Add(note);

                await dbContext.SaveChangesAsync();
                dbContext.Entry(note).State = EntityState.Detached;

                return note;
            });
        }
    }
}
