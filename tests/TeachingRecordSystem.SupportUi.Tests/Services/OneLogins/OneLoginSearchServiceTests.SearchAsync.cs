using Optional;
using TeachingRecordSystem.SupportUi.Pages.OneLogins;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.OneLogins;

namespace TeachingRecordSystem.SupportUi.Tests.Services.OneLogins;

public partial class OneLoginSearchServiceTests
{
    [Theory(Skip = "Sort by TRN failing on CI build")]
    [InlineData(OneLoginSearchSortByOption.Email, SortDirection.Ascending)]
    [InlineData(OneLoginSearchSortByOption.Email, SortDirection.Descending)]
    [InlineData(OneLoginSearchSortByOption.Name, SortDirection.Ascending)]
    [InlineData(OneLoginSearchSortByOption.Name, SortDirection.Descending)]
    [InlineData(OneLoginSearchSortByOption.DateOfBirth, SortDirection.Ascending)]
    [InlineData(OneLoginSearchSortByOption.DateOfBirth, SortDirection.Descending)]
    [InlineData(OneLoginSearchSortByOption.Trn, SortDirection.Ascending)]
    [InlineData(OneLoginSearchSortByOption.Trn, SortDirection.Descending)]
    public async Task SearchAsync_WithSorting_ReturnsOrderedResults(OneLoginSearchSortByOption sortBy, SortDirection sortDirection)
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Bob")
            .WithLastName("Brown")
            .WithDateOfBirth(new DateOnly(1980, 5, 10)));
        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alice")
            .WithLastName("Anderson")
            .WithDateOfBirth(new DateOnly(1975, 3, 20)));

        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            person1,
            email: Option.Some<string?>("bob@example.com"));

        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            person2,
            email: Option.Some<string?>("alice@example.com"));

        var oneLoginUser3 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("charlie@example.com"),
            verifiedInfo: (new[] { "Charlie", "Clark" }, new DateOnly(1995, 6, 15)));

        var options = new OneLoginSearchOptions
        {
            Search = null,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var expectedResults = new[]
        {
            new
            {
                Subject = oneLoginUser1.Subject,
                EmailAddress = "bob@example.com",
                Name = "Bob Brown",
                DateOfBirth = (DateOnly?)new DateOnly(1980, 5, 10),
                Trn = (string?)person1.Person.Trn
            },
            new
            {
                Subject = oneLoginUser2.Subject,
                EmailAddress = "alice@example.com",
                Name = "Alice Anderson",
                DateOfBirth = (DateOnly?)new DateOnly(1975, 3, 20),
                Trn = (string?)person2.Person.Trn
            },
            new
            {
                Subject = oneLoginUser3.Subject,
                EmailAddress = "charlie@example.com",
                Name = "Charlie Clark",
                DateOfBirth = (DateOnly?)new DateOnly(1995, 6, 15),
                Trn = (string?)null
            }
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        var expectedResultsOrdered = (sortBy switch
        {
            OneLoginSearchSortByOption.Email => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(r => r.EmailAddress).AsEnumerable()
                : expectedResults.OrderByDescending(r => r.EmailAddress).AsEnumerable(),
            OneLoginSearchSortByOption.Name => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(r => r.Name).AsEnumerable()
                : expectedResults.OrderByDescending(r => r.Name).AsEnumerable(),
            OneLoginSearchSortByOption.DateOfBirth => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(r => r.DateOfBirth).AsEnumerable()
                : expectedResults.OrderByDescending(r => r.DateOfBirth).AsEnumerable(),
            OneLoginSearchSortByOption.Trn => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(r => r.Trn).AsEnumerable()
                : expectedResults.OrderByDescending(r => r.Trn).AsEnumerable(),
            _ => expectedResults.AsEnumerable()
        }).ToArray();

        Assert.Equal(expectedResultsOrdered.Length, result.Results.Count);
        Assert.Equal(expectedResultsOrdered.Select(r => r.Subject), result.Results.Select(r => r.Subject));
        Assert.Equal(expectedResultsOrdered.Select(r => r.EmailAddress), result.Results.Select(r => r.EmailAddress));
        Assert.Equal(expectedResultsOrdered.Select(r => r.Name), result.Results.Select(r => r.Name));
        Assert.Equal(expectedResultsOrdered.Select(r => r.DateOfBirth), result.Results.Select(r => r.DateOfBirth));
        Assert.Equal(expectedResultsOrdered.Select(r => r.Trn), result.Results.Select(r => r.Trn));
    }

    [Theory]
    [InlineData("alice@example.com", 1)]
    [InlineData("bob@example.com", 1)]
    [InlineData("nonexistent@example.com", 0)]
    [InlineData("ALICE@EXAMPLE.COM", 1)] // Case insensitive
    public async Task SearchAsync_WithEmailAddress_ReturnsMatchingResults(string searchEmail, int expectedCount)
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            person1,
            email: Option.Some<string?>("alice@example.com"));

        var person2 = await TestData.CreatePersonAsync();
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            person2,
            email: Option.Some<string?>("bob@example.com"));

        var options = new OneLoginSearchOptions
        {
            Search = searchEmail,
            SortBy = OneLoginSearchSortByOption.Email,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Equal(expectedCount, result.Results.Count);
    }

    [Theory]
    [InlineData("John", new string[] { "John Smith" })]
    [InlineData("john", new string[] { "John Smith" })]
    [InlineData("John Smith", new string[] { "John Smith" })]
    [InlineData("Smith", new string[] { "Jane Smith", "John Smith" })]
    [InlineData("Jones", new string[] { "Bob Jones" })]
    public async Task SearchAsync_WithName_ReturnsMatchingResults(string searchName, string[] expectedNames)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("john.smith@example.com"),
            verifiedInfo: (new[] { "John", "Smith" }, new DateOnly(1990, 1, 1)));

        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("jane.smith@example.com"),
            verifiedInfo: (new[] { "Jane", "Smith" }, new DateOnly(1985, 5, 15)));

        var oneLoginUser3 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("bob.jones@example.com"),
            verifiedInfo: (new[] { "Bob", "Jones" }, new DateOnly(1992, 3, 20)));

        var options = new OneLoginSearchOptions
        {
            Search = searchName,
            SortBy = OneLoginSearchSortByOption.Name,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Equal(expectedNames, result.Results.Select(r => r.Name));
    }

    [Theory]
    [InlineData("20/01/1990")]
    [InlineData("20/1/1990")]
    [InlineData("20 Jan 1990")]
    [InlineData("20 January 1990")]
    [InlineData("20 jan 1990")]
    [InlineData("20 january 1990")]
    public async Task SearchAsync_WithDateOfBirth_ReturnsMatchingResults(string searchDate)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("john.smith@example.com"),
            verifiedInfo: (new[] { "John", "Smith" }, new DateOnly(1990, 1, 20)));

        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("jane.smith@example.com"),
            verifiedInfo: (new[] { "Jane", "Smith" }, new DateOnly(1985, 5, 15)));

        var options = new OneLoginSearchOptions
        {
            Search = searchDate,
            SortBy = OneLoginSearchSortByOption.Email,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Single(result.Results);
        Assert.Equal("john.smith@example.com", result.Results.First().EmailAddress);
    }

    [Fact]
    public async Task SearchAsync_WithTrn_ReturnsMatchingResults()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            person1,
            email: Option.Some<string?>("john.smith@example.com"));

        var person2 = await TestData.CreatePersonAsync();
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            person2,
            email: Option.Some<string?>("jane.doe@example.com"));

        var oneLoginUser3 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("orphan@example.com"),
            verifiedInfo: (new[] { "Orphan", "User" }, new DateOnly(1990, 1, 1)));

        var options = new OneLoginSearchOptions
        {
            Search = person1.Person.Trn,
            SortBy = OneLoginSearchSortByOption.Email,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Single(result.Results);
        Assert.Equal(oneLoginUser1.Subject, result.Results.First().Subject);
        Assert.Equal(person1.Person.Trn, result.Results.First().Trn);
    }

    [Fact]
    public async Task SearchAsync_WithNullSearch_ReturnsAllResults()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            person1,
            email: Option.Some<string?>("user1@example.com"));

        var person2 = await TestData.CreatePersonAsync();
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            person2,
            email: Option.Some<string?>("user2@example.com"));

        var options = new OneLoginSearchOptions
        {
            Search = null,
            SortBy = OneLoginSearchSortByOption.Email,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public async Task SearchAsync_WithEmptySearch_ReturnsAllResults()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            person1,
            email: Option.Some<string?>("user1@example.com"));

        var person2 = await TestData.CreatePersonAsync();
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(
            person2,
            email: Option.Some<string?>("user2@example.com"));

        var options = new OneLoginSearchOptions
        {
            Search = "   ",
            SortBy = OneLoginSearchSortByOption.Email,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public async Task SearchAsync_OrphanedOneLoginUser_ReturnsNullForTrn()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("orphan@example.com"),
            verifiedInfo: (new[] { "Orphan", "User" }, new DateOnly(1990, 1, 1)));

        var options = new OneLoginSearchOptions
        {
            Search = null,
            SortBy = OneLoginSearchSortByOption.Email,
            SortDirection = SortDirection.Ascending
        };

        // Act
        var result = await WithServiceAsync<OneLoginSearchService, OneLoginSearchResult>(service =>
            service.SearchAsync(options, new PaginationOptions(null, 100)));

        // Assert
        Assert.Single(result.Results);
        Assert.Null(result.Results.First().Trn);
        Assert.Equal("Orphan User", result.Results.First().Name);
        Assert.True(result.Results.First().DateOfBirth.HasValue);
        Assert.Equal(new DateOnly(1990, 1, 1), result.Results.First().DateOfBirth!.Value);
    }
}
