using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.OneLogins;

namespace TeachingRecordSystem.SupportUi.Services.OneLogins;

public class OneLoginSearchService(TrsDbContext dbContext)
{
    public async Task<OneLoginSearchResult> SearchAsync(OneLoginSearchOptions options)
    {
        var search = options.Search?.Trim() ?? string.Empty;
        var sortBy = options.SortBy ?? OneLoginSearchSortByOption.Email;
        var sortDirection = options.SortDirection ?? SortDirection.Ascending;
        var sortAscending = sortDirection == SortDirection.Ascending;

        var orderByClause = sortBy switch
        {
            OneLoginSearchSortByOption.Name => $"verified_names->0 {(sortAscending ? "ASC NULLS FIRST" : "DESC NULLS LAST")}, subject {(sortAscending ? "ASC" : "DESC")}",
            OneLoginSearchSortByOption.DateOfBirth => $"(verified_dates_of_birth->0)::text {(sortAscending ? "ASC NULLS FIRST" : "DESC NULLS LAST")}, subject {(sortAscending ? "ASC" : "DESC")}",
            OneLoginSearchSortByOption.Trn => $"COALESCE((SELECT p.trn FROM persons p WHERE p.person_id = one_login_users.person_id), '0000000') {(sortAscending ? "ASC" : "DESC")}, subject {(sortAscending ? "ASC" : "DESC")}",
            OneLoginSearchSortByOption.Email or _ => $"email_address {(sortAscending ? "ASC NULLS FIRST" : "DESC NULLS LAST")}, subject {(sortAscending ? "ASC" : "DESC")}"
        };

        IQueryable<OneLoginUser> query;

#pragma warning disable EF1002, EF1003 // Risk of SQL injection - orderByClause is constructed from enum values only, not user input
        if (SearchTextHelper.IsEmailAddress(search, out _))
        {
            query = dbContext.OneLoginUsers
                .FromSqlRaw(
                    "SELECT * FROM one_login_users WHERE email_address = {0} ORDER BY " + orderByClause,
                    search);
        }
        else if (SearchTextHelper.IsDate(search, out var dateOfBirth))
        {
            query = dbContext.OneLoginUsers
                .FromSqlRaw(
                    "SELECT * FROM one_login_users WHERE ARRAY(SELECT jsonb_array_elements_text(verified_dates_of_birth))::date[] @> ARRAY[{0}] ORDER BY " + orderByClause,
                    dateOfBirth);
        }
        else if (SearchTextHelper.IsTrn(search))
        {
            query = dbContext.OneLoginUsers
                .FromSqlRaw(
                    "SELECT o.* FROM one_login_users o INNER JOIN persons p ON o.person_id = p.person_id WHERE p.trn = {0} ORDER BY " + orderByClause,
                    search);
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            query = dbContext.OneLoginUsers
                .FromSqlRaw(
                    @"SELECT * FROM one_login_users
                    WHERE verified_names IS NOT NULL
                    AND EXISTS (
                        SELECT 1 
                        FROM jsonb_array_elements(verified_names) AS name_array
                        WHERE fn_split_names(ARRAY[{0}]::varchar[]) COLLATE ""case_insensitive"" <@ 
                              ARRAY(SELECT jsonb_array_elements_text(name_array))::varchar[] COLLATE ""case_insensitive""
                    )
                    ORDER BY " + orderByClause,
                    search);
        }
        else
        {
            query = dbContext.OneLoginUsers
                .FromSqlRaw("SELECT * FROM one_login_users ORDER BY " + orderByClause);
        }
#pragma warning restore EF1002, EF1003

        query = query.Include(o => o.Person);

        var results = await query
            .Select(o => new OneLoginSearchResultItem(
                o.Subject,
                o.EmailAddress ?? string.Empty,
                o.VerifiedNames,
                o.VerifiedDatesOfBirth,
                o.Person != null ? o.Person.Trn : null))
            .ToArrayAsync();

        return new OneLoginSearchResult
        {
            Results = results
        };
    }
}
