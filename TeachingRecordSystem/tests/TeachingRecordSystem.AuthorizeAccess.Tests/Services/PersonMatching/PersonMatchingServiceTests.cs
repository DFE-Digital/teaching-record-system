using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.AuthorizeAccess.Services.PersonMatching;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.Services.PersonMatching;

public class PersonMatchingServiceTests
{
    public PersonMatchingServiceTests(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        DbFixture = dbFixture;
        Clock = new();

        var dbContextFactory = dbFixture.GetDbContextFactory();

        var syncHelper = new TrsDataSyncHelper(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(syncHelper));
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    [Theory]
    [MemberData(nameof(MatchData))]
    public Task Match_ReturnsExpectedResult(
            NameArgumentOption nameOption,
            DateOfBirthArgumentOption dateOfBirthOption,
            NationalInsuranceNumberArgumentOption nationalInsuranceNumberOption,
            TrnArgumentOption trnOption,
            bool expectMatch) =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();

            var alias = nameOption == NameArgumentOption.MatchesAlias ? TestData.GenerateFirstName() : null;
            if (alias is not null)
            {
                dbContext.NameSynonyms.Add(new NameSynonyms()
                {
                    Name = firstName,
                    Synonyms = [alias],
                });
                await dbContext.SaveChangesAsync();
            }

            var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber().WithFirstName(firstName));

            string[][] names = nameOption switch
            {
                NameArgumentOption.NoFullName => [[person.FirstName]],
                NameArgumentOption.MatchesPersonName => [[person.FirstName, person.LastName]],
                NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName => [[person.FirstName, person.LastName], [TestData.GenerateChangedFirstName(person.FirstName), person.LastName]],
                NameArgumentOption.MatchesAlias => [[alias!, person.LastName]],
                NameArgumentOption.SpecifiedButDifferentFirstName => [[TestData.GenerateChangedFirstName(person.FirstName), person.LastName]],
                NameArgumentOption.SpecifiedButDifferentLastName => [[person.FirstName, TestData.GenerateChangedLastName(person.LastName)]],
                _ => [],
            };

            DateOnly[] datesOfBirth = dateOfBirthOption switch
            {
                DateOfBirthArgumentOption.MatchesPersonDateOfBirth => [person.DateOfBirth],
                DateOfBirthArgumentOption.MultipleSpecifiedAndOneMatchesPersonDateOfBirth => [person.DateOfBirth, TestData.GenerateChangedDateOfBirth(person.DateOfBirth)],
                DateOfBirthArgumentOption.SpecifiedButDifferent => [TestData.GenerateChangedDateOfBirth(person.DateOfBirth)],
                _ => []
            };

