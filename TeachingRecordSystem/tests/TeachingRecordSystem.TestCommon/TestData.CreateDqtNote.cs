using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.TestCommon
{
    public partial class TestData
    {
        public Task<DqtNote> CreateDqtNoteAsync(Guid personId, string noteText, Guid createdByUserId, string? createdByUserName, string? originalFileName, Guid? fileName, DateTime? createDate = null) => WithDbContextAsync(async dbContext =>
        {
            var note = new DqtNote()
            {
                Id = Guid.NewGuid(),
                PersonId = personId,
                NoteText = noteText,
                CreatedByDqtUserName = createdByUserName ?? Faker.Name.FullName(),
                CreatedByDqtUserId = createdByUserId,
                CreatedOn = createDate ?? Clock.UtcNow,
                UpdatedByDqtUserId = null,
                UpdatedOn = null,
                UpdatedByDqtUserName = null,
                OriginalFileName = originalFileName,
                FileName = fileName?.ToString()
            };

            dbContext.DqtNotes.Add(note);
            await dbContext.SaveChangesAsync();

            return note;
        });
    }
}
