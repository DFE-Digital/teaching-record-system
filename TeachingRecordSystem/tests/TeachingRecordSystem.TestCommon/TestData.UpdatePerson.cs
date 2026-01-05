using Optional;
using TeachingRecordSystem.Core.Services.Persons;
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
        private (string FirstName, string? MiddleName, string LastName, PersonNameChangeReason NameChangeReason)? _updatedName;

        public UpdatePersonBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public UpdatePersonBuilder WithUpdatedName(string firstName, string? middleName, string lastName, PersonNameChangeReason nameChangeReason)
        {
            if (_updatedName is not null)
            {
                throw new InvalidOperationException("WithUpdatedName has already been set");
            }

            _updatedName = (firstName, middleName, lastName, nameChangeReason);
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
                    var personService = new PersonService(dbContext, testData.Clock, new TestTrnGenerator(testData.DbContextFactory), new TestEventPublisher());
                    var person = await dbContext.Persons.SingleAsync(p => p.PersonId == _personId.Value);
                    var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, now, SystemUser.SystemUserId);

                    var updatePersonResult = await personService.UpdatePersonDetailsAsync(new(
                        _personId.Value,
                        new()
                        {
                            FirstName = Option.Some(_updatedName.Value.FirstName),
                            MiddleName = Option.Some(_updatedName.Value.MiddleName ?? string.Empty),
                            LastName = Option.Some(_updatedName.Value.LastName),
                            DateOfBirth = Option.Some(person.DateOfBirth),
                            EmailAddress = Option.Some((EmailAddress?)person.EmailAddress),
                            NationalInsuranceNumber = Option.Some((NationalInsuranceNumber?)person.NationalInsuranceNumber),
                            Gender = Option.Some(person.Gender),
                        },
                        new Justification<PersonNameChangeReason> { Reason = _updatedName.Value.NameChangeReason },
                        new Justification<PersonDetailsChangeReason> { Reason = PersonDetailsChangeReason.AnotherReason }),
                        processContext);

                    //var updatedEvent = updatePersonResult.Changes != 0 ?
                    //    new LegacyEvents.PersonDetailsUpdatedEvent
                    //    {
                    //        EventId = Guid.NewGuid(),
                    //        CreatedUtc = now,
                    //        RaisedBy = SystemUser.SystemUserId,
                    //        PersonId = person.PersonId,
                    //        PersonAttributes = updatePersonResult.PersonAttributes,
                    //        OldPersonAttributes = updatePersonResult.OldPersonAttributes,
                    //        NameChangeReason = null,
                    //        NameChangeEvidenceFile = null,
                    //        DetailsChangeReason = null,
                    //        DetailsChangeReasonDetail = null,
                    //        DetailsChangeEvidenceFile = null,
                    //        Changes = (LegacyEvents.PersonDetailsUpdatedEventChanges)updatePersonResult.Changes
                    //    } :
                    //    null;

                    //if (updatedEvent?.Changes.HasAnyFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.NameChange) == true)
                    //{
                    //    dbContext.PreviousNames.Add(new PreviousName
                    //    {
                    //        PreviousNameId = Guid.NewGuid(),
                    //        PersonId = _personId.Value,
                    //        FirstName = updatedEvent.OldPersonAttributes.FirstName,
                    //        MiddleName = updatedEvent.OldPersonAttributes.MiddleName,
                    //        LastName = updatedEvent.OldPersonAttributes.LastName,
                    //        CreatedOn = now,
                    //        UpdatedOn = now
                    //    });
                    //}

                    //await dbContext.SaveChangesAsync();
                });
            }
        }
    }
}
