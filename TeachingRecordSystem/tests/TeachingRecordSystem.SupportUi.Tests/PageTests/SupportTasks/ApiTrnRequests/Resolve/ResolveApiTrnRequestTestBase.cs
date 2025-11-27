using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public abstract class ResolveApiTrnRequestTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithAllDifferences(Guid applicationUserId)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(TestData.GenerateChangedFirstName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithMiddleName(TestData.GenerateChangedMiddleName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithLastName(TestData.GenerateChangedLastName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!))
                .WithGender(TestData.GenerateGender()));

        return (supportTask, matchedPerson);
    }

    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithSingleDifferenceToMatch(
        Guid applicationUserId,
        PersonMatchedAttribute differentAttribute,
        bool matchedPersonHasFlags = false)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p =>
        {
            p

                .WithNationalInsuranceNumber()
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithGender(TestData.GenerateGender());

            if (matchedPersonHasFlags)
            {
                p.WithQts().WithEyts().WithAlert();
            }
        });

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(
                    differentAttribute != PersonMatchedAttribute.FirstName
                        ? matchedPerson.FirstName
                        : TestData.GenerateChangedFirstName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithMiddleName(
                    differentAttribute != PersonMatchedAttribute.MiddleName
                        ? matchedPerson.MiddleName
                        : TestData.GenerateChangedMiddleName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithLastName(
                    differentAttribute != PersonMatchedAttribute.LastName
                        ? matchedPerson.LastName
                        : TestData.GenerateChangedLastName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithDateOfBirth(
                    differentAttribute != PersonMatchedAttribute.DateOfBirth
                        ? matchedPerson.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(
                    differentAttribute != PersonMatchedAttribute.EmailAddress
                        ? matchedPerson.EmailAddress
                        : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(
                    differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                        ? matchedPerson.NationalInsuranceNumber
                        : TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!))
                .WithGender(
                    differentAttribute != PersonMatchedAttribute.Gender
                        ? matchedPerson.Gender
                        : TestData.GenerateChangedGender(matchedPerson.Gender)));

        return (supportTask, matchedPerson);
    }
}

