using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.AuthorizeAccess.Services.PersonMatching;

public class PersonMatchingService(TrsDbContext dbContext) : IPersonMatchingService
{
    public async Task<(Guid PersonId, string Trn)?> Match(
        IEnumerable<string[]> names,
        IEnumerable<DateOnly> datesOfBirth,
        string? nationalInsuranceNumber,
        string? trn)
    {
        var fullNames = names.Where(parts => parts.Length > 1).Select(parts => $"{parts.First()} {parts.Last()}").ToArray();
        if (fullNames.Length == 0 || !datesOfBirth.Any() || (nationalInsuranceNumber is null && trn is null))
        {
            return null;
        }

        var results = await dbContext.Database.SqlQueryRaw<Result>(
                """
                WITH matches AS (
                	SELECT a.person_id, ARRAY_AGG(a.attribute_type) matched_attrs FROM person_search_attributes a
                	WHERE (a.attribute_type = 'FullName' AND a.attribute_value = ANY(:full_names))
                	OR (a.attribute_type = 'DateOfBirth' AND a.attribute_value = ANY(:dobs))
                	OR (a.attribute_type = 'NationalInsuranceNumber' AND a.attribute_value = :ni_number)
                	OR (a.attribute_type = 'Trn' AND a.attribute_value = :trn)
                	GROUP BY a.person_id
                )
                SELECT p.person_id, p.trn from matches m
                JOIN persons p ON m.person_id = p.person_id
                WHERE ARRAY['FullName', 'DateOfBirth']::varchar[] <@ m.matched_attrs
                AND ARRAY['NationalInsuranceNumber', 'Trn']::varchar[] && m.matched_attrs
                AND p.dqt_state = 0
                """,
                parameters:
                [
                    new NpgsqlParameter("full_names", fullNames),
                    new NpgsqlParameter("dobs", datesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
                    new NpgsqlParameter("ni_number", NpgsqlTypes.NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trn", NpgsqlTypes.NpgsqlDbType.Varchar)
                    {
                        Value = trn is not null ? trn : DBNull.Value
                    }
                ]
            ).ToArrayAsync();

        if (results.Length == 1)
        {
            return (results[0].person_id, results[0].trn);
        }
        else
        {
            return null;
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    private record Result(Guid person_id, string trn);
#pragma warning restore IDE1006 // Naming Styles
}
