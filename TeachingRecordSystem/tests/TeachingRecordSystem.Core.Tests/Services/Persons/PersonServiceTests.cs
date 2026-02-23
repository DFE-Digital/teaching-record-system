using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Core.Tests.Services.Persons;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class PersonServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task GetPersonAsync_ActivePerson_ReturnsPerson()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var result = await WithServiceAsync(s => s.GetPersonAsync(person.PersonId));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(person.PersonId, result.PersonId);
    }

    [Fact]
    public async Task GetPersonAsync_DeactivatedPerson_ReturnsNull()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await WithServiceAsync(s => s.GetPersonAsync(person.PersonId));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPersonAsync_DeactivatedPerson_WhenIncludeDeactivatedPersonsIsTrue_ReturnsPerson()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await WithServiceAsync(s => s.GetPersonAsync(person.PersonId, includeDeactivatedPersons: true));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(person.PersonId, result.PersonId);
    }

    [Fact]
    public async Task CreatePersonAsync_CreatesPersonAndPublishesEvent()
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = EmailAddress.Parse((string?)"test@test.com");
        var nationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C");
        var gender = Gender.Female;

        var options = new CreatePersonOptions
        {
            SourceTrnRequest = null,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender,
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var person = await WithServiceAsync(s => s.CreatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.CreatedOn);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.UpdatedOn);
            Assert.Equal(firstName, createdPersonRecord.FirstName);
            Assert.Equal(middleName, createdPersonRecord.MiddleName);
            Assert.Equal(lastName, createdPersonRecord.LastName);
            Assert.Equal(dateOfBirth, createdPersonRecord.DateOfBirth);
            Assert.Equal(emailAddress?.ToString(), createdPersonRecord.EmailAddress);
            Assert.Equal(nationalInsuranceNumber?.ToString(), createdPersonRecord.NationalInsuranceNumber);
            Assert.Equal(gender, createdPersonRecord.Gender);
            Assert.Null(createdPersonRecord.SourceTrnRequestId);
            Assert.Null(createdPersonRecord.SourceApplicationUserId);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonCreatedEvent>(e);
            Assert.Equal(person.PersonId, @event.PersonId);
            Assert.Equal(firstName, @event.Details.FirstName);
            Assert.Equal(middleName, @event.Details.MiddleName);
            Assert.Equal(lastName, @event.Details.LastName);
            Assert.Equal(dateOfBirth, @event.Details.DateOfBirth);
            Assert.Equal(emailAddress?.ToString(), @event.Details.EmailAddress);
            Assert.Equal(nationalInsuranceNumber?.ToString(), @event.Details.NationalInsuranceNumber);
            Assert.Equal(gender, @event.Details.Gender);
            Assert.Null(@event.TrnRequestMetadata);
        });
    }

    [Fact]
    public async Task CreatePersonAsync_WithTrn_SetsTrnOnPerson()
    {
        // Arrange
        var trn = "1234567";
        var trnRequestMetadata = await CreateTrnRequestMetadataAsync();

        var options = new CreatePersonOptions
        {
            Trn = Option.Some(trn),
            SourceTrnRequest = (trnRequestMetadata.ApplicationUserId, trnRequestMetadata.RequestId),
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null,
        };
        var processContext = new ProcessContext(ProcessType.TeacherPensionsRecordImporting, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var person = await WithServiceAsync(s => s.CreatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(trn, createdPersonRecord.Trn);
            Assert.True(createdPersonRecord.CreatedByTps);
        });
    }

    [Fact]
    public async Task CreatePersonAsync_WithSourceTrnRequest_WhenSourceRequestDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var sourceTrnRequest = (SystemUser.SystemUserId, Guid.NewGuid().ToString());

        var options = new CreatePersonOptions
        {
            SourceTrnRequest = sourceTrnRequest,
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = DateOnly.Parse("1 Feb 1980"),
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null,
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.CreatePersonAsync(options, processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact]
    public async Task CreatePersonAsync_WithSourceTrnRequest_SetsSourceRequestPropertiesOnPersonAndPopulatesEventTrnRequestMetadata()
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = EmailAddress.Parse((string?)"test@test.com");
        var nationalInsuranceNumber = NationalInsuranceNumber.Parse("AB123456C");
        var gender = Gender.Female;

        var trnRequestMetadata = await CreateTrnRequestMetadataAsync(firstName, middleName, lastName, dateOfBirth, emailAddress?.ToString(), nationalInsuranceNumber?.ToString(), gender);

        var options = new CreatePersonOptions
        {
            SourceTrnRequest = (trnRequestMetadata.ApplicationUserId, trnRequestMetadata.RequestId),
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender,
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var person = await WithServiceAsync(s => s.CreatePersonAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(trnRequestMetadata.RequestId, createdPersonRecord.SourceTrnRequestId);
            Assert.Equal(trnRequestMetadata.ApplicationUserId, createdPersonRecord.SourceApplicationUserId);
        });

        Events.AssertEventsPublished(e =>
        {
            var @event = Assert.IsType<PersonCreatedEvent>(e);
            Assert.Equal(person.PersonId, @event.PersonId);
            Assert.NotNull(@event.TrnRequestMetadata);
            Assert.Equal(trnRequestMetadata.ApplicationUserId, @event.TrnRequestMetadata.ApplicationUserId);
            Assert.Equal(trnRequestMetadata.RequestId, @event.TrnRequestMetadata.RequestId);
        });
    }

    [Fact]
    public async Task UpdatePersonDetailsAsync_PersonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var options = new UpdatePersonDetailsOptions
        {
            PersonId = Guid.NewGuid(),
            CreatePreviousName = false,
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact]
    public async Task UpdatePersonDetailsAsync_PersonIsDeactivated_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToUpdate = await TestData.CreatePersonAsync(p => p.WithFirstName("Alfred"));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToUpdate.Person);
            personToUpdate.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var options = new UpdatePersonDetailsOptions
        {
            PersonId = personToUpdate.PersonId,
            CreatePreviousName = false,
            FirstName = Option.Some("Jim"),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
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

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Jim" : personToUpdate.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "A" : personToUpdate.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Person" : personToUpdate.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("3 July 1990") : personToUpdate.DateOfBirth;
        var emailAddress = EmailAddress.Parse(changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email.com" : personToUpdate.EmailAddress);
        var nationalInsuranceNumber = NationalInsuranceNumber.Parse(changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "JK987654D" : personToUpdate.NationalInsuranceNumber!);
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : personToUpdate.Gender;

        var options = new UpdatePersonDetailsOptions
        {
            PersonId = personToUpdate.PersonId,
            CreatePreviousName = false,
            FirstName = Option.Some(firstName),
            MiddleName = Option.Some(middleName),
            LastName = Option.Some(lastName),
            DateOfBirth = Option.Some<DateOnly?>(dateOfBirth),
            EmailAddress = Option.Some<EmailAddress?>(emailAddress),
            NationalInsuranceNumber = Option.Some<NationalInsuranceNumber?>(nationalInsuranceNumber),
            Gender = Option.Some<Gender?>(gender),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.UpdatePersonDetailsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == personToUpdate.PersonId);
            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal(firstName, updatedPersonRecord.FirstName);
            Assert.Equal(middleName, updatedPersonRecord.MiddleName);
            Assert.Equal(lastName, updatedPersonRecord.LastName);
            Assert.Equal(dateOfBirth, updatedPersonRecord.DateOfBirth);
            Assert.Equal(emailAddress?.ToString(), updatedPersonRecord.EmailAddress);
            Assert.Equal(nationalInsuranceNumber?.ToString(), updatedPersonRecord.NationalInsuranceNumber);
            Assert.Equal(gender, updatedPersonRecord.Gender);
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
            Assert.Equal(firstName, @event.PersonDetails.FirstName);
            Assert.Equal(middleName, @event.PersonDetails.MiddleName);
            Assert.Equal(lastName, @event.PersonDetails.LastName);
            Assert.Equal(dateOfBirth, @event.PersonDetails.DateOfBirth);
            Assert.Equal(emailAddress?.ToString(), @event.PersonDetails.EmailAddress);
            Assert.Equal(nationalInsuranceNumber?.ToString(), @event.PersonDetails.NationalInsuranceNumber);
            Assert.Equal(gender, @event.PersonDetails.Gender);
            Assert.Equal(changes, @event.Changes);
        });
    }

    [Fact]
    public async Task UpdatePersonDetailsAsync_WhenNameFieldChanged_AndCreatePreviousNameIsFalse_DoesNotCreatePreviousName()
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

        var options = new UpdatePersonDetailsOptions
        {
            PersonId = personToUpdate.PersonId,
            CreatePreviousName = false,
            FirstName = Option.Some("Alfrede"),
            MiddleName = Option.Some("Thee"),
            LastName = Option.Some("Greate"),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

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

    [Fact]
    public async Task UpdatePersonDetailsAsync_WhenNameFieldChanged_AndCreatePreviousNameIsTrue_CreatesPreviousName()
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

        var options = new UpdatePersonDetailsOptions
        {
            PersonId = personToUpdate.PersonId,
            CreatePreviousName = true,
            FirstName = Option.Some("Megan"),
            MiddleName = Option.Some("Thee"),
            LastName = Option.Some("Stallion"),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

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

        var options = new UpdatePersonDetailsOptions
        {
            PersonId = personToUpdate.PersonId,
            CreatePreviousName = false,
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

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
    public async Task DeactivatePersonAsync_PersonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonAsync(Guid.NewGuid(), processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
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

        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonAsync(personToDeactivate.PersonId, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task DeactivatePersonAsync_UpdatesPersonStatusAndPublishesEvent()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink"));

        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.DeactivatePersonAsync(personToDeactivate.PersonId, processContext));

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
        });
    }

    [Fact]
    public async Task ReactivatePersonAsync_PersonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.ReactivatePersonAsync(Guid.NewGuid(), processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact]
    public async Task ReactivatePersonAsync_PersonIsAlreadyActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToReactivate = await TestData.CreatePersonAsync();
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.ReactivatePersonAsync(personToReactivate.PersonId, processContext)));

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

        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ReactivatePersonAsync(personToReactivate.PersonId, processContext));

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
        });
    }

    [Fact]
    public async Task DeactivatePersonViaMergeAsync_PersonToDeactivateDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var personToRetain = await TestData.CreatePersonAsync();

        var options = new DeactivatePersonViaMergeOptions(Guid.NewGuid(), personToRetain.PersonId);
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
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

        var options = new DeactivatePersonViaMergeOptions(personToDeactivate.PersonId, personToRetain.PersonId);
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task DeactivatePersonViaMergeAsync_PersonToRetainDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();

        var options = new DeactivatePersonViaMergeOptions(personToDeactivate.PersonId, Guid.NewGuid());
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact]
    public async Task DeactivatePersonViaMergeAsync_PersonToRetainIsDeactivated_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToRetain.Person);
            personToRetain.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var options = new DeactivatePersonViaMergeOptions(personToDeactivate.PersonId, personToRetain.PersonId);
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task DeactivatePersonViaMergeAsync_ValidRequest_DeactivatesPersonAndPublishesEvent()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        var options = new DeactivatePersonViaMergeOptions(personToDeactivate.PersonId, personToRetain.PersonId);
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.DeactivatePersonViaMergeAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var deactivatedPerson = await dbContext.Persons
                .IgnoreQueryFilters()
                .SingleAsync(p => p.PersonId == personToDeactivate.PersonId);
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

    [Fact]
    public async Task MergePersonsAsync_PersonToDeactivateDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var personToRetain = await TestData.CreatePersonAsync();

        var options = new MergePersonsOptions
        {
            DeactivatingPersonId = Guid.NewGuid(),
            RetainedPersonId = personToRetain.PersonId,
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.MergePersonsAsync(options, processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact]
    public async Task MergePersonsAsync_PersonToDeactivateIsAlreadyDeactivated_ThrowsInvalidOperationException()
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

        var options = new MergePersonsOptions
        {
            DeactivatingPersonId = personToDeactivate.PersonId,
            RetainedPersonId = personToRetain.PersonId,
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.MergePersonsAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task MergePersonsAsync_PersonToRetainDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();

        var options = new MergePersonsOptions
        {
            DeactivatingPersonId = personToDeactivate.PersonId,
            RetainedPersonId = Guid.NewGuid(),
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.MergePersonsAsync(options, processContext)));

        // Assert
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact]
    public async Task MergePersonsAsync_PersonToRetainIsDeactivated_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToRetain.Person);
            personToRetain.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var options = new MergePersonsOptions
        {
            DeactivatingPersonId = personToDeactivate.PersonId,
            RetainedPersonId = personToRetain.PersonId,
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.MergePersonsAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Theory]
    [MemberData(nameof(GetPersonAttributeInfoData))]
    public async Task MergePersonsAsync_ValidRequest_UpdatesRetainedPersonAndDeactivatesDeactivatingPersonAndPublishesEvents(
        PersonAttributeInfo sourcedFromDeactivatingPersonAttribute,
        bool useNullValues)
    {
        // Arrange
        var (personToRetain, personToDeactivate) = await CreatePersonsWithSingleDifferenceToMatch(
            sourcedFromDeactivatingPersonAttribute.Attribute,
            useNullValues: useNullValues);

        Clock.Advance();

        var options = new MergePersonsOptions
        {
            DeactivatingPersonId = personToDeactivate.PersonId,
            RetainedPersonId = personToRetain.PersonId,
            FirstName = Option.Some(sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.FirstName ? personToDeactivate.FirstName : personToRetain.FirstName),
            MiddleName = Option.Some(sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.MiddleName ? personToDeactivate.MiddleName : personToRetain.MiddleName),
            LastName = Option.Some(sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.LastName ? personToDeactivate.LastName : personToRetain.LastName),
            DateOfBirth = Option.Some<DateOnly?>(sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.DateOfBirth ? personToDeactivate.DateOfBirth : personToRetain.DateOfBirth),
            EmailAddress = Option.Some<EmailAddress?>((sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.EmailAddress ? personToDeactivate.EmailAddress : personToRetain.EmailAddress)
                is string email ? EmailAddress.Parse(email) : null),
            NationalInsuranceNumber = Option.Some<NationalInsuranceNumber?>((sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.NationalInsuranceNumber ? personToDeactivate.NationalInsuranceNumber : personToRetain.NationalInsuranceNumber)
                is string nino ? NationalInsuranceNumber.Parse(nino) : null),
            Gender = Option.Some<Gender?>(sourcedFromDeactivatingPersonAttribute.Attribute is PersonMatchedAttribute.Gender ? personToDeactivate.Gender : personToRetain.Gender),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.MergePersonsAsync(options, processContext));

        // Assert
        var updatedPersonToRetain = await WithDbContextAsync(dbContext => dbContext.Persons
            .IgnoreQueryFilters()
            .Include(p => p.MergedWithPerson)
            .SingleAsync(p => p.PersonId == personToRetain.PersonId));
        Assert.Equal(PersonStatus.Active, updatedPersonToRetain.Status);
        Assert.Null(updatedPersonToRetain.MergedWithPersonId);
        Assert.Equal(Clock.UtcNow, updatedPersonToRetain.UpdatedOn);

        var updatedPersonToDeactivate = await WithDbContextAsync(dbContext => dbContext.Persons
            .IgnoreQueryFilters()
            .Include(p => p.MergedWithPerson)
            .SingleAsync(p => p.PersonId == personToDeactivate.PersonId));
        Assert.Equal(PersonStatus.Deactivated, updatedPersonToDeactivate.Status);
        Assert.Equal(updatedPersonToRetain.PersonId, updatedPersonToDeactivate.MergedWithPersonId);
        Assert.Equal(Clock.UtcNow, updatedPersonToDeactivate.UpdatedOn);

        foreach (var attr in PersonAttributeInfos)
        {
            if (attr.Attribute == sourcedFromDeactivatingPersonAttribute.Attribute)
            {
                Assert.Equal(attr.GetValueFromPersonResult(personToDeactivate), attr.GetValueFromPerson(updatedPersonToRetain));
            }
            else
            {
                Assert.Equal(attr.GetValueFromPersonResult(personToRetain), attr.GetValueFromPerson(updatedPersonToRetain));
            }
        }

        var expectedChange = sourcedFromDeactivatingPersonAttribute.Attribute switch
        {
            PersonMatchedAttribute.FirstName => PersonDetailsUpdatedEventChanges.FirstName,
            PersonMatchedAttribute.MiddleName => PersonDetailsUpdatedEventChanges.MiddleName,
            PersonMatchedAttribute.LastName => PersonDetailsUpdatedEventChanges.LastName,
            PersonMatchedAttribute.DateOfBirth => PersonDetailsUpdatedEventChanges.DateOfBirth,
            PersonMatchedAttribute.EmailAddress => PersonDetailsUpdatedEventChanges.EmailAddress,
            PersonMatchedAttribute.NationalInsuranceNumber => PersonDetailsUpdatedEventChanges.NationalInsuranceNumber,
            PersonMatchedAttribute.Gender => PersonDetailsUpdatedEventChanges.Gender,
            PersonMatchedAttribute.FullName => throw new NotImplementedException(),
            PersonMatchedAttribute.Trn => throw new NotImplementedException(),
            _ => PersonDetailsUpdatedEventChanges.None
        };

        Events.AssertEventsPublished(
            e =>
            {
                var @event = Assert.IsType<PersonDeactivatedEvent>(e);
                Assert.Equal(personToDeactivate.PersonId, @event.PersonId);
                Assert.Equal(personToRetain.PersonId, @event.MergedWithPersonId);
            },
            e =>
            {
                var @event = Assert.IsType<PersonDetailsUpdatedEvent>(e);
                Assert.Equal(personToRetain.PersonId, @event.PersonId);

                foreach (var attr in PersonAttributeInfos)
                {
                    Assert.Equal(attr.GetValueFromPersonResult(personToRetain), attr.GetValueFromEventPersonDetails(@event.OldPersonDetails));
                }

                foreach (var attr in PersonAttributeInfos)
                {
                    if (attr.Attribute == sourcedFromDeactivatingPersonAttribute.Attribute)
                    {
                        Assert.Equal(attr.GetValueFromPersonResult(personToDeactivate), attr.GetValueFromEventPersonDetails(@event.PersonDetails));
                    }
                    else
                    {
                        Assert.Equal(attr.GetValueFromPersonResult(personToRetain), attr.GetValueFromEventPersonDetails(@event.PersonDetails));
                    }
                }

                Assert.Equal(expectedChange, @event.Changes);
            });
    }

    [Fact]
    public async Task MergePersonsAsync_PersonToDeactivateIsAssociatedWithOneLoginUser_AssociatesThatOneLoginUserWithRetainedPerson()
    {
        // Arrange
        var personToRetain = await TestData.CreatePersonAsync();
        var personToDeactivate = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: personToDeactivate.PersonId,
            verifiedInfo: ([personToDeactivate.FirstName, personToDeactivate.LastName], personToDeactivate.DateOfBirth));

        var options = new MergePersonsOptions
        {
            DeactivatingPersonId = personToDeactivate.PersonId,
            RetainedPersonId = personToRetain.PersonId,
            FirstName = Option.None<string>(),
            MiddleName = Option.None<string>(),
            LastName = Option.None<string>(),
            DateOfBirth = Option.None<DateOnly?>(),
            EmailAddress = Option.None<EmailAddress?>(),
            NationalInsuranceNumber = Option.None<NationalInsuranceNumber?>(),
            Gender = Option.None<Gender?>(),
        };
        var processContext = new ProcessContext((ProcessType)0, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.MergePersonsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedOneLoginUser = await dbContext.OneLoginUsers
                .SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(personToRetain.PersonId, updatedOneLoginUser.PersonId);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.MatchedOn);
            Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
        });
    }

    private Task WithServiceAsync(Func<PersonService, Task> action, params object[] arguments) =>
        WithServiceAsync<PersonService>(action, arguments);

    private Task<TResult> WithServiceAsync<TResult>(Func<PersonService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<PersonService, TResult>(action, arguments);

    private async Task<TrnRequestMetadata> CreateTrnRequestMetadataAsync(
        string? firstName = null,
        string? middleName = null,
        string? lastName = null,
        DateOnly? dateOfBirth = null,
        string? emailAddress = null,
        string? nationalInsuranceNumber = null,
        Gender? gender = null)
    {
        var trnRequestMetadata = new TrnRequestMetadata
        {
            ApplicationUserId = SystemUser.SystemUserId,
            RequestId = Guid.NewGuid().ToString(),
            CreatedOn = Clock.UtcNow,
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Name = new[] { firstName ?? "Alfred", middleName ?? "The", lastName ?? "Great" }.GetNonEmptyValues(),
            FirstName = firstName ?? "Alfred",
            MiddleName = middleName ?? "The",
            LastName = lastName ?? "Great",
            PreviousFirstName = null,
            PreviousLastName = null,
            DateOfBirth = dateOfBirth ?? DateOnly.Parse("1 Feb 1980"),
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender,
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
            await dbContext.SaveChangesAsync();
        });

        return trnRequestMetadata;
    }

    public static PersonAttributeInfo[] PersonAttributeInfos { get; } =
    [
        new(
            PersonMatchedAttribute.FirstName,
            p => p.FirstName,
            p => p.FirstName,
            p => p.FirstName
        ),
        new(
            PersonMatchedAttribute.MiddleName,
            p => p.MiddleName,
            p => p.MiddleName,
            p => p.MiddleName
        ),
        new(
            PersonMatchedAttribute.LastName,
            p => p.LastName,
            p => p.LastName,
            p => p.LastName
        ),
        new(
            PersonMatchedAttribute.DateOfBirth,
            p => p.DateOfBirth,
            p => p.DateOfBirth,
            p => p.DateOfBirth
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            p => p.EmailAddress,
            p => p.EmailAddress,
            p => p.EmailAddress
        ),
        new(
            PersonMatchedAttribute.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber
        ),
        new(
            PersonMatchedAttribute.Gender,
            p => p.Gender,
            p => p.Gender,
            p => p.Gender
        )
    ];

    public static (PersonAttributeInfo Attribute, bool UseNullValues)[] GetPersonAttributeInfoData() =>
        PersonAttributeInfos.SelectMany(i => new[] { (i, false), (i, true) }).ToArray();

    public record PersonAttributeInfo(
        PersonMatchedAttribute Attribute,
        Func<TestData.CreatePersonResult, object?> GetValueFromPersonResult,
        Func<Person, object?> GetValueFromPerson,
        Func<EventModels.PersonDetails, object?> GetValueFromEventPersonDetails);

    protected async Task<(TestData.CreatePersonResult PersonToRetain, TestData.CreatePersonResult PersonToDeactivate)> CreatePersonsWithSingleDifferenceToMatch(
        PersonMatchedAttribute differentAttribute,
        bool useNullValues = false)
    {
        var personToRetain = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personToDeactivate = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithFirstName(
                    differentAttribute != PersonMatchedAttribute.FirstName
                        ? personToRetain.FirstName
                        : TestData.GenerateChangedFirstName([personToRetain.FirstName, personToRetain.MiddleName, personToRetain.LastName]))
                .WithMiddleName(
                    differentAttribute != PersonMatchedAttribute.MiddleName
                        ? personToRetain.MiddleName
                        : TestData.GenerateChangedMiddleName([personToRetain.FirstName, personToRetain.MiddleName, personToRetain.LastName]))
                .WithLastName(
                    differentAttribute != PersonMatchedAttribute.LastName
                        ? personToRetain.LastName
                        : TestData.GenerateChangedLastName([personToRetain.FirstName, personToRetain.MiddleName, personToRetain.LastName]))
                .WithDateOfBirth(
                    differentAttribute != PersonMatchedAttribute.DateOfBirth
                        ? personToRetain.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(personToRetain.DateOfBirth));

            if (useNullValues)
            {
                p
                    .WithEmailAddress(differentAttribute != PersonMatchedAttribute.EmailAddress
                        ? false
                        : true)
                    .WithNationalInsuranceNumber(differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                       ? false
                       : true)
                    .WithGender(differentAttribute != PersonMatchedAttribute.Gender
                        ? false
                        : true);
            }
            else
            {
                p
                    .WithEmailAddress(differentAttribute != PersonMatchedAttribute.EmailAddress
                        ? personToRetain.EmailAddress
                        : TestData.GenerateUniqueEmail())
                    .WithNationalInsuranceNumber(differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                        ? personToRetain.NationalInsuranceNumber!
                        : TestData.GenerateChangedNationalInsuranceNumber(personToRetain.NationalInsuranceNumber!))
                    .WithGender(differentAttribute != PersonMatchedAttribute.Gender
                        ? personToRetain.Gender!.Value
                        : TestData.GenerateChangedGender(personToRetain.Gender!.Value));
            }
        });

        return (personToRetain, personToDeactivate);
    }
}
