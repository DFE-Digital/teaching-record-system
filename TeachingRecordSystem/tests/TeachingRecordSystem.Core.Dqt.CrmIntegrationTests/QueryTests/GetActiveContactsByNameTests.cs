using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetActiveContactsByNameTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetActiveContactsByNameTests(CrmClientFixture crmClientFixture)
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
        var name = "andrew";
        var maxRecordCount = 4; // pretty safe bet there will always be at least 4 andrew names in the dev CRM database
        var columnSet = new ColumnSet(
            Contact.Fields.dfeta_TRN,
            Contact.Fields.BirthDate,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.FullName);

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactsByNameQuery(name, testScenarioData.SortBy, maxRecordCount, columnSet));

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
