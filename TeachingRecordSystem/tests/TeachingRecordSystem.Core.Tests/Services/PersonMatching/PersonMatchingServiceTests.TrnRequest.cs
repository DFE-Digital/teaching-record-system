using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.PersonMatching;
using static TeachingRecordSystem.Core.Tests.Services.PersonMatching.PersonMatchingServiceTests.TrnRequest;

namespace TeachingRecordSystem.Core.Tests.Services.PersonMatching;

public partial class PersonMatchingServiceTests
{
    [Fact]
    public Task MatchFromTrnRequestAsync_WithOutOfOrderNames_ReturnsMatch() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var applicationUser = await TestData.CreateApplicationUserAsync();
            var firstName = TestData.GenerateFirstName();
            var middleName = TestData.GenerateMiddleName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();
            var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
            var emailAddress = TestData.GenerateUniqueEmail();

            var person = await TestData.CreatePersonAsync(p => p
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmailAddress(emailAddress));

            var requestData = new TrnRequestMetadata()
            {
                ApplicationUserId = applicationUser.UserId,
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = Clock.UtcNow,
                IdentityVerified = null,
                EmailAddress = null,
                OneLoginUserSubject = null,
                FirstName = lastName,
                MiddleName = middleName,
                LastName = firstName,
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = [lastName, firstName, middleName],
                DateOfBirth = TestData.GenerateChangedDateOfBirth(dateOfBirth),
                NationalInsuranceNumber = TestData.GenerateChangedNationalInsuranceNumber(nationalInsuranceNumber)
            };

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.MatchFromTrnRequestAsync(requestData);

