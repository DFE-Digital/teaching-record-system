using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public abstract class ResolveApiTrnRequestTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithAllDifferences(Guid applicationUserId)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithTrn().WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedRecords(matchedPerson.PersonId)
                .WithFirstName(TestData.GenerateChangedFirstName(matchedPerson.FirstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(matchedPerson.MiddleName))
                .WithLastName(TestData.GenerateChangedLastName(matchedPerson.LastName))
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!)));

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
                .WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs)
                .WithTrn()
                .WithNationalInsuranceNumber()
                .WithEmail(TestData.GenerateUniqueEmail());

            if (matchedPersonHasFlags)
            {
                p.WithQts().WithEyts().WithAlert();
            }
        });

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedRecords(matchedPerson.PersonId)
                .WithFirstName(
                    differentAttribute != PersonMatchedAttribute.FirstName
                        ? matchedPerson.FirstName
                        : TestData.GenerateChangedFirstName(matchedPerson.FirstName))
                .WithMiddleName(
                    differentAttribute != PersonMatchedAttribute.MiddleName
                        ? matchedPerson.MiddleName
                        : TestData.GenerateChangedMiddleName(matchedPerson.MiddleName))
                .WithLastName(
                    differentAttribute != PersonMatchedAttribute.LastName
                        ? matchedPerson.LastName
                        : TestData.GenerateChangedLastName(matchedPerson.LastName))
                .WithDateOfBirth(
                    differentAttribute != PersonMatchedAttribute.DateOfBirth
                        ? matchedPerson.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(
                    differentAttribute != PersonMatchedAttribute.EmailAddress
                        ? matchedPerson.Email
                        : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(
                    differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                        ? matchedPerson.NationalInsuranceNumber
                        : TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!)));

        return (supportTask, matchedPerson);
    }
}

