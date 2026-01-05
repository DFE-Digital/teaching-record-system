using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Core.Tests.Services.Persons;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class PersonServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreatePersonAsync_WithCreatePersonViaSupportUIOptions_CreatesPersonAndPublishesEvent()
    {
        // Arrange
        var personDetails = new PersonDetails()
        {
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = EmailAddress.Parse((string?)"test@test.com"),
            NationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C"),
            Gender = Gender.Female,
        };

        var justification = new Justification<PersonCreateReason>()
        {
            Reason = PersonCreateReason.AnotherReason,
            ReasonDetail = "Some details",
            Evidence = new()
            {
                FileId = Guid.NewGuid(),
                Name = "filename.png"
            }
        };

        var options = new CreatePersonViaSupportUiOptions(personDetails, justification);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var person = await WithServiceAsync(s => s.CreatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.CreatedOn);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.UpdatedOn);
            Assert.Equal(personDetails.FirstName, createdPersonRecord.FirstName);
            Assert.Equal(personDetails.MiddleName, createdPersonRecord.MiddleName);
            Assert.Equal(personDetails.LastName, createdPersonRecord.LastName);
            Assert.Equal(personDetails.DateOfBirth, createdPersonRecord.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), createdPersonRecord.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), createdPersonRecord.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, createdPersonRecord.Gender);
            Assert.Null(createdPersonRecord.SourceTrnRequestId);
            Assert.Null(createdPersonRecord.SourceApplicationUserId);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonCreatedEvent>(e);
            Assert.Equal(person.PersonId, @event.PersonId);
            Assert.Equal(personDetails.FirstName, @event.Details.FirstName);
            Assert.Equal(personDetails.MiddleName, @event.Details.MiddleName);
            Assert.Equal(personDetails.LastName, @event.Details.LastName);
            Assert.Equal(personDetails.DateOfBirth, @event.Details.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), @event.Details.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), @event.Details.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, @event.Details.Gender);
            Assert.Equal(justification.Reason.GetDisplayName(), @event.CreateReason);
            Assert.Equal(justification.ReasonDetail, @event.CreateReasonDetail);
            Assert.Equal(justification.Evidence.FileId, @event.EvidenceFile!.FileId);
            Assert.Equal(justification.Evidence.Name, @event.EvidenceFile.Name);
        });
    }

    [Fact]
    public async Task CreatePersonAsync_WithCreatePersonViaTrnRequestOptions_WhenSourceRequestDoesNotExist_Throws()
    {
        // Arrange
        var personDetails = new PersonDetails()
        {
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = EmailAddress.Parse((string?)"test@test.com"),
            NationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C"),
            Gender = Gender.Female,
        };
        var sourceTrnRequest = (SystemUser.SystemUserId, Guid.NewGuid().ToString());

        var options = new CreatePersonViaTrnRequestOptions(personDetails, sourceTrnRequest);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.CreatePersonAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task CreatePersonAsync_WithCreatePersonViaTrnRequestOptions_SetsSourceRequestPropertiesOnPersonAndEvent()
    {
        // Arrange
        var personDetails = new PersonDetails()
        {
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = EmailAddress.Parse((string?)"test@test.com"),
            NationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C"),
            Gender = Gender.Female,
        };
        var trnRequestMetadata = new TrnRequestMetadata()
        {
            ApplicationUserId = SystemUser.SystemUserId,
            RequestId = Guid.NewGuid().ToString(),
            CreatedOn = Clock.UtcNow,
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Name = new[] { personDetails.FirstName, personDetails.MiddleName, personDetails.LastName }.GetNonEmptyValues(),
            FirstName = personDetails.FirstName,
            MiddleName = personDetails.MiddleName,
            LastName = personDetails.LastName,
            PreviousFirstName = null,
            PreviousLastName = null,
            DateOfBirth = personDetails.DateOfBirth!.Value!,
            EmailAddress = personDetails.EmailAddress?.ToString(),
            NationalInsuranceNumber = personDetails.NationalInsuranceNumber.ToString(),
            Gender = personDetails.Gender,
        };
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
            await dbContext.SaveChangesAsync();
        });

        var options = new CreatePersonViaTrnRequestOptions(personDetails, (trnRequestMetadata.ApplicationUserId, trnRequestMetadata.RequestId));
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var person = await WithServiceAsync(s => s.CreatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.CreatedOn);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.UpdatedOn);
            Assert.Equal(personDetails.FirstName, createdPersonRecord.FirstName);
            Assert.Equal(personDetails.MiddleName, createdPersonRecord.MiddleName);
            Assert.Equal(personDetails.LastName, createdPersonRecord.LastName);
            Assert.Equal(personDetails.DateOfBirth, createdPersonRecord.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), createdPersonRecord.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), createdPersonRecord.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, createdPersonRecord.Gender);
            Assert.Equal(trnRequestMetadata.RequestId, createdPersonRecord.SourceTrnRequestId);
            Assert.Equal(trnRequestMetadata.ApplicationUserId, createdPersonRecord.SourceApplicationUserId);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonCreatedEvent>(e);
            Assert.Equal(person.PersonId, @event.PersonId);
            Assert.Equal(personDetails.FirstName, @event.Details.FirstName);
            Assert.Equal(personDetails.MiddleName, @event.Details.MiddleName);
            Assert.Equal(personDetails.LastName, @event.Details.LastName);
            Assert.Equal(personDetails.DateOfBirth, @event.Details.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), @event.Details.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), @event.Details.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, @event.Details.Gender);
            Assert.Null(@event.CreateReason);
            Assert.Null(@event.CreateReasonDetail);
            Assert.Null(@event.EvidenceFile);
        });
    }

    [Fact]
    public async Task CreatePersonAsync_WithCreatePersonViaTpsImportOptions_WhenSourceRequestDoesNotExist_Throws()
    {
        // Arrange
        var trn = "1234567";
        var personDetails = new PersonDetails()
        {
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = EmailAddress.Parse((string?)"test@test.com"),
            NationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C"),
            Gender = Gender.Female,
        };
        var sourceTrnRequest = (SystemUser.SystemUserId, Guid.NewGuid().ToString());

        var options = new CreatePersonViaTpsImportOptions(trn, personDetails, sourceTrnRequest);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.CreatePersonAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task CreatePersonAsync_WithCreatePersonViaTpsImportOptions_SetsSourceRequestPropertiesOnPersonAndEvent()
    {
        // Arrange
        var trn = "1234567";
        var personDetails = new PersonDetails()
        {
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = EmailAddress.Parse((string?)"test@test.com"),
            NationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C"),
            Gender = Gender.Female,
        };
        var trnRequestMetadata = new TrnRequestMetadata()
        {
            ApplicationUserId = SystemUser.SystemUserId,
            RequestId = Guid.NewGuid().ToString(),
            CreatedOn = Clock.UtcNow,
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Name = new[] { personDetails.FirstName, personDetails.MiddleName, personDetails.LastName }.GetNonEmptyValues(),
            FirstName = personDetails.FirstName,
            MiddleName = personDetails.MiddleName,
            LastName = personDetails.LastName,
            PreviousFirstName = null,
            PreviousLastName = null,
            DateOfBirth = personDetails.DateOfBirth!.Value!,
            EmailAddress = personDetails.EmailAddress?.ToString(),
            NationalInsuranceNumber = personDetails.NationalInsuranceNumber.ToString(),
            Gender = personDetails.Gender,
        };
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
            await dbContext.SaveChangesAsync();
        });

        var options = new CreatePersonViaTpsImportOptions(trn, personDetails, (trnRequestMetadata.ApplicationUserId, trnRequestMetadata.RequestId));
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var person = await WithServiceAsync(s => s.CreatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(trn, createdPersonRecord.Trn);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.CreatedOn);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.UpdatedOn);
            Assert.Equal(personDetails.FirstName, createdPersonRecord.FirstName);
            Assert.Equal(personDetails.MiddleName, createdPersonRecord.MiddleName);
            Assert.Equal(personDetails.LastName, createdPersonRecord.LastName);
            Assert.Equal(personDetails.DateOfBirth, createdPersonRecord.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), createdPersonRecord.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), createdPersonRecord.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, createdPersonRecord.Gender);
            Assert.Equal(trnRequestMetadata.RequestId, createdPersonRecord.SourceTrnRequestId);
            Assert.Equal(trnRequestMetadata.ApplicationUserId, createdPersonRecord.SourceApplicationUserId);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonCreatedEvent>(e);
            Assert.Equal(person.PersonId, @event.PersonId);
            Assert.Equal(personDetails.FirstName, @event.Details.FirstName);
            Assert.Equal(personDetails.MiddleName, @event.Details.MiddleName);
            Assert.Equal(personDetails.LastName, @event.Details.LastName);
            Assert.Equal(personDetails.DateOfBirth, @event.Details.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), @event.Details.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), @event.Details.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, @event.Details.Gender);
            Assert.Null(@event.CreateReason);
            Assert.Null(@event.CreateReasonDetail);
            Assert.Null(@event.EvidenceFile);
        });
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName)]
    [InlineData(PersonDetailsUpdatedEventChanges.MiddleName)]
    [InlineData(PersonDetailsUpdatedEventChanges.LastName)]
    [InlineData(PersonDetailsUpdatedEventChanges.DateOfBirth)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber)]
    [InlineData(PersonDetailsUpdatedEventChanges.Gender)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber | PersonDetailsUpdatedEventChanges.Gender)]
    public async Task UpdatePersonDetailsAsync_UpdatesPersonDetailsAndPublishesEvent(PersonDetailsUpdatedEventChanges changes)
    {
        // Arrange
        var personToUpdate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Other));

        var personDetails = new PersonDetails
        {
            FirstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Jim" : personToUpdate.FirstName,
            MiddleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "A" : personToUpdate.MiddleName,
            LastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Person" : personToUpdate.LastName,
            DateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("3 July 1990") : personToUpdate.DateOfBirth,
            EmailAddress = EmailAddress.Parse(changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email.com" : personToUpdate.EmailAddress),
            NationalInsuranceNumber = NationalInsuranceNumber.Parse(changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "JK987654D" : personToUpdate.NationalInsuranceNumber!),
            Gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : personToUpdate.Gender
        };

        var nameChangeJustification = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange)
            ? new Justification<PersonNameChangeReason>()
            {
                Reason = PersonNameChangeReason.CorrectingAnError,
                ReasonDetail = "Some details",
                Evidence = new()
                {
                    FileId = Guid.NewGuid(),
                    Name = "name-evidence.pdf"
                }
            }
            : null;

        var detailsChangeJustification = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange)
            ? new Justification<PersonDetailsChangeReason>()
            {
                Reason = PersonDetailsChangeReason.AnotherReason,
                ReasonDetail = "Some more details",
                Evidence = new()
                {
                    FileId = Guid.NewGuid(),
                    Name = "other-evidence.pdf"
                }
            }
            : null;

        var options = new UpdatePersonDetailsOptions(personToUpdate.PersonId, personDetails.UpdateAll(), nameChangeJustification, detailsChangeJustification);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == personToUpdate.PersonId);
            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal(personDetails.FirstName, updatedPersonRecord.FirstName);
            Assert.Equal(personDetails.MiddleName, updatedPersonRecord.MiddleName);
            Assert.Equal(personDetails.LastName, updatedPersonRecord.LastName);
            Assert.Equal(personDetails.DateOfBirth, updatedPersonRecord.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), updatedPersonRecord.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), updatedPersonRecord.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, updatedPersonRecord.Gender);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonDetailsUpdatedEvent>(e);
            Assert.Equal(personToUpdate.PersonId, @event.PersonId);
            Assert.Equal(personToUpdate.FirstName, @event.OldPersonDetails.FirstName);
            Assert.Equal(personToUpdate.MiddleName, @event.OldPersonDetails.MiddleName);
            Assert.Equal(personToUpdate.LastName, @event.OldPersonDetails.LastName);
            Assert.Equal(personToUpdate.DateOfBirth, @event.OldPersonDetails.DateOfBirth);
            Assert.Equal(personToUpdate.EmailAddress, @event.OldPersonDetails.EmailAddress);
            Assert.Equal(personToUpdate.NationalInsuranceNumber, @event.OldPersonDetails.NationalInsuranceNumber);
            Assert.Equal(personToUpdate.Gender, @event.OldPersonDetails.Gender);
            Assert.Equal(personDetails.FirstName, @event.PersonDetails.FirstName);
            Assert.Equal(personDetails.MiddleName, @event.PersonDetails.MiddleName);
            Assert.Equal(personDetails.LastName, @event.PersonDetails.LastName);
            Assert.Equal(personDetails.DateOfBirth, @event.PersonDetails.DateOfBirth);
            Assert.Equal(personDetails.EmailAddress?.ToString(), @event.PersonDetails.EmailAddress);
            Assert.Equal(personDetails.NationalInsuranceNumber?.ToString(), @event.PersonDetails.NationalInsuranceNumber);
            Assert.Equal(personDetails.Gender, @event.PersonDetails.Gender);
            Assert.Equal(nameChangeJustification?.Reason.GetDisplayName(), @event.NameChangeReason);
            Assert.Equal(nameChangeJustification?.Evidence?.FileId, @event.NameChangeEvidenceFile?.FileId);
            Assert.Equal(nameChangeJustification?.Evidence?.Name, @event.NameChangeEvidenceFile?.Name);
            Assert.Equal(detailsChangeJustification?.Reason.GetDisplayName(), @event.DetailsChangeReason);
            Assert.Equal(detailsChangeJustification?.ReasonDetail, @event.DetailsChangeReasonDetail);
            Assert.Equal(detailsChangeJustification?.Evidence?.FileId, @event.DetailsChangeEvidenceFile?.FileId);
            Assert.Equal(detailsChangeJustification?.Evidence?.Name, @event.DetailsChangeEvidenceFile?.Name);
        });
    }

    [Fact]
    public async Task UpdatePersonDetailsAsync_WhenAnyNameFieldChanged_AndNameChangeReasonIsCorrectingAnError_DoesNotUpdatePersonPreviousNames()
    {
        // Arrange
        var ethelredDate = DateTime.Parse("1 Jan 1990").ToUniversalTime();
        var conanDate = DateTime.Parse("1 Jan 1991").ToUniversalTime();
        var personToUpdate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithPreviousNames(("Ethelred", "The", "Unready", ethelredDate), ("Conan", "The", "Barbarian", conanDate))
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var personDetails = new PersonDetails
        {
            FirstName = "Alfrede",
            MiddleName = "Thee",
            LastName = "Greate",
            DateOfBirth = personToUpdate.DateOfBirth,
            EmailAddress = personToUpdate.EmailAddress != null ? EmailAddress.Parse(personToUpdate.EmailAddress) : null,
            NationalInsuranceNumber = personToUpdate.NationalInsuranceNumber != null ? NationalInsuranceNumber.Parse(personToUpdate.NationalInsuranceNumber) : null,
            Gender = personToUpdate.Gender
        };

        var nameChangeJustification = new Justification<PersonNameChangeReason>()
        {
            Reason = PersonNameChangeReason.CorrectingAnError,
            ReasonDetail = null,
            Evidence = null
        };

        var options = new UpdatePersonDetailsOptions(personToUpdate.PersonId, personDetails.UpdateAll(), nameChangeJustification, null);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .Include(p => p.PreviousNames)
                .SingleAsync(p => p.PersonId == personToUpdate.PersonId);

            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal("Alfrede", updatedPersonRecord.FirstName);
            Assert.Equal("Thee", updatedPersonRecord.MiddleName);
            Assert.Equal("Greate", updatedPersonRecord.LastName);

            Assert.Collection(updatedPersonRecord.PreviousNames!.OrderByDescending(pn => pn.CreatedOn),
                pn => Assert.Equal(("Conan", "The", "Barbarian", conanDate, conanDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Ethelred", "The", "Unready", ethelredDate, ethelredDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)));
        });
    }

    [Theory]
    [InlineData(PersonNameChangeReason.DeedPollOrOtherLegalProcess)]
    [InlineData(PersonNameChangeReason.MarriageOrCivilPartnership)]
    public async Task UpdatePersonDetailsAsync_WhenAnyNameFieldChanged_AndNameChangeReasonIsFormalNameChange_UpdatesPersonPreviousNames(PersonNameChangeReason reason)
    {
        // Arrange
        var ethelredDate = DateTime.Parse("1 Jan 1990").ToUniversalTime();
        var conanDate = DateTime.Parse("1 Jan 1991").ToUniversalTime();
        var personToUpdate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithPreviousNames(("Ethelred", "The", "Unready", ethelredDate), ("Conan", "The", "Barbarian", conanDate))
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var personDetails = new PersonDetails
        {
            FirstName = "Megan",
            MiddleName = "Thee",
            LastName = "Stallion",
            DateOfBirth = personToUpdate.DateOfBirth,
            EmailAddress = personToUpdate.EmailAddress != null ? EmailAddress.Parse(personToUpdate.EmailAddress) : null,
            NationalInsuranceNumber = personToUpdate.NationalInsuranceNumber != null ? NationalInsuranceNumber.Parse(personToUpdate.NationalInsuranceNumber) : null,
            Gender = personToUpdate.Gender
        };

        var nameChangeJustification = new Justification<PersonNameChangeReason>()
        {
            Reason = reason,
            ReasonDetail = null,
            Evidence = null
        };

        var options = new UpdatePersonDetailsOptions(personToUpdate.PersonId, personDetails.UpdateAll(), nameChangeJustification, null);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .Include(p => p.PreviousNames)
                .SingleAsync(p => p.PersonId == personToUpdate.PersonId);

            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal("Megan", updatedPersonRecord.FirstName);
            Assert.Equal("Thee", updatedPersonRecord.MiddleName);
            Assert.Equal("Stallion", updatedPersonRecord.LastName);

            Assert.Collection(updatedPersonRecord.PreviousNames!.OrderByDescending(pn => pn.CreatedOn),
                pn => Assert.Equal(("Alfred", "The", "Great", Clock.UtcNow, Clock.UtcNow), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Conan", "The", "Barbarian", conanDate, conanDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Ethelred", "The", "Unready", ethelredDate, ethelredDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)));
        });
    }

    [Fact]
    public async Task UpdatePersonDetailsAsync_WhenNothingChanged_DoesNotUpdatePersonOrPublishEvent()
    {
        // Arrange
        var timeOfCreation = Clock.UtcNow;

        var personToUpdate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Other));

        Clock.Advance();

        var personDetails = new PersonDetails
        {
            FirstName = personToUpdate.FirstName,
            MiddleName = personToUpdate.MiddleName,
            LastName = personToUpdate.LastName,
            DateOfBirth = personToUpdate.DateOfBirth,
            EmailAddress = personToUpdate.EmailAddress != null ? EmailAddress.Parse(personToUpdate.EmailAddress) : null,
            NationalInsuranceNumber = personToUpdate.NationalInsuranceNumber != null ? NationalInsuranceNumber.Parse(personToUpdate.NationalInsuranceNumber) : null,
            Gender = personToUpdate.Gender
        };

        var options = new UpdatePersonDetailsOptions(personToUpdate.PersonId, personDetails.UpdateAll(), null, null);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .SingleAsync(p => p.PersonId == personToUpdate.PersonId);

            Assert.Equal(timeOfCreation, updatedPersonRecord.UpdatedOn);
        });

        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task DeactivatePersonAsync_UpdatesPersonStatusAndPublishesEvent()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink"));

        var justification = new Justification<PersonDeactivateReason>()
        {
            Reason = PersonDeactivateReason.AnotherReason,
            ReasonDetail = "Some detail",
            Evidence = new()
            {
                FileId = Guid.NewGuid(),
                Name = "evidence.pdf"
            }
        };

        var options = new DeactivatePersonOptions(personToDeactivate.PersonId, justification);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.DeactivatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .IgnoreQueryFilters()
                .SingleAsync(p => p.PersonId == personToDeactivate.PersonId);
            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal(PersonStatus.Deactivated, updatedPersonRecord.Status);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonDeactivatedEvent>(e);

            Assert.Equal(personToDeactivate.PersonId, @event.PersonId);
            Assert.Null(@event.MergedWithPersonId);
            Assert.Equal(justification?.Reason.GetDisplayName(), @event.Reason);
            Assert.Equal(justification?.ReasonDetail, @event.ReasonDetail);
            Assert.Equal(justification?.Evidence?.FileId, @event.EvidenceFile!.FileId);
            Assert.Equal(justification?.Evidence?.Name, @event.EvidenceFile.Name);
        });
    }

    [Fact]
    public async Task DeactivatePersonAsync_PersonIsAlreadyDeactivated_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToDeactivate.Person);
            personToDeactivate.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new DeactivatePersonOptions(personToDeactivate.PersonId, new() { Reason = PersonDeactivateReason.AnotherReason });

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task ReactivatePersonAsync_UpdatesPersonStatusAndPublishesEvent()
    {
        // Arrange
        var personToReactivate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink"));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToReactivate.Person);
            personToReactivate.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var justification = new Justification<PersonReactivateReason>()
        {
            Reason = PersonReactivateReason.AnotherReason,
            ReasonDetail = "Some detail",
            Evidence = new()
            {
                FileId = Guid.NewGuid(),
                Name = "evidence.pdf"
            }
        };

        var options = new ReactivatePersonOptions(personToReactivate.PersonId, justification);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ReactivatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .IgnoreQueryFilters()
                .SingleAsync(p => p.PersonId == personToReactivate.PersonId);
            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal(PersonStatus.Active, updatedPersonRecord.Status);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonReactivatedEvent>(e);

            Assert.Equal(personToReactivate.PersonId, @event.PersonId);
            Assert.Equal(justification?.Reason.GetDisplayName(), @event.Reason);
            Assert.Equal(justification?.ReasonDetail, @event.ReasonDetail);
            Assert.Equal(justification?.Evidence?.FileId, @event.EvidenceFile!.FileId);
            Assert.Equal(justification?.Evidence?.Name, @event.EvidenceFile.Name);
        });
    }

    [Fact]
    public async Task ReactivatePersonAsync_PersonIsAlreadyActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToReactivate = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new ReactivatePersonOptions(personToReactivate.PersonId, new() { Reason = PersonReactivateReason.AnotherReason });

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.ReactivatePersonAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task DeactivatePersonViaMergeAsync_PersonToDeactivateIsAlreadyDeactivated_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToDeactivate.Person);
            personToDeactivate.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new DeactivatePersonViaMergeOptions(personToDeactivate.PersonId, personToRetain.PersonId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task DeactivatePersonViaMergeAsync_ValidRequest_UpdatesPersonStatusAndPublishesEvent()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new DeactivatePersonViaMergeOptions(personToDeactivate.PersonId, personToRetain.PersonId);

        // Act
        await WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var deactivatedPerson = await dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == personToDeactivate.PersonId);
            Assert.Equal(PersonStatus.Deactivated, deactivatedPerson.Status);
            Assert.Equal(personToRetain.PersonId, deactivatedPerson.MergedWithPersonId);
        });

        Events.AssertEventsPublished(e =>
        {
            var personDeactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(e);
            Assert.Equal(personToDeactivate.PersonId, personDeactivatedEvent.PersonId);
            Assert.Equal(personToRetain.PersonId, personDeactivatedEvent.MergedWithPersonId);
        });
    }

    //[Fact]
    //public async Task MergePersonsAsync_PersonToDeactivateIsAlreadyDeactivated_ThrowsInvalidOperationException()
    //{
    //    // Arrange
    //    var personToDeactivate = await TestData.CreatePersonAsync();
    //    var personToRetain = await TestData.CreatePersonAsync();

    //    await WithDbContextAsync(async dbContext =>
    //    {
    //        dbContext.Attach(personToDeactivate.Person);
    //        personToDeactivate.Person.Status = PersonStatus.Deactivated;
    //        await dbContext.SaveChangesAsync();
    //    });

    //    var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

    //    var options = new MergePersonsOptions(personToDeactivate.PersonId, personToRetain.PersonId);

    //    // Act
    //    var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.MergePersonsAsync(options, processContext)));

    //    // Assert
    //    Assert.IsType<InvalidOperationException>(ex);
    //}

    //[Fact]
    //public async Task MergePersonsAsync_ValidRequest_UpdatesPersonStatusAndPublishesEvent()
    //{
    //    // Arrange
    //    var personToDeactivate = await TestData.CreatePersonAsync();
    //    var personToRetain = await TestData.CreatePersonAsync();

    //    var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

    //    var options = new MergePersonsOptions(personToDeactivate.PersonId, personToRetain.PersonId);

    //    // Act
    //    await WithServiceAsync(s => s.MergePersonsAsync(options, processContext));

    //    // Assert
    //    await WithDbContextAsync(async dbContext =>
    //    {
    //        var deactivatedPerson = await dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == personToDeactivate.PersonId);
    //        Assert.Equal(PersonStatus.Deactivated, deactivatedPerson.Status);
    //        Assert.Equal(personToRetain.PersonId, deactivatedPerson.MergedWithPersonId);
    //    });

    //    Events.AssertEventsPublished(e =>
    //    {
    //        var personDeactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(e);
    //        Assert.Equal(personToDeactivate.PersonId, personDeactivatedEvent.PersonId);
    //        Assert.Equal(personToRetain.PersonId, personDeactivatedEvent.MergedWithPersonId);
    //    });
    //}

    private Task WithServiceAsync(Func<PersonService, Task> action, params object[] arguments) =>
        WithServiceAsync<PersonService>(action, arguments);

    private Task<TResult> WithServiceAsync<TResult>(Func<PersonService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<PersonService, TResult>(action, arguments);
}