            // Assert
            Assert.Contains(person.PersonId, result.PotentialMatchesPersonIds);
        });

    [Theory]
    [MemberData(nameof(GetMatchFromTrnRequestData))]
    [MemberData(nameof(GetMatchFromTrnRequestDataWithMissingNino))]
    public Task MatchFromTrnRequestAsync_ReturnsExpectedResult(
            EmailAddressArgumentOption emailAddressOption,
            FirstNameArgumentOption firstNameOption,
            MiddleNameArgumentOption middleNameOption,
            LastNameArgumentOption lastNameOption,
            DateOfBirthArgumentOption dateOfBirthOption,
            NationalInsuranceNumberArgumentOption nationalInsuranceNumberOption,
            GenderArgumentOption genderOption,
            TrnRequestMatchResultOutcome expectedOutcome) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var applicationUser = await TestData.CreateApplicationUserAsync();

            var personFirstName = TestData.GenerateFirstName();

            var alias = firstNameOption == FirstNameArgumentOption.MatchesAlias ? TestData.GenerateChangedFirstName(personFirstName) : null;
            if (alias is not null)
            {
                dbContext.NameSynonyms.Add(new NameSynonyms()
                {
                    Name = personFirstName,
                    Synonyms = [alias]
                });
                dbContext.NameSynonyms.Add(new NameSynonyms()
                {
                    Name = alias,
                    Synonyms = [personFirstName]
                });
                await dbContext.SaveChangesAsync();
            }

            var person = await TestData.CreatePersonAsync(p => p
                .WithNationalInsuranceNumber()
                .WithFirstName(personFirstName)
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithGender());
            var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321", establishmentNumber: "4321", establishmentStatusCode: 1);
            var employmentNino = TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
            var personEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment, new DateOnly(2023, 08, 03), new DateOnly(2024, 05, 25), EmploymentType.FullTime, new DateOnly(2024, 05, 25), employmentNino);

            var emailAddress = emailAddressOption switch
            {
                EmailAddressArgumentOption.Matches => person.EmailAddress!,
                _ => TestData.GenerateUniqueEmail()
            };

            var firstName = firstNameOption switch
            {
                FirstNameArgumentOption.Matches => person.FirstName,
                FirstNameArgumentOption.MatchesAlias => alias!,
                _ => TestData.GenerateChangedFirstName(person.FirstName)
            };

            var middleName = middleNameOption switch
            {
                MiddleNameArgumentOption.Matches => person.MiddleName,
                _ => TestData.GenerateChangedMiddleName(person.MiddleName)
            };

            var lastName = lastNameOption switch
            {
                LastNameArgumentOption.Matches => person.LastName,
                _ => TestData.GenerateChangedLastName(person.LastName)
            };

            var dateOfBirth = dateOfBirthOption switch
            {
                DateOfBirthArgumentOption.Matches => person.DateOfBirth,
                _ => TestData.GenerateChangedDateOfBirth(person.DateOfBirth)
            };

            var nationalInsuranceNumber = nationalInsuranceNumberOption switch
            {
                NationalInsuranceNumberArgumentOption.Null => null,
                NationalInsuranceNumberArgumentOption.Empty => string.Empty,
                NationalInsuranceNumberArgumentOption.MatchesPersonNino => person.NationalInsuranceNumber!,
                NationalInsuranceNumberArgumentOption.MatchesEmploymentNino => personEmployment.NationalInsuranceNumber!,
                _ => TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!)
            };

            var gender = genderOption switch
            {
                GenderArgumentOption.Matches => person.Gender,
                _ => TestData.GenerateChangedGender(person.Gender)
            };

            var requestData = new TrnRequestMetadata()
            {
                ApplicationUserId = applicationUser.UserId,
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = Clock.UtcNow,
                IdentityVerified = null,
                EmailAddress = emailAddress,
                OneLoginUserSubject = null,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = [firstName, middleName, lastName],
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender
            };

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.MatchFromTrnRequestAsync(requestData);

            // Assert
            Assert.Equal(expectedOutcome, result.Outcome);

            if (expectedOutcome == TrnRequestMatchResultOutcome.DefiniteMatch)
            {
                Assert.Equal(person.PersonId, result.PersonId);
            }
        });

    [Fact]
    public Task GetSuggestedMatchesFromTrnRequestAsync() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var applicationUser = await TestData.CreateApplicationUserAsync();
            var firstName = TestData.GenerateFirstName();
            var middleName = TestData.GenerateMiddleName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();
            var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
            var emailAddress = TestData.GenerateUniqueEmail();

            // Person matching on NINO, first name and last name
            var person1 = await TestData.CreatePersonAsync(p => p
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithFirstName(firstName)
                .WithLastName(lastName));

            // Person matching on first name, last name, DOB and email
            var person2 = await TestData.CreatePersonAsync(p => p
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmailAddress(emailAddress));

            // Person matching on first name, middle name and last name
            var person3 = await TestData.CreatePersonAsync(p => p
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName));

            var requestData = new TrnRequestMetadata()
            {
                ApplicationUserId = applicationUser.UserId,
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = Clock.UtcNow,
                IdentityVerified = null,
                EmailAddress = emailAddress,
                OneLoginUserSubject = null,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = [firstName, middleName, lastName],
                DateOfBirth = dateOfBirth,
                NationalInsuranceNumber = nationalInsuranceNumber
            };

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.GetSuggestedMatchesFromTrnRequestAsync(requestData);

            // Assert
            Assert.Collection(
                result,
                r => Assert.Equal(person1.PersonId, r.PersonId),
                r => Assert.Equal(person2.PersonId, r.PersonId),
                r => Assert.Equal(person3.PersonId, r.PersonId));
        });

    public static TrnRequestTheoryData GetMatchFromTrnRequestData()
    {
        var data = new TrnRequestTheoryData();

        // Definite matches

        data.AddCase(
            TrnRequestMatchResultOutcome.DefiniteMatch,
            dateOfBirth: DateOfBirthArgumentOption.Matches,
            nationalInsuranceNumber: NationalInsuranceNumberArgumentOption.MatchesPersonNino);

        data.AddCase(
            TrnRequestMatchResultOutcome.DefiniteMatch,
            dateOfBirth: DateOfBirthArgumentOption.Matches,
            nationalInsuranceNumber: NationalInsuranceNumberArgumentOption.MatchesEmploymentNino);

        var allSubsets = MatchableAttributes.Subsets().ToList();

        // Match on 3 or more attributes

        foreach (var matchedAttrs in allSubsets)
        {
            if (GetDistinctAttributeTypes(matchedAttrs).Count < 3)
            {
                continue;
            }

            // Can't match on both FirstName and FirstNameAlias
            if (matchedAttrs.Contains("FirstName") && matchedAttrs.Contains("FirstNameAlias"))
            {
                continue;
            }

            // Exclude definite match cases
            if (matchedAttrs.Contains("DateOfBirth") &&
                (matchedAttrs.Contains("NationalInsuranceNumber") || matchedAttrs.Contains("WorkforceNationalInsuranceNumber")))
            {
                continue;
            }

            data.AddCase(
                TrnRequestMatchResultOutcome.PotentialMatches,
                matchedAttrs.Contains("EmailAddress") ? EmailAddressArgumentOption.Matches : EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? FirstNameArgumentOption.Matches : matchedAttrs.Contains("FirstNameAlias") ? FirstNameArgumentOption.MatchesAlias : FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? MiddleNameArgumentOption.Matches : MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? LastNameArgumentOption.Matches : LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? DateOfBirthArgumentOption.Matches : DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? GenderArgumentOption.Matches : GenderArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("NationalInsuranceNumber")
                    ? NationalInsuranceNumberArgumentOption.MatchesPersonNino
                    : matchedAttrs.Contains("WorkforceNationalInsuranceNumber")
                        ? NationalInsuranceNumberArgumentOption.MatchesEmploymentNino
                        : NationalInsuranceNumberArgumentOption.DoesNotMatch);
        }

        // Match on 2 or fewer attributes

        foreach (var matchedAttrs in allSubsets)
        {
            if (GetDistinctAttributeTypes(matchedAttrs).Count >= 3)
            {
                continue;
            }

            // Can't match on both FirstName and FirstNameAlias
            if (matchedAttrs.Contains("FirstName") && matchedAttrs.Contains("FirstNameAlias"))
            {
                continue;
            }

            // Exclude definite match cases
            if (matchedAttrs.Contains("DateOfBirth") &&
                (matchedAttrs.Contains("NationalInsuranceNumber") || matchedAttrs.Contains("WorkforceNationalInsuranceNumber")))
            {
                continue;
            }

            // NINO and email matches are always potential matches
            if (matchedAttrs.Contains("NationalInsuranceNumber") ||
                matchedAttrs.Contains("WorkforceNationalInsuranceNumber") ||
                matchedAttrs.Contains("EmailAddress"))
            {
                continue;
            }

            data.AddCase(
                TrnRequestMatchResultOutcome.NoMatches,
                matchedAttrs.Contains("EmailAddress") ? EmailAddressArgumentOption.Matches : EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? FirstNameArgumentOption.Matches
                    : matchedAttrs.Contains("FirstNameAlias") ? FirstNameArgumentOption.MatchesAlias : FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? MiddleNameArgumentOption.Matches : MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? LastNameArgumentOption.Matches : LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? DateOfBirthArgumentOption.Matches : DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? GenderArgumentOption.Matches : GenderArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("NationalInsuranceNumber") ? NationalInsuranceNumberArgumentOption.MatchesPersonNino
                    : matchedAttrs.Contains("WorkforceNationalInsuranceNumber") ? NationalInsuranceNumberArgumentOption.MatchesEmploymentNino : NationalInsuranceNumberArgumentOption.DoesNotMatch);
        }

        return data;
    }

    public static TrnRequestTheoryData GetMatchFromTrnRequestDataWithMissingNino()
    {
        var data = new TrnRequestTheoryData();

        // Definite matches

        data.AddCase(
            TrnRequestMatchResultOutcome.DefiniteMatch,
            firstName: FirstNameArgumentOption.Matches,
            lastName: LastNameArgumentOption.Matches,
            dateOfBirth: DateOfBirthArgumentOption.Matches,
            emailAddress: EmailAddressArgumentOption.Matches,
            gender: GenderArgumentOption.Matches,
            nationalInsuranceNumber: NationalInsuranceNumberArgumentOption.Null);

        data.AddCase(
            TrnRequestMatchResultOutcome.DefiniteMatch,
            firstName: FirstNameArgumentOption.Matches,
            lastName: LastNameArgumentOption.Matches,
            dateOfBirth: DateOfBirthArgumentOption.Matches,
            emailAddress: EmailAddressArgumentOption.Matches,
            gender: GenderArgumentOption.Matches,
            nationalInsuranceNumber: NationalInsuranceNumberArgumentOption.Empty);

        var allSubsetsExcludingNino = MatchableAttributes
            .Except(["NationalInsuranceNumber", "WorkforceNationalInsuranceNumber"])
            .Subsets().ToList();

        // Match on 3 or more attributes

        foreach (var matchedAttrs in allSubsetsExcludingNino)
        {
            if (GetDistinctAttributeTypes(matchedAttrs).Count < 3)
            {
                continue;
            }

            // Can't match on both FirstName and FirstNameAlias
            if (matchedAttrs.Contains("FirstName") && matchedAttrs.Contains("FirstNameAlias"))
            {
                continue;
            }

            // Exclude definite match cases
            if ((matchedAttrs.Contains("FirstName") || matchedAttrs.Contains("FirstNameAlias")) &&
                matchedAttrs.Contains("LastName") &&
                matchedAttrs.Contains("DateOfBirth") &&
                matchedAttrs.Contains("EmailAddress") &&
                matchedAttrs.Contains("Gender"))
            {
                continue;
            }

            data.AddCase(
                TrnRequestMatchResultOutcome.PotentialMatches,
                matchedAttrs.Contains("EmailAddress") ? EmailAddressArgumentOption.Matches : EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? FirstNameArgumentOption.Matches : matchedAttrs.Contains("FirstNameAlias") ? FirstNameArgumentOption.MatchesAlias : FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? MiddleNameArgumentOption.Matches : MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? LastNameArgumentOption.Matches : LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? DateOfBirthArgumentOption.Matches : DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? GenderArgumentOption.Matches : GenderArgumentOption.DoesNotMatch,
                NationalInsuranceNumberArgumentOption.Null);

            data.AddCase(
                TrnRequestMatchResultOutcome.PotentialMatches,
                matchedAttrs.Contains("EmailAddress") ? EmailAddressArgumentOption.Matches : EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? FirstNameArgumentOption.Matches : matchedAttrs.Contains("FirstNameAlias") ? FirstNameArgumentOption.MatchesAlias : FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? MiddleNameArgumentOption.Matches : MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? LastNameArgumentOption.Matches : LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? DateOfBirthArgumentOption.Matches : DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? GenderArgumentOption.Matches : GenderArgumentOption.DoesNotMatch,
                NationalInsuranceNumberArgumentOption.Empty);
        }

        // Match on 2 or fewer attributes

        foreach (var matchedAttrs in allSubsetsExcludingNino)
        {
            if (GetDistinctAttributeTypes(matchedAttrs).Count >= 3)
            {
                continue;
            }

            // Can't match on both FirstName and FirstNameAlias
            if (matchedAttrs.Contains("FirstName") && matchedAttrs.Contains("FirstNameAlias"))
            {
                continue;
            }

            // Exclude definite match cases
            if ((matchedAttrs.Contains("FirstName") || matchedAttrs.Contains("FirstNameAlias")) &&
                matchedAttrs.Contains("LastName") &&
                matchedAttrs.Contains("DateOfBirth") &&
                matchedAttrs.Contains("EmailAddress") &&
                matchedAttrs.Contains("Gender"))
            {
                continue;
            }

            // Email matches is always a potential match
            if (matchedAttrs.Contains("EmailAddress"))
            {
                continue;
            }

            data.AddCase(
                TrnRequestMatchResultOutcome.NoMatches,
                matchedAttrs.Contains("EmailAddress") ? EmailAddressArgumentOption.Matches : EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? FirstNameArgumentOption.Matches
                    : matchedAttrs.Contains("FirstNameAlias") ? FirstNameArgumentOption.MatchesAlias : FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? MiddleNameArgumentOption.Matches : MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? LastNameArgumentOption.Matches : LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? DateOfBirthArgumentOption.Matches : DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? GenderArgumentOption.Matches : GenderArgumentOption.DoesNotMatch,
                NationalInsuranceNumberArgumentOption.Null);

            data.AddCase(
                TrnRequestMatchResultOutcome.NoMatches,
                matchedAttrs.Contains("EmailAddress") ? EmailAddressArgumentOption.Matches : EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? FirstNameArgumentOption.Matches
                    : matchedAttrs.Contains("FirstNameAlias") ? FirstNameArgumentOption.MatchesAlias : FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? MiddleNameArgumentOption.Matches : MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? LastNameArgumentOption.Matches : LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? DateOfBirthArgumentOption.Matches : DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? GenderArgumentOption.Matches : GenderArgumentOption.DoesNotMatch,
                NationalInsuranceNumberArgumentOption.Empty);
        }

        return data;
    }

    public class TrnRequestTheoryData : TheoryData<
        EmailAddressArgumentOption,
        FirstNameArgumentOption,
        MiddleNameArgumentOption,
        LastNameArgumentOption,
        DateOfBirthArgumentOption,
        NationalInsuranceNumberArgumentOption,
        GenderArgumentOption,
        TrnRequestMatchResultOutcome>
    {
        public void AddCase(
            TrnRequestMatchResultOutcome expectedOutcome,
            EmailAddressArgumentOption emailAddress = EmailAddressArgumentOption.DoesNotMatch,
            FirstNameArgumentOption firstName = FirstNameArgumentOption.DoesNotMatch,
            MiddleNameArgumentOption middleName = MiddleNameArgumentOption.DoesNotMatch,
            LastNameArgumentOption lastName = LastNameArgumentOption.DoesNotMatch,
            DateOfBirthArgumentOption dateOfBirth = DateOfBirthArgumentOption.DoesNotMatch,
            GenderArgumentOption gender = GenderArgumentOption.DoesNotMatch,
            NationalInsuranceNumberArgumentOption nationalInsuranceNumber = NationalInsuranceNumberArgumentOption.DoesNotMatch)
        {
            Add(emailAddress, firstName, middleName, lastName, dateOfBirth, nationalInsuranceNumber, gender, expectedOutcome);
        }
    }

    private static readonly string[] MatchableAttributes =
        [
            "FirstName",
            "FirstNameAlias",
            "MiddleName",
            "LastName",
            "DateOfBirth",
            "EmailAddress",
            "NationalInsuranceNumber",
            "WorkforceNationalInsuranceNumber",
            // Adding gender as it's used to determine a definite match if NINO is not provided.
            "Gender"
        ];

    private static ISet<string> GetDistinctAttributeTypes(IEnumerable<string> attributes)
    {
        var attrNames = new HashSet<string>();

        foreach (var attr in attributes)
        {
            // Gender does not count towards the initial matching pass so we exclude it from the count of distinct attributes.
            if (attr == "Gender")
            {
                continue;
            }

            if (attr == "FirstNameAlias")
            {
                attrNames.Add("FirstName");
                continue;
            }

            if (attr == "WorkforceNationalInsuranceNumber")
            {
                attrNames.Add("NationalInsuranceNumber");
                continue;
            }

            attrNames.Add(attr);
        }

        return attrNames;
    }

    public static class TrnRequest
    {
        public enum FirstNameArgumentOption
        {
            Matches,
            MatchesAlias,
            DoesNotMatch
        }

        public enum MiddleNameArgumentOption
        {
            Matches,
            DoesNotMatch
        }

        public enum LastNameArgumentOption
        {
            Matches,
            DoesNotMatch
        }

        public enum DateOfBirthArgumentOption
        {
            Matches,
            DoesNotMatch
        }

        public enum EmailAddressArgumentOption
        {
            Matches,
            DoesNotMatch
        }

        public enum GenderArgumentOption
        {
            Matches,
            DoesNotMatch
        }

        public enum NationalInsuranceNumberArgumentOption
        {
            Null,
            Empty,
            MatchesPersonNino,
            MatchesEmploymentNino,
            DoesNotMatch
        }
    }
}

