#nullable disable

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class FindTeachersTests : IClassFixture<FindTeachersFixture>
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly FindTeachersFixture _findTeachersFixture;

    public FindTeachersTests(FindTeachersFixture findTeachersFixture, CrmClientFixture crmClientFixture)
    {
        _findTeachersFixture = findTeachersFixture;
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    [Fact]
    public async Task Given_less_than_3_identifiers_returns_no_matches()
    {
        var queries = Enumerable.Range(0, 3).SelectMany(f => CreateQueries(matchingFieldsCount: f, populateNonMatchingFields: false));

        foreach (var query in queries)
        {
            var result = await _dataverseAdapter.FindTeachersAsync(query);
            AssertDoesNotContainMatch(result);
        }
    }

    [Fact]
    public async Task Given_3_matching_identifiers_returns_match()
    {
        var queries = CreateQueries(matchingFieldsCount: 3, populateNonMatchingFields: false);

        foreach (var query in queries)
        {
            var result = await _dataverseAdapter.FindTeachersAsync(query);
            AssertContainsMatch(result);
        }
    }

    [Fact]
    public async Task Given_4_matching_identifiers_returns_match()
    {
        var queries = CreateQueries(matchingFieldsCount: 4, populateNonMatchingFields: true);

        foreach (var query in queries)
        {
            var result = await _dataverseAdapter.FindTeachersAsync(query);
            AssertContainsMatch(result);
        }
    }

    [Fact]
    public async Task Given_3_matching_identifiers_and_one_mismatching_identifier_returns_match()
    {
        var queries = CreateQueries(matchingFieldsCount: 3, populateNonMatchingFields: true);

        foreach (var query in queries)
        {
            var result = await _dataverseAdapter.FindTeachersAsync(query);
            AssertContainsMatch(result);
        }
    }

    private void AssertDoesNotContainMatch(Contact[] results) =>
        Assert.DoesNotContain(_findTeachersFixture.MatchingTeacherId, results.Select(r => r.Id));

    private void AssertContainsMatch(Contact[] results) =>
        Assert.Contains(_findTeachersFixture.MatchingTeacherId, results.Select(r => r.Id));

    /// <summary>
    /// Returns a collection of <see cref="FindTeachersQuery"/>s with the specified number of properties matching our reference record.
    /// </summary>
    /// <param name="matchingFieldsCount">The number of fields that should match with the reference record.</param>
    /// <param name="populateNonMatchingFields">Whether non-matching fields should be filled with non-matching values.</param>
    private IEnumerable<FindTeachersQuery> CreateQueries(int matchingFieldsCount, bool populateNonMatchingFields = true)
    {
        var fields = new (Action<FindTeachersQuery> SetMatchingValue, Action<FindTeachersQuery> SetNonMatchingValue)[]
        {
            (
                (FindTeachersQuery q) =>
                {
                    q.FirstName = _findTeachersFixture.MatchingTeacherFirstName;
                    q.LastName = _findTeachersFixture.MatchingTeacherLastName;
                },
                (FindTeachersQuery q) =>
                {
                    q.FirstName = "Bad First Name";
                    q.LastName = "Bad Last Name";
                }),
            ((FindTeachersQuery q) => q.DateOfBirth = _findTeachersFixture.MatchingTeacherBirthDate, (FindTeachersQuery q) => q.DateOfBirth = _findTeachersFixture.MatchingTeacherBirthDate.AddDays(1)),
            ((FindTeachersQuery q) => q.NationalInsuranceNumber = _findTeachersFixture.MatchingTeacherNino, (FindTeachersQuery q) => q.NationalInsuranceNumber = "ABC"),
            ((FindTeachersQuery q) => q.IttProviderOrganizationIds = new[] { _findTeachersFixture.MatchingTeacherIttProviderId }, (FindTeachersQuery q) => q.IttProviderOrganizationIds = new[] { Guid.NewGuid() }),
        };

        var combinations = fields.GetCombinations(matchingFieldsCount);

        foreach (var c in combinations)
        {
            var query = new FindTeachersQuery();

            foreach (var field in c)
            {
                field.SetMatchingValue(query);
            }

            if (populateNonMatchingFields)
            {
                foreach (var field in fields.Except(c))
                {
                    field.SetNonMatchingValue(query);
                }
            }

            yield return query;
        }
    }
}

public class FindTeachersFixture : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;

    public FindTeachersFixture(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
    }

    public Guid MatchingTeacherId { get; private set; }
    public Guid MatchingTeacherIttProviderId { get; private set; }
    public string MatchingTeacherFirstName { get; private set; }
    public string MatchingTeacherLastName { get; private set; }
    public DateOnly MatchingTeacherBirthDate { get; private set; }
    public string MatchingTeacherNino { get; private set; }

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    public async Task InitializeAsync()
    {
        var testDataHelper = _dataScope.CreateTestDataHelper();

        var createPersonResult = await testDataHelper.CreatePersonAsync();
        MatchingTeacherId = createPersonResult.TeacherId;
        MatchingTeacherIttProviderId = createPersonResult.IttProviderId;
        MatchingTeacherFirstName = createPersonResult.FirstName;
        MatchingTeacherLastName = createPersonResult.LastName;
        MatchingTeacherBirthDate = createPersonResult.BirthDate;
        MatchingTeacherNino = createPersonResult.Nino;
    }
}
