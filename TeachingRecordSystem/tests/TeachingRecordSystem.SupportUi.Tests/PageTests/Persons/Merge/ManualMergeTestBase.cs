using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class ManualMergeTestBase(HostFixture hostFixture)
    : TestBase(hostFixture)
{
    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithNoDifferences(
        Action<CreatePersonBuilder>? configurePersonA = null,
        Action<CreatePersonBuilder>? configurePersonB = null)
    {
        configurePersonA ??= new Action<CreatePersonBuilder>(p => { });
        configurePersonB ??= new Action<CreatePersonBuilder>(p => { });

        var personA = await TestData.CreatePersonAsync(p => configurePersonA(p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()));

        var personB = await TestData.CreatePersonAsync(p => configurePersonB(p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithFirstName(personA.FirstName)
            .WithMiddleName(personA.MiddleName)
            .WithLastName(personA.LastName)
            .WithDateOfBirth(personA.DateOfBirth)
            .WithEmail(personA.Email)
            .WithNationalInsuranceNumber(personA.NationalInsuranceNumber ?? string.Empty)));

        return (personA, personB);
    }

    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithAllDifferences()
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithNationalInsuranceNumber()
            .WithEmail(TestData.GenerateUniqueEmail()));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithFirstName(TestData.GenerateChangedFirstName(personA.FirstName))
            .WithMiddleName(TestData.GenerateChangedMiddleName(personA.MiddleName))
            .WithLastName(TestData.GenerateChangedLastName(personA.LastName))
            .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(personA.DateOfBirth))
            .WithEmail(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber!)));

        return (personA, personB);
    }

    protected async Task<(CreatePersonResult PersonA, CreatePersonResult PersonB)> CreatePersonsWithSingleDifferenceToMatch(
        PersonMatchedAttribute differentAttribute)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithNationalInsuranceNumber()
            .WithEmail(TestData.GenerateUniqueEmail()));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
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
                    : TestData.GenerateChangedDateOfBirth(personA.DateOfBirth))
            .WithEmail(
                differentAttribute != PersonMatchedAttribute.EmailAddress
                    ? personA.Email
                    : TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber(
                differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                    ? personA.NationalInsuranceNumber ?? ""
                    : TestData.GenerateChangedNationalInsuranceNumber(personA.NationalInsuranceNumber!)));

        return (personA, personB);
    }
}
