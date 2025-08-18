using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergeTestBase(HostFixture hostFixture)
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
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithEmail(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues)));

        var personB = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithPersonDataSource(TestDataPersonDataSource.Trs)
                .WithTrn()
                .WithFirstName(personA.FirstName)
                .WithMiddleName(personA.MiddleName)
                .WithLastName(personA.LastName)
                .WithDateOfBirth(personA.DateOfBirth);

            if (useNullValues)
            {
                p
                    .WithEmail(false)
                    .WithNationalInsuranceNumber(false)
                    .WithGender(false);
            }
            else
            {
                p
                    .WithEmail(personA.Email)
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
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithEmail(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithFirstName(TestData.GenerateChangedFirstName(personA.FirstName))
            .WithMiddleName(TestData.GenerateChangedMiddleName(personA.MiddleName))
            .WithLastName(TestData.GenerateChangedLastName(personA.LastName))
            .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(personA.DateOfBirth))
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber ?? ""))
            .WithGender(TestData.GenerateChangedGender(personA.Gender ?? Gender.Other)));

        return (personA, personB);
    }

    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithSingleDifferenceToMatch(
        PersonMatchedAttribute differentAttribute,
        bool useNullValues = false)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithEmail(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personB = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithPersonDataSource(TestDataPersonDataSource.Trs)
                .WithTrn()
                .WithFirstName(
                    differentAttribute != PersonMatchedAttribute.FirstName
                        ? personA.FirstName
                        : TestData.GenerateChangedFirstName(personA.FirstName))
                .WithMiddleName(
                    differentAttribute != PersonMatchedAttribute.MiddleName
                        ? personA.MiddleName
                        : TestData.GenerateChangedMiddleName(personA.MiddleName))
                .WithLastName(
                    differentAttribute != PersonMatchedAttribute.LastName
                        ? personA.LastName
                        : TestData.GenerateChangedLastName(personA.LastName))
                .WithDateOfBirth(
                    differentAttribute != PersonMatchedAttribute.DateOfBirth
                        ? personA.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(personA.DateOfBirth));

            if (useNullValues)
            {
                p
                    .WithEmail(differentAttribute != PersonMatchedAttribute.EmailAddress
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
                    .WithEmail(differentAttribute != PersonMatchedAttribute.EmailAddress
                        ? personA.Email
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
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithEmail(!useNullValues)
            .WithNationalInsuranceNumber(!useNullValues)
            .WithGender(!useNullValues));

        var personB = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithPersonDataSource(TestDataPersonDataSource.Trs)
                .WithTrn()
                .WithFirstName(
                    matchedAttributes.Contains(PersonMatchedAttribute.FirstName)
                        ? personA.FirstName
                        : TestData.GenerateChangedFirstName(personA.FirstName))
                .WithMiddleName(
                    matchedAttributes.Contains(PersonMatchedAttribute.MiddleName)
                        ? personA.MiddleName
                        : TestData.GenerateChangedMiddleName(personA.MiddleName))
                .WithLastName(
                    matchedAttributes.Contains(PersonMatchedAttribute.LastName)
                        ? personA.LastName
                        : TestData.GenerateChangedLastName(personA.LastName))
                .WithDateOfBirth(
                    matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth)
                        ? personA.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(personA.DateOfBirth));

            if (useNullValues)
            {
                p
                    .WithEmail(matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress)
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
                    .WithEmail(matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress)
                        ? personA.Email
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
