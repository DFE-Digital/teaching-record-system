using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;

public abstract class ResolveNpqTrnRequestTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithAllDifferences(Guid applicationUserId)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithTrn().WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedRecords(matchedPerson.PersonId)
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!)));

        return (supportTask, matchedPerson);
    }

    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithSingleDifferenceToMatch(
        Guid applicationUserId,
        PersonMatchedAttribute differentAttribute)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithNationalInsuranceNumber()
            .WithEmail(TestData.GenerateUniqueEmail()));

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedRecords(matchedPerson.PersonId)
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