            var nationalInsuranceNumber = nationalInsuranceNumberOption switch
            {
                NationalInsuranceNumberArgumentOption.SpecifiedAndMatches => person.NationalInsuranceNumber!,
                NationalInsuranceNumberArgumentOption.SpecifiedButDifferent => TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!),
                _ => null
            };

            var trn = trnOption switch
            {
                TrnArgumentOption.SpecifiedAndMatches => person.Trn!,
                TrnArgumentOption.SpecifiedButDifferent => await TestData.GenerateTrn(),
                _ => null
            };

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.Match(names, datesOfBirth, nationalInsuranceNumber, trn);

            // Assert
            if (expectMatch)
            {
                Assert.NotNull(result);
                Assert.Equal(person.PersonId, result.Value!.PersonId);
                Assert.Equal(person.Trn, result.Value!.Trn);
            }
            else
            {
                Assert.Null(result);
            }
        });

    [Fact]
    public Task Match_WithMultipleMatchingResults_ReturnsNull() =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();

            var person1 = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth));
            var person2 = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth));

            string[][] names = [[firstName, lastName]];
            DateOnly[] datesOfBirth = [dateOfBirth];
            var nationalInsuranceNumber = person1.NationalInsuranceNumber!;
            var trn = person2.Trn!;

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.Match(names, datesOfBirth, nationalInsuranceNumber, trn);

            // Assert
            Assert.Null(result);
        });

    [Fact]
    public Task Match_WithMultipleMatchingNames_ReturnsResult() =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var firstName = Guid.NewGuid().ToString();  // Deliberately weird first name to avoid unique constraint violations in NameSynonyms below

            var alias = TestData.GenerateFirstName();
            dbContext.NameSynonyms.Add(new NameSynonyms()
            {
                Name = firstName,
                Synonyms = [alias],
            });
            await dbContext.SaveChangesAsync();

            var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber().WithFirstName(firstName));

            string[][] names = [[person.FirstName, person.LastName], [alias, person.LastName]];
            DateOnly[] datesOfBirth = [person.DateOfBirth];
            var nationalInsuranceNumber = person.NationalInsuranceNumber!;
            var trn = person.Trn!;

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.Match(names, datesOfBirth, nationalInsuranceNumber, trn);

            // Assert
            Assert.NotNull(result);
        });

    public static TheoryData<NameArgumentOption, DateOfBirthArgumentOption, NationalInsuranceNumberArgumentOption, TrnArgumentOption, bool> MatchData { get; } = new()
    {
        // *** Match cases ***

        // Single name, single DOB, NINO and TRN all match
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Single name, single DOB, NINO match but no TRN
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true
        },

        // Single name, single DOB, NINO match but TRN doesn't match
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true
        },

        // Single name, single DOB, TRN match but no NINO
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Single name, single DOB, TRN match but NINO doesn't match
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Single name with alias, single DOB, NINO and TRN all match
        {
            NameArgumentOption.MatchesAlias,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Single name with alias, single DOB, NINO match but no TRN
        {
            NameArgumentOption.MatchesAlias,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true
        },

        // Single name with alias, single DOB, NINO match but TRN doesn't match
        {
            NameArgumentOption.MatchesAlias,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true
        },

        // Single name with alias, single DOB, TRN match but no NINO
        {
            NameArgumentOption.MatchesAlias,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Single name with alias, single DOB, TRN match but NINO doesn't match
        {
            NameArgumentOption.MatchesAlias,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Multiple names with one match, single DOB, NINO and TRN all match
        {
            NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Multiple names with one match, single DOB, NINO match but no TRN
        {
            NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true
        },

        // Multiple names with one match, single DOB, NINO match but TRN doesn't match
        {
            NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true
        },

        // Multiple names with one match, single DOB, TRN match but no NINO
        {
            NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },

        // Multiple names with one match, single DOB, TRN match but NINO doesn't match
        {
            NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true
        },


        // *** No match cases ***

        // Missing names
        {
            NameArgumentOption.NotSpecifed,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ false
        },

        // Missing full name
        {
            NameArgumentOption.NoFullName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ false
        },

        // Missing dates of birth
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.NotSpecifed,
            NationalInsuranceNumberArgumentOption.SpecifiedAndMatches,
            TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ false
        },

        // Missing TRN and NINO
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false
        },

        // First name doesn't match
        {
            NameArgumentOption.SpecifiedButDifferentFirstName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false
        },

        // Last name doesn't match
        {
            NameArgumentOption.SpecifiedButDifferentLastName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false
        },

        // DOB doesn't match
        {
            NameArgumentOption.SpecifiedButDifferentLastName,
            DateOfBirthArgumentOption.SpecifiedButDifferent,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false
        },

        // Neither NINO nor TRN match
        {
            NameArgumentOption.SpecifiedButDifferentLastName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ false
        },

        // NINO doesn't match, TRN not specified
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false
        },

        // TRN doesn't match, NINO not specified
        {
            NameArgumentOption.MatchesPersonName,
            DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            NationalInsuranceNumberArgumentOption.NotSpecified,
            TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ false
        },
    };

    public enum NameArgumentOption
    {
        NotSpecifed,
        NoFullName,
        MatchesPersonName,
        MultipleSpecifiedAndOneMatchesPersonName,
        MatchesAlias,
        SpecifiedButDifferentFirstName,
        SpecifiedButDifferentLastName
    }

    public enum DateOfBirthArgumentOption
    {
        NotSpecifed,
        MatchesPersonDateOfBirth,
        MultipleSpecifiedAndOneMatchesPersonDateOfBirth,
        SpecifiedButDifferent
    }

    public enum NationalInsuranceNumberArgumentOption
    {
        NotSpecified,
        SpecifiedAndMatches,
        SpecifiedButDifferent
    }

    public enum TrnArgumentOption
    {
        NotSpecified,
        SpecifiedAndMatches,
        SpecifiedButDifferent
    }
}
