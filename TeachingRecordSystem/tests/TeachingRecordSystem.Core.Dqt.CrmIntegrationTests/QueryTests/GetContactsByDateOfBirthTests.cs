using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetContactsByDateOfBirthTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetContactsByDateOfBirthTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    public static TheoryData<ContactSearchSortScenarioData> GetContactSearchSortScenarioData()
    {
        return new TheoryData<ContactSearchSortScenarioData>
        {
            new ContactSearchSortScenarioData
            {
                SortBy = ContactSearchSortByOption.LastNameAscending,
                Selector = (Contact c) => c.LastName,
                IsAscending = true
            },
            new ContactSearchSortScenarioData
            {
                SortBy = ContactSearchSortByOption.LastNameDescending,
                Selector = (Contact c) => c.LastName,
                IsAscending = false
            },
            new ContactSearchSortScenarioData
            {
                SortBy = ContactSearchSortByOption.FirstNameAscending,
                Selector = (Contact c) => c.FirstName,
                IsAscending = true
            },
            new ContactSearchSortScenarioData
            {
                SortBy = ContactSearchSortByOption.FirstNameDescending,
                Selector = (Contact c) => c.FirstName,
                IsAscending = false
            },
            new ContactSearchSortScenarioData
            {
                SortBy = ContactSearchSortByOption.DateOfBirthAscending,
                Selector = (Contact c) => c.BirthDate is null ? string.Empty : c.BirthDate.Value.ToString("yyyy-MM-dd"),
                IsAscending = true
            },
            new ContactSearchSortScenarioData
            {
                SortBy = ContactSearchSortByOption.DateOfBirthDescending,
                Selector = (Contact c) => c.BirthDate is null ? string.Empty : c.BirthDate.Value.ToString("yyyy-MM-dd"),
                IsAscending = false
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetContactSearchSortScenarioData))]
    public async Task ReturnsMatchingContactsFromCrmInExpectedSortOrder(ContactSearchSortScenarioData testScenarioData)
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var maxRecordCount = 3;
        var columnSet = new ColumnSet(
            Contact.Fields.dfeta_TRN,
            Contact.Fields.BirthDate,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.FullName);


        var person1 = await _dataScope.TestData.CreatePersonAsync(p => p.WithTrn().WithDateOfBirth(dateOfBirth));
        var person2 = await _dataScope.TestData.CreatePersonAsync(p => p.WithTrn().WithDateOfBirth(dateOfBirth));
        var person3 = await _dataScope.TestData.CreatePersonAsync(p => p.WithTrn().WithDateOfBirth(dateOfBirth));

        // Act
        var results = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactsByDateOfBirthQuery(dateOfBirth, testScenarioData.SortBy, maxRecordCount, columnSet));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(maxRecordCount, results.Length);
        if (testScenarioData.IsAscending)
        {
            Assert.Equal(results.Select(testScenarioData.Selector).OrderBy(x => x).ToArray(), results.Select(testScenarioData.Selector).ToArray());
        }
        else
        {
            Assert.Equal(results.Select(testScenarioData.Selector).OrderByDescending(x => x).ToArray(), results.Select(testScenarioData.Selector).ToArray());
        }
    }

    public class ContactSearchSortScenarioData
    {
        public required ContactSearchSortByOption SortBy { get; init; }
        public required Func<Contact, string> Selector { get; init; }
        public required bool IsAscending { get; init; }
    }
}
