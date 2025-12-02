using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task UpdatePersonAsync(Action<UpdatePersonBuilder>? configure)
    {
        var builder = new UpdatePersonBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class UpdatePersonBuilder
    {
        private Guid? _personId;
        private (string FirstName, string? MiddleName, string LastName)? _updatedName;

        public UpdatePersonBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public UpdatePersonBuilder WithUpdatedName(string firstName, string? middleName, string lastName)
        {
            if (_updatedName is not null)
            {
                throw new InvalidOperationException("WithUpdatedName has already been set");
            }

            _updatedName = (firstName, middleName, lastName);
            return this;
        }

        public async Task ExecuteAsync(TestData testData)
        {
            if (_personId is null)
            {
                throw new InvalidOperationException("WithPersonId has not been set");
            }

            if (_updatedName is not null)
            {
                var now = testData.Clock.UtcNow;

                await testData.WithDbContextAsync(async dbContext =>
                {
                    var person = await dbContext.Persons.SingleAsync(p => p.PersonId == _personId.Value);

                    var updatePersonResult = person.UpdateDetails(
                        Option.Some(_updatedName.Value.FirstName),
                        Option.Some(_updatedName.Value.MiddleName ?? string.Empty),
                        Option.Some(_updatedName.Value.LastName),
                        Option.Some(person.DateOfBirth),
                        Option.Some((EmailAddress?)person.EmailAddress),
                        Option.Some((NationalInsuranceNumber?)person.NationalInsuranceNumber),
                        Option.Some(person.Gender),
                        now);

                    var updatedEvent = updatePersonResult.Changes != 0 ?
                        new LegacyEvents.PersonDetailsUpdatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            CreatedUtc = now,
                            RaisedBy = SystemUser.SystemUserId,
                            PersonId = person.PersonId,
                            PersonAttributes = updatePersonResult.PersonAttributes,
                            OldPersonAttributes = updatePersonResult.OldPersonAttributes,
                            NameChangeReason = null,
                            NameChangeEvidenceFile = null,
                            DetailsChangeReason = null,
                            DetailsChangeReasonDetail = null,
                            DetailsChangeEvidenceFile = null,
                            Changes = (LegacyEvents.PersonDetailsUpdatedEventChanges)updatePersonResult.Changes
                        } :
                        null;

                    if (updatedEvent?.Changes.HasAnyFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.NameChange) == true)
                    {
                        dbContext.PreviousNames.Add(new PreviousName
                        {
                            PreviousNameId = Guid.NewGuid(),
                            PersonId = _personId.Value,
                            FirstName = updatedEvent.OldPersonAttributes.FirstName,
                            MiddleName = updatedEvent.OldPersonAttributes.MiddleName,
                            LastName = updatedEvent.OldPersonAttributes.LastName,
                            CreatedOn = now,
                            UpdatedOn = now
                        });
                    }

                    await dbContext.SaveChangesAsync();
                });
            }
        }
    }
}
