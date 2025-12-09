using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Tests.Services.TrnRequests;

public partial class TrnRequestServiceTests
{
    [Fact]
    public async Task MatchPersonsAsync_WithOutOfOrderNames_ReturnsMatch()
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

        // Act
        var result = await WithServiceAsync(s => s.MatchPersonsAsync(requestData));

        // Assert
        Assert.Contains(person.PersonId, result.PotentialMatchesPersonIds);
    }

    [Fact]
    public async Task MatchPersonsAsync_WithOneMatchOnNinoAndDobAndOtherMatchesOnDifferentFields_ReturnDefiniteMatch()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var dateOfBirth = TestData.GenerateDateOfBirth();

        var person1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(nationalInsuranceNumber)));

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
            DateOfBirth = dateOfBirth,
            NationalInsuranceNumber = nationalInsuranceNumber
        };

        // Act
        var result = await WithServiceAsync(s => s.MatchPersonsAsync(requestData));

        // Assert
        Assert.Equal(MatchPersonsResultOutcome.DefiniteMatch, result.Outcome);
        Assert.Equal(person1.PersonId, result.PersonId);
    }

    [Theory]
    [MemberData(nameof(GetMatchFromTrnRequestData))]
    [MemberData(nameof(GetMatchFromTrnRequestDataWithMissingNino))]
    public async Task MatchPersonsAsync_ReturnsExpectedResult(
        TrnRequest.EmailAddressArgumentOption emailAddressOption,
        TrnRequest.FirstNameArgumentOption firstNameOption,
        TrnRequest.MiddleNameArgumentOption middleNameOption,
        TrnRequest.LastNameArgumentOption lastNameOption,
        TrnRequest.DateOfBirthArgumentOption dateOfBirthOption,
        TrnRequest.NationalInsuranceNumberArgumentOption nationalInsuranceNumberOption,
        TrnRequest.GenderArgumentOption genderOption,
        MatchPersonsResultOutcome expectedOutcome)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var personFirstName = TestData.GenerateFirstName();

        var alias = firstNameOption == TrnRequest.FirstNameArgumentOption.MatchesAlias ? TestData.GenerateChangedFirstName(personFirstName) : null;
        if (alias is not null)
        {
            await WithDbContextAsync(async dbContext =>
            {
                dbContext.NameSynonyms.Add(new NameSynonyms { Name = personFirstName, Synonyms = [alias] });
                dbContext.NameSynonyms.Add(new NameSynonyms { Name = alias, Synonyms = [personFirstName] });
                await dbContext.SaveChangesAsync();
            });
        }

        var personMiddleName = TestData.GenerateChangedMiddleName([personFirstName, alias]);

        var person = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber()
            .WithFirstName(personFirstName)
            .WithMiddleName(personMiddleName)
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithGender());
        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321", establishmentNumber: "4321", establishmentStatusCode: 1);
        var employmentNino = TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
        var personEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment, new DateOnly(2023, 08, 03), new DateOnly(2024, 05, 25), EmploymentType.FullTime, new DateOnly(2024, 05, 25), employmentNino);

        var emailAddress = emailAddressOption switch
        {
            TrnRequest.EmailAddressArgumentOption.Matches => person.EmailAddress!,
            _ => TestData.GenerateUniqueEmail()
        };

        var firstName = firstNameOption switch
        {
            TrnRequest.FirstNameArgumentOption.Matches => person.FirstName,
            TrnRequest.FirstNameArgumentOption.MatchesAlias => alias!,
            _ => TestData.GenerateChangedFirstName([person.FirstName, alias, person.MiddleName, person.LastName])
        };

        var middleName = middleNameOption switch
        {
            TrnRequest.MiddleNameArgumentOption.Matches => person.MiddleName,
            _ => TestData.GenerateChangedMiddleName([person.FirstName, alias, person.MiddleName, person.LastName])
        };

        var lastName = lastNameOption switch
        {
            TrnRequest.LastNameArgumentOption.Matches => person.LastName,
            _ => TestData.GenerateChangedLastName([person.FirstName, alias, person.MiddleName, person.LastName])
        };

        var dateOfBirth = dateOfBirthOption switch
        {
            TrnRequest.DateOfBirthArgumentOption.Matches => person.DateOfBirth,
            _ => TestData.GenerateChangedDateOfBirth(person.DateOfBirth)
        };

        var nationalInsuranceNumber = nationalInsuranceNumberOption switch
        {
            TrnRequest.NationalInsuranceNumberArgumentOption.Null => null,
            TrnRequest.NationalInsuranceNumberArgumentOption.Empty => string.Empty,
            TrnRequest.NationalInsuranceNumberArgumentOption.MatchesPersonNino => person.NationalInsuranceNumber!,
            TrnRequest.NationalInsuranceNumberArgumentOption.MatchesEmploymentNino => personEmployment.NationalInsuranceNumber!,
            _ => TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!)
        };

        var gender = genderOption switch
        {
            TrnRequest.GenderArgumentOption.Matches => person.Gender,
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

        // Act
        var result = await WithServiceAsync(s => s.MatchPersonsAsync(requestData));

        // Assert
        if (expectedOutcome != result.Outcome)
        {
            var outputHelper = TestContext.Current.TestOutputHelper!;
            var pad = new[] { person.EmailAddress, person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth.ToShortDateString(), person.NationalInsuranceNumber, person.Gender.ToString() }
                .Max(s => s?.Length ?? 0);
            outputHelper.WriteLine($"                         {"Record".PadRight(pad)} Request");
            outputHelper.WriteLine($"EmailAddress:            {(person.EmailAddress ?? "").PadRight(pad)} {emailAddress}");
            outputHelper.WriteLine($"FirstName:               {(person.FirstName ?? "").PadRight(pad)} {firstName}");
            outputHelper.WriteLine($"MiddleName:              {(person.MiddleName ?? "").PadRight(pad)} {middleName}");
            outputHelper.WriteLine($"LastName:                {(person.LastName ?? "").PadRight(pad)} {lastName}");
            outputHelper.WriteLine($"DateOfBirth:             {(person.DateOfBirth.ToShortDateString() ?? "").PadRight(pad)} {dateOfBirth}");
            outputHelper.WriteLine($"NationalInsuranceNumber: {(person.NationalInsuranceNumber ?? "").PadRight(pad)} {nationalInsuranceNumber}");
            outputHelper.WriteLine($"Gender:                  {(person.Gender?.ToString() ?? "").PadRight(pad)} {gender}");
        }

        Assert.Equal(expectedOutcome, result.Outcome);

        if (expectedOutcome == MatchPersonsResultOutcome.DefiniteMatch)
        {
            Assert.Equal(person.PersonId, result.PersonId);
        }
    }

    public static TrnRequestTheoryData GetMatchFromTrnRequestData()
    {
        var data = new TrnRequestTheoryData();

        // Definite matches

        data.AddCase(
            MatchPersonsResultOutcome.DefiniteMatch,
            dateOfBirth: TrnRequest.DateOfBirthArgumentOption.Matches,
            nationalInsuranceNumber: TrnRequest.NationalInsuranceNumberArgumentOption.MatchesPersonNino);

        data.AddCase(
            MatchPersonsResultOutcome.DefiniteMatch,
            dateOfBirth: TrnRequest.DateOfBirthArgumentOption.Matches,
            nationalInsuranceNumber: TrnRequest.NationalInsuranceNumberArgumentOption.MatchesEmploymentNino);

        var allSubsets = _matchableAttributes.Subsets().ToList();

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
                MatchPersonsResultOutcome.PotentialMatches,
                matchedAttrs.Contains("EmailAddress") ? TrnRequest.EmailAddressArgumentOption.Matches : TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? TrnRequest.FirstNameArgumentOption.Matches : matchedAttrs.Contains("FirstNameAlias") ? TrnRequest.FirstNameArgumentOption.MatchesAlias : TrnRequest.FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? TrnRequest.MiddleNameArgumentOption.Matches : TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? TrnRequest.LastNameArgumentOption.Matches : TrnRequest.LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? TrnRequest.DateOfBirthArgumentOption.Matches : TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? TrnRequest.GenderArgumentOption.Matches : TrnRequest.GenderArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("NationalInsuranceNumber")
                    ? TrnRequest.NationalInsuranceNumberArgumentOption.MatchesPersonNino
                    : matchedAttrs.Contains("WorkforceNationalInsuranceNumber")
                        ? TrnRequest.NationalInsuranceNumberArgumentOption.MatchesEmploymentNino
                        : TrnRequest.NationalInsuranceNumberArgumentOption.DoesNotMatch);
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
                MatchPersonsResultOutcome.NoMatches,
                matchedAttrs.Contains("EmailAddress") ? TrnRequest.EmailAddressArgumentOption.Matches : TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? TrnRequest.FirstNameArgumentOption.Matches
                    : matchedAttrs.Contains("FirstNameAlias") ? TrnRequest.FirstNameArgumentOption.MatchesAlias : TrnRequest.FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? TrnRequest.MiddleNameArgumentOption.Matches : TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? TrnRequest.LastNameArgumentOption.Matches : TrnRequest.LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? TrnRequest.DateOfBirthArgumentOption.Matches : TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? TrnRequest.GenderArgumentOption.Matches : TrnRequest.GenderArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("NationalInsuranceNumber") ? TrnRequest.NationalInsuranceNumberArgumentOption.MatchesPersonNino
                    : matchedAttrs.Contains("WorkforceNationalInsuranceNumber") ? TrnRequest.NationalInsuranceNumberArgumentOption.MatchesEmploymentNino : TrnRequest.NationalInsuranceNumberArgumentOption.DoesNotMatch);
        }

        return data;
    }

    public static TrnRequestTheoryData GetMatchFromTrnRequestDataWithMissingNino()
    {
        var data = new TrnRequestTheoryData();

        // Definite matches

        data.AddCase(
            MatchPersonsResultOutcome.DefiniteMatch,
            firstName: TrnRequest.FirstNameArgumentOption.Matches,
            lastName: TrnRequest.LastNameArgumentOption.Matches,
            dateOfBirth: TrnRequest.DateOfBirthArgumentOption.Matches,
            emailAddress: TrnRequest.EmailAddressArgumentOption.Matches,
            gender: TrnRequest.GenderArgumentOption.Matches,
            nationalInsuranceNumber: TrnRequest.NationalInsuranceNumberArgumentOption.Null);

        data.AddCase(
            MatchPersonsResultOutcome.DefiniteMatch,
            firstName: TrnRequest.FirstNameArgumentOption.Matches,
            lastName: TrnRequest.LastNameArgumentOption.Matches,
            dateOfBirth: TrnRequest.DateOfBirthArgumentOption.Matches,
            emailAddress: TrnRequest.EmailAddressArgumentOption.Matches,
            gender: TrnRequest.GenderArgumentOption.Matches,
            nationalInsuranceNumber: TrnRequest.NationalInsuranceNumberArgumentOption.Empty);

        var allSubsetsExcludingNino = _matchableAttributes
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
                MatchPersonsResultOutcome.PotentialMatches,
                matchedAttrs.Contains("EmailAddress") ? TrnRequest.EmailAddressArgumentOption.Matches : TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? TrnRequest.FirstNameArgumentOption.Matches : matchedAttrs.Contains("FirstNameAlias") ? TrnRequest.FirstNameArgumentOption.MatchesAlias : TrnRequest.FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? TrnRequest.MiddleNameArgumentOption.Matches : TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? TrnRequest.LastNameArgumentOption.Matches : TrnRequest.LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? TrnRequest.DateOfBirthArgumentOption.Matches : TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? TrnRequest.GenderArgumentOption.Matches : TrnRequest.GenderArgumentOption.DoesNotMatch,
                TrnRequest.NationalInsuranceNumberArgumentOption.Null);

            data.AddCase(
                MatchPersonsResultOutcome.PotentialMatches,
                matchedAttrs.Contains("EmailAddress") ? TrnRequest.EmailAddressArgumentOption.Matches : TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? TrnRequest.FirstNameArgumentOption.Matches : matchedAttrs.Contains("FirstNameAlias") ? TrnRequest.FirstNameArgumentOption.MatchesAlias : TrnRequest.FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? TrnRequest.MiddleNameArgumentOption.Matches : TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? TrnRequest.LastNameArgumentOption.Matches : TrnRequest.LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? TrnRequest.DateOfBirthArgumentOption.Matches : TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? TrnRequest.GenderArgumentOption.Matches : TrnRequest.GenderArgumentOption.DoesNotMatch,
                TrnRequest.NationalInsuranceNumberArgumentOption.Empty);
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
                MatchPersonsResultOutcome.NoMatches,
                matchedAttrs.Contains("EmailAddress") ? TrnRequest.EmailAddressArgumentOption.Matches : TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? TrnRequest.FirstNameArgumentOption.Matches
                    : matchedAttrs.Contains("FirstNameAlias") ? TrnRequest.FirstNameArgumentOption.MatchesAlias : TrnRequest.FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? TrnRequest.MiddleNameArgumentOption.Matches : TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? TrnRequest.LastNameArgumentOption.Matches : TrnRequest.LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? TrnRequest.DateOfBirthArgumentOption.Matches : TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? TrnRequest.GenderArgumentOption.Matches : TrnRequest.GenderArgumentOption.DoesNotMatch,
                TrnRequest.NationalInsuranceNumberArgumentOption.Null);

            data.AddCase(
                MatchPersonsResultOutcome.NoMatches,
                matchedAttrs.Contains("EmailAddress") ? TrnRequest.EmailAddressArgumentOption.Matches : TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("FirstName") ? TrnRequest.FirstNameArgumentOption.Matches
                    : matchedAttrs.Contains("FirstNameAlias") ? TrnRequest.FirstNameArgumentOption.MatchesAlias : TrnRequest.FirstNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("MiddleName") ? TrnRequest.MiddleNameArgumentOption.Matches : TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("LastName") ? TrnRequest.LastNameArgumentOption.Matches : TrnRequest.LastNameArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("DateOfBirth") ? TrnRequest.DateOfBirthArgumentOption.Matches : TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
                matchedAttrs.Contains("Gender") ? TrnRequest.GenderArgumentOption.Matches : TrnRequest.GenderArgumentOption.DoesNotMatch,
                TrnRequest.NationalInsuranceNumberArgumentOption.Empty);
        }

        return data;
    }

    public class TrnRequestTheoryData : TheoryData<
        TrnRequest.EmailAddressArgumentOption,
        TrnRequest.FirstNameArgumentOption,
        TrnRequest.MiddleNameArgumentOption,
        TrnRequest.LastNameArgumentOption,
        TrnRequest.DateOfBirthArgumentOption,
        TrnRequest.NationalInsuranceNumberArgumentOption,
        TrnRequest.GenderArgumentOption,
        MatchPersonsResultOutcome>
    {
        public void AddCase(
            MatchPersonsResultOutcome expectedOutcome,
            TrnRequest.EmailAddressArgumentOption emailAddress = TrnRequest.EmailAddressArgumentOption.DoesNotMatch,
            TrnRequest.FirstNameArgumentOption firstName = TrnRequest.FirstNameArgumentOption.DoesNotMatch,
            TrnRequest.MiddleNameArgumentOption middleName = TrnRequest.MiddleNameArgumentOption.DoesNotMatch,
            TrnRequest.LastNameArgumentOption lastName = TrnRequest.LastNameArgumentOption.DoesNotMatch,
            TrnRequest.DateOfBirthArgumentOption dateOfBirth = TrnRequest.DateOfBirthArgumentOption.DoesNotMatch,
            TrnRequest.GenderArgumentOption gender = TrnRequest.GenderArgumentOption.DoesNotMatch,
            TrnRequest.NationalInsuranceNumberArgumentOption nationalInsuranceNumber = TrnRequest.NationalInsuranceNumberArgumentOption.DoesNotMatch)
        {
            Add(emailAddress, firstName, middleName, lastName, dateOfBirth, nationalInsuranceNumber, gender, expectedOutcome);
        }
    }

    private static readonly string[] _matchableAttributes =
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
