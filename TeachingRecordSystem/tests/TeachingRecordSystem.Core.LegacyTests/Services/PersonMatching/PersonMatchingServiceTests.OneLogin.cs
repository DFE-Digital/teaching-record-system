using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.Core.Tests.Services.PersonMatching;

public partial class PersonMatchingServiceTests
{
    [Theory]
    [MemberData(nameof(MatchOneLoginUserData))]
    public Task MatchOneLoginUserAsync_ReturnsExpectedResult(
            OneLogin.NameArgumentOption nameOption,
            OneLogin.DateOfBirthArgumentOption dateOfBirthOption,
            OneLogin.NationalInsuranceNumberArgumentOption nationalInsuranceNumberOption,
            OneLogin.TrnArgumentOption trnOption,
            bool expectMatch,
            IEnumerable<PersonMatchedAttribute>? expectedMatchedAttributes) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();

            var alias = nameOption == OneLogin.NameArgumentOption.MatchesAlias ? TestData.GenerateChangedFirstName(firstName) : null;
            if (alias is not null)
            {
                dbContext.NameSynonyms.Add(new NameSynonyms()
                {
                    Name = firstName,
                    Synonyms = [alias]
                });
                dbContext.NameSynonyms.Add(new NameSynonyms()
                {
                    Name = alias,
                    Synonyms = [firstName]
                });
                await dbContext.SaveChangesAsync();
            }

            var middleName = TestData.GenerateChangedMiddleName([firstName, alias]);

