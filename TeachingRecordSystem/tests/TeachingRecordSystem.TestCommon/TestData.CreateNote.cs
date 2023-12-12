using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task CreateNote(Action<CreateNoteBuilder>? configure)
    {
        var builder = new CreateNoteBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreateNoteBuilder
    {
        private Guid? _personId = null;
        private string? _subject = null;
        private string? _description = null;

        public CreateNoteBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null && _personId != personId)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public CreateNoteBuilder WithSubject(string subject)
        {
            if (_subject is not null && _subject != subject)
            {
                throw new InvalidOperationException("WithSubject has already been set");
            }

            _subject = subject;
            return this;
        }

        public CreateNoteBuilder WithDescription(string description)
        {
            if (_description is not null && _description != description)
            {
                throw new InvalidOperationException("WithDescription has already been set");
            }

            _description = description;
            return this;
        }

        public async Task Execute(TestData testData)
        {
            if (_personId is null)
            {
                throw new InvalidOperationException("WithPersonId has not been set");
            }

            var subject = _subject ?? "Test Note";
            var description = _description ?? "Test Note Description";

            await testData.OrganizationService.ExecuteAsync(new CreateRequest()
            {
                Target = new Annotation()
                {
                    Subject = subject,
                    NoteText = description,
                    ObjectId = _personId.Value.ToEntityReference(Contact.EntityLogicalName),
                    ObjectTypeCode = Contact.EntityLogicalName
                }
            });
        }
    }
}
