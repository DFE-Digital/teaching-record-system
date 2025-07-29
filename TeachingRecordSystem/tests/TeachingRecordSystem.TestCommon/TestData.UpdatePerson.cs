using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
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
        private Guid? _personId = null;
        private (string FirstName, string? MiddleName, string LastName)? _updatedName = null;
        private bool? _contactsMigrated = null;

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

        public UpdatePersonBuilder AfterContactsMigrated(bool contactsMigrated = true)
        {
            if (_contactsMigrated is not null)
            {
                throw new InvalidOperationException("AfterContactsMigrated has already been set");
            }

            _contactsMigrated = contactsMigrated;
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
                if (_contactsMigrated is true)
                {
                    await testData.WithDbContextAsync(async dbContext =>
                    {
                        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == _personId.Value);
                        var now = testData.Clock.UtcNow;

                        var updatePersonResult = person.UpdateDetails(
                            _updatedName.Value.FirstName,
                            _updatedName.Value.MiddleName ?? string.Empty,
                            _updatedName.Value.LastName,
                            person.DateOfBirth,
                            (EmailAddress?)person.EmailAddress,
                            (NationalInsuranceNumber?)person.NationalInsuranceNumber,
                            person.Gender,
                            now);

                        var updatedEvent = updatePersonResult.Changes != 0 ?
                            new PersonDetailsUpdatedEvent
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
                                Changes = PersonDetailsUpdatedEventChanges.None
                            } :
                            null;

                        if (updatedEvent?.Changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange) == true)
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
                else
                {
                    await testData.OrganizationService.ExecuteAsync(new UpdateRequest()
                    {
                        Target = new Contact()
                        {
                            Id = _personId!.Value,
                            FirstName = _updatedName.Value.FirstName,
                            MiddleName = _updatedName.Value.MiddleName,
                            LastName = _updatedName.Value.LastName
                        }
                    });

                    await testData.WithDbContextAsync(async dbContext =>
                    {
                        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == _personId);
                        person.FirstName = _updatedName.Value.FirstName;
                        person.MiddleName = _updatedName.Value.MiddleName ?? "";
                        person.LastName = _updatedName.Value.LastName;
                        await dbContext.SaveChangesAsync();
                    });
                }
            }
        }
    }
}
