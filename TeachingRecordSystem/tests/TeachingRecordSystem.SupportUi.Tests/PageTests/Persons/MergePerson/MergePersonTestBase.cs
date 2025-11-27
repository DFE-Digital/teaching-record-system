using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class MergePersonTestBase(HostFixture hostFixture)
    : TestBase(hostFixture)
{
    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithNoDifferences(
        Action<CreatePersonBuilder>? configurePersonA = null,
        Action<CreatePersonBuilder>? configurePersonB = null,
        bool useNullValues = false)
    {
        configurePersonA ??= new Action<CreatePersonBuilder>(p => { });
        configurePersonB ??= new Action<CreatePersonBuilder>(p => { });

        var personA = await TestData.CreatePersonAsync(p => configurePersonA(p
            .WithEmailAddress(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues)));

        var personB = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithFirstName(personA.FirstName)
                .WithMiddleName(personA.MiddleName)
                .WithLastName(personA.LastName)
                .WithDateOfBirth(personA.DateOfBirth);

            if (useNullValues)
            {
                p
                    .WithEmailAddress(false)
                    .WithNationalInsuranceNumber(false)
                    .WithGender(false);
            }
            else
            {
                p
                    .WithEmailAddress(personA.EmailAddress)
                    .WithNationalInsuranceNumber(personA.NationalInsuranceNumber!)
                    .WithGender(personA.Gender!.Value);
            }

            configurePersonB(p);
        });

        return (personA, personB);
    }

    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithAllDifferences(
        bool useNullValues = false)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithFirstName(TestData.GenerateChangedFirstName([personA.FirstName, personA.MiddleName, personA.LastName]))
            .WithMiddleName(TestData.GenerateChangedMiddleName([personA.FirstName, personA.MiddleName, personA.LastName]))
            .WithLastName(TestData.GenerateChangedLastName([personA.FirstName, personA.MiddleName, personA.LastName]))
            .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(personA.DateOfBirth))
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber ?? ""))
            .WithGender(TestData.GenerateChangedGender(personA.Gender ?? Gender.Other)));

        return (personA, personB);
    }

    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithSingleDifferenceToMatch(
        PersonMatchedAttribute differentAttribute,
        bool useNullValues = false)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personB = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithFirstName(
                    differentAttribute != PersonMatchedAttribute.FirstName
                        ? personA.FirstName
                        : TestData.GenerateChangedFirstName([personA.FirstName, personA.MiddleName, personA.LastName]))
                .WithMiddleName(
                    differentAttribute != PersonMatchedAttribute.MiddleName
                        ? personA.MiddleName
                        : TestData.GenerateChangedMiddleName([personA.FirstName, personA.MiddleName, personA.LastName]))
                .WithLastName(
                    differentAttribute != PersonMatchedAttribute.LastName
                        ? personA.LastName
                        : TestData.GenerateChangedLastName([personA.FirstName, personA.MiddleName, personA.LastName]))
                .WithDateOfBirth(
                    differentAttribute != PersonMatchedAttribute.DateOfBirth
                        ? personA.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(personA.DateOfBirth));

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
                        ? personA.EmailAddress
                        : TestData.GenerateUniqueEmail())
                    .WithNationalInsuranceNumber(differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                        ? personA.NationalInsuranceNumber!
                        : TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber!))
                    .WithGender(differentAttribute != PersonMatchedAttribute.Gender
                        ? personA.Gender!.Value
                        : TestData.GenerateChangedGender(personA.Gender!.Value));
            }
        });

        return (personA, personB);
    }

    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithMultipleDifferencesToMatch(
        IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes,
        bool useNullValues = false)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personB = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithFirstName(
                    matchedAttributes.Contains(PersonMatchedAttribute.FirstName)
                        ? personA.FirstName
                        : TestData.GenerateChangedFirstName([personA.FirstName, personA.MiddleName, personA.LastName]))
                .WithMiddleName(
                    matchedAttributes.Contains(PersonMatchedAttribute.MiddleName)
                        ? personA.MiddleName
                        : TestData.GenerateChangedMiddleName([personA.FirstName, personA.MiddleName, personA.LastName]))
                .WithLastName(
                    matchedAttributes.Contains(PersonMatchedAttribute.LastName)
                        ? personA.LastName
                        : TestData.GenerateChangedLastName([personA.FirstName, personA.MiddleName, personA.LastName]))
                .WithDateOfBirth(
                    matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth)
                        ? personA.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(personA.DateOfBirth));

            if (useNullValues)
            {
                p
                    .WithEmailAddress(matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress)
                        ? false
                        : true)
                    .WithNationalInsuranceNumber(matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                       ? false
                       : true)
                    .WithGender(matchedAttributes.Contains(PersonMatchedAttribute.Gender)
                        ? false
                        : true);
            }
            else
            {
                p
                    .WithEmailAddress(matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress)
                        ? personA.EmailAddress
                        : TestData.GenerateUniqueEmail())
                    .WithNationalInsuranceNumber(matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                        ? personA.NationalInsuranceNumber!
                        : TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber!))
                    .WithGender(matchedAttributes.Contains(PersonMatchedAttribute.Gender)
                        ? personA.Gender!.Value
                        : TestData.GenerateChangedGender(personA.Gender!.Value));
            }
        });

        return (personA, personB);
    }
}