            var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithFirstName(firstName).WithMiddleName(middleName));
            var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321", establishmentNumber: "4321", establishmentStatusCode: 1);
            var employmentNino = TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
            var personEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment, new DateOnly(2023, 08, 03), new DateOnly(2024, 05, 25), EmploymentType.FullTime, new DateOnly(2024, 05, 25), employmentNino);

            string[][] names = nameOption switch
            {
                OneLogin.NameArgumentOption.NoFullName => [[person.FirstName]],
                OneLogin.NameArgumentOption.MatchesPersonName => [[person.FirstName, person.LastName]],
                OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName => [[person.FirstName, person.LastName], [TestData.GenerateChangedFirstName([person.FirstName, alias, person.MiddleName]), person.LastName]],
                OneLogin.NameArgumentOption.MatchesAlias => [[alias!, person.LastName]],
                OneLogin.NameArgumentOption.SpecifiedButDifferentFirstName => [[TestData.GenerateChangedFirstName([person.FirstName, alias, person.MiddleName]), person.LastName]],
                OneLogin.NameArgumentOption.SpecifiedButDifferentLastName => [[person.FirstName, TestData.GenerateChangedLastName(person.LastName)]],
                _ => []
            };

            DateOnly[] datesOfBirth = dateOfBirthOption switch
            {
                OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth => [person.DateOfBirth],
                OneLogin.DateOfBirthArgumentOption.MultipleSpecifiedAndOneMatchesPersonDateOfBirth => [person.DateOfBirth, TestData.GenerateChangedDateOfBirth(person.DateOfBirth)],
                OneLogin.DateOfBirthArgumentOption.SpecifiedButDifferent => [TestData.GenerateChangedDateOfBirth(person.DateOfBirth)],
                _ => []
            };

            var nationalInsuranceNumber = nationalInsuranceNumberOption switch
            {
                OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino => person.NationalInsuranceNumber!,
                OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino => personEmployment.NationalInsuranceNumber!,
                OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedButDifferent => TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!),
                _ => null
            };

            var trn = trnOption switch
            {
                OneLogin.TrnArgumentOption.SpecifiedAndMatches => person.Trn!,
                OneLogin.TrnArgumentOption.SpecifiedButDifferent => await TestData.GenerateTrnAsync(),
                _ => null
            };

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.MatchOneLoginUserAsync(new(names, datesOfBirth, nationalInsuranceNumber, trn));

            // Assert
            if (expectMatch)
            {
                Assert.NotNull(result);
                Assert.Equal(person.PersonId, result.PersonId);
                Assert.Equal(person.Trn, result.Trn);
                Assert.Equal(expectedMatchedAttributes?.Order(), result.MatchedAttributes.Select(kvp => kvp.Key).Distinct().Order());
            }
            else
            {
                Assert.Null(result);
            }
        });

    [Fact]
    public Task MatchOneLoginUserAsync_WithMultipleMatchingResults_ReturnsNull() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();

            var person1 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth));
            var person2 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth));

            string[][] names = [[firstName, lastName]];
            DateOnly[] datesOfBirth = [dateOfBirth];
            var nationalInsuranceNumber = person1.NationalInsuranceNumber!;
            var trn = person2.Trn!;

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.MatchOneLoginUserAsync(new(names, datesOfBirth, nationalInsuranceNumber, trn));

            // Assert
            Assert.Null(result);
        });

    [Fact]
    public Task MatchOneLoginUserAsync_WithMultipleMatchingNames_ReturnsResult() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var firstName = Guid.NewGuid().ToString();  // Deliberately weird first name to avoid unique constraint violations in NameSynonyms below

            var alias = TestData.GenerateFirstName();
            dbContext.NameSynonyms.Add(new NameSynonyms()
            {
                Name = firstName,
                Synonyms = [alias]
            });
            await dbContext.SaveChangesAsync();

            var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithFirstName(firstName));

            string[][] names = [[person.FirstName, person.LastName], [alias, person.LastName]];
            DateOnly[] datesOfBirth = [person.DateOfBirth];
            var nationalInsuranceNumber = person.NationalInsuranceNumber!;
            var trn = person.Trn!;

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.MatchOneLoginUserAsync(new(names, datesOfBirth, nationalInsuranceNumber, trn));

            // Assert
            Assert.NotNull(result);
        });

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task GetSuggestedOneLoginUserMatchesAsync_ReturnsExpectedResults(bool usePersonNino) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();
            var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
            var alternativeNationalInsuranceNumber = TestData.GenerateChangedNationalInsuranceNumber(nationalInsuranceNumber);

            // Person who matches on last name & DOB
            var person1 = await TestData.CreatePersonAsync(p => p.WithLastName(lastName).WithDateOfBirth(dateOfBirth));

            // Person who matches on NINO
            var person2 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(usePersonNino ? nationalInsuranceNumber : alternativeNationalInsuranceNumber));
            var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321", establishmentNumber: "4321", establishmentStatusCode: 1);
            var personEmployment = await TestData.CreateTpsEmploymentAsync(person2, establishment, new DateOnly(2023, 08, 03), new DateOnly(2024, 05, 25), EmploymentType.FullTime, new DateOnly(2024, 05, 25), usePersonNino ? alternativeNationalInsuranceNumber : nationalInsuranceNumber);

            // Person who matches on TRN
            var person3 = await TestData.CreatePersonAsync();
            var trn = person3.Trn!;

            // Person who matches on last name, DOB & TRN
            var person4 = await TestData.CreatePersonAsync(p => p.WithLastName(lastName).WithDateOfBirth(dateOfBirth));
            var trnTokenHintTrn = person4.Trn!;

            string[][] names = [[firstName, lastName]];
            DateOnly[] datesOfBirth = [dateOfBirth];

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.GetSuggestedOneLoginUserMatchesAsync(new(names, datesOfBirth, nationalInsuranceNumber, trn, trnTokenHintTrn));

            // Assert
            Assert.Collection(
                result,
                r => Assert.Equal(person4.PersonId, r.PersonId),
                r => Assert.Equal(person3.PersonId, r.PersonId),
                r => Assert.Equal(person2.PersonId, r.PersonId),
                r => Assert.Equal(person1.PersonId, r.PersonId));
        });

    private static readonly PersonMatchedAttribute[] _matchNameDobNinoAndTrnAttributes =
    [
        PersonMatchedAttribute.FirstName,
        PersonMatchedAttribute.LastName,
        PersonMatchedAttribute.DateOfBirth,
        PersonMatchedAttribute.NationalInsuranceNumber,
        PersonMatchedAttribute.Trn
    ];

    private static readonly PersonMatchedAttribute[] _matchNameDobAndNinoAttributes =
    [
        PersonMatchedAttribute.FirstName,
        PersonMatchedAttribute.LastName,
        PersonMatchedAttribute.DateOfBirth,
        PersonMatchedAttribute.NationalInsuranceNumber
    ];

    private static readonly PersonMatchedAttribute[] _matchNameDobAndTrnAttributes =
    [
        PersonMatchedAttribute.FirstName,
        PersonMatchedAttribute.LastName,
        PersonMatchedAttribute.DateOfBirth,
        PersonMatchedAttribute.Trn
    ];

    public static TheoryData<OneLogin.NameArgumentOption, OneLogin.DateOfBirthArgumentOption, OneLogin.NationalInsuranceNumberArgumentOption, OneLogin.TrnArgumentOption, bool, IEnumerable<PersonMatchedAttribute>?> MatchOneLoginUserData { get; } = new()
    {
        // *** Match cases ***

        // Single name, single DOB, person NINO and TRN all match
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobNinoAndTrnAttributes
        },

        // Single name, single DOB, person NINO match but no TRN
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name, single DOB, person NINO match but TRN doesn't match
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name, single DOB, TRN match but no NINO
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobAndTrnAttributes
        },

        // Single name, single DOB, TRN match but person NINO doesn't match
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobAndTrnAttributes
        },

        // Single name, single DOB, employment NINO and TRN all match
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobNinoAndTrnAttributes
        },

        // Single name, single DOB, employment NINO match but no TRN
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name, single DOB, employment NINO match but TRN doesn't match
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name with alias, single DOB, person NINO and TRN all match
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobNinoAndTrnAttributes
        },

        // Single name with alias, single DOB, person NINO match but no TRN
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name with alias, single DOB, person NINO match but TRN doesn't match
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name with alias, single DOB, TRN match but no NINO
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobAndTrnAttributes
        },

        // Single name with alias, single DOB, TRN match but person NINO doesn't match
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobAndTrnAttributes
        },

        // Single name with alias, single DOB, employment NINO and TRN all match
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobNinoAndTrnAttributes
        },

        // Single name with alias, single DOB, employment NINO match but no TRN
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Single name with alias, single DOB, employment NINO match but TRN doesn't match
        {
            OneLogin.NameArgumentOption.MatchesAlias,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Multiple names with one match, single DOB, person NINO and TRN all match
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobNinoAndTrnAttributes
        },

        // Multiple names with one match, single DOB, person NINO match but no TRN
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Multiple names with one match, single DOB, person NINO match but TRN doesn't match
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Multiple names with one match, single DOB, TRN match but no person NINO
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobAndTrnAttributes
        },

        // Multiple names with one match, single DOB, TRN match but person NINO doesn't match
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobAndTrnAttributes
        },

        // Multiple names with one match, single DOB, employment NINO and TRN all match
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ true,
            _matchNameDobNinoAndTrnAttributes
        },

        // Multiple names with one match, single DOB, employment NINO match but no TRN
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },

        // Multiple names with one match, single DOB, employment NINO match but TRN doesn't match
        {
            OneLogin.NameArgumentOption.MultipleSpecifiedAndOneMatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesEmploymentNino,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ true,
            _matchNameDobAndNinoAttributes
        },


        // *** No match cases ***

        // Missing names
        {
            OneLogin.NameArgumentOption.NotSpecified,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ false,
            null
        },

        // Missing full name
        {
            OneLogin.NameArgumentOption.NoFullName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ false,
            null
        },

        // Missing dates of birth
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.NotSpecified,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedAndMatchesPersonNino,
            OneLogin.TrnArgumentOption.SpecifiedAndMatches,
            /*expectMatch: */ false,
            null
        },

        // Missing TRN and NINO
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false,
            null
        },

        // First name doesn't match
        {
            OneLogin.NameArgumentOption.SpecifiedButDifferentFirstName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false,
            null
        },

        // Last name doesn't match
        {
            OneLogin.NameArgumentOption.SpecifiedButDifferentLastName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false,
            null
        },

        // DOB doesn't match
        {
            OneLogin.NameArgumentOption.SpecifiedButDifferentLastName,
            OneLogin.DateOfBirthArgumentOption.SpecifiedButDifferent,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false,
            null
        },

        // Neither NINO nor TRN match
        {
            OneLogin.NameArgumentOption.SpecifiedButDifferentLastName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ false,
            null
        },

        // NINO doesn't match, TRN not specified
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.SpecifiedButDifferent,
            OneLogin.TrnArgumentOption.NotSpecified,
            /*expectMatch: */ false,
            null
        },

        // TRN doesn't match, NINO not specified
        {
            OneLogin.NameArgumentOption.MatchesPersonName,
            OneLogin.DateOfBirthArgumentOption.MatchesPersonDateOfBirth,
            OneLogin.NationalInsuranceNumberArgumentOption.NotSpecified,
            OneLogin.TrnArgumentOption.SpecifiedButDifferent,
            /*expectMatch: */ false,
            null
        }
    };

    public static class OneLogin
    {
        public enum NameArgumentOption
        {
            NotSpecified,
            NoFullName,
            MatchesPersonName,
            MultipleSpecifiedAndOneMatchesPersonName,
            MatchesAlias,
            SpecifiedButDifferentFirstName,
            SpecifiedButDifferentLastName
        }

        public enum DateOfBirthArgumentOption
        {
            NotSpecified,
            MatchesPersonDateOfBirth,
            MultipleSpecifiedAndOneMatchesPersonDateOfBirth,
            SpecifiedButDifferent
        }

        public enum NationalInsuranceNumberArgumentOption
        {
            NotSpecified,
            SpecifiedAndMatchesPersonNino,
            SpecifiedAndMatchesEmploymentNino,
            SpecifiedButDifferent
        }

        public enum TrnArgumentOption
        {
            NotSpecified,
            SpecifiedAndMatches,
            SpecifiedButDifferent
        }
    }
}
