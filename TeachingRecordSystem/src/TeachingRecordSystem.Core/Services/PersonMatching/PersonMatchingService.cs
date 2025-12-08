using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Services.PersonMatching;

public class PersonMatchingService(TrsDbContext dbContext) : IPersonMatchingService
{
    public async Task<OneLoginUserMatchResult?> MatchOneLoginUserAsync(OneLoginUserMatchRequest request)
    {
        var firstNames = request.Names.Select(parts => parts.First()).ToArray();
        var lastNames = request.Names.Where(parts => parts.Length > 1).Select(parts => parts.Last()).ToArray();

        var trn = NormalizeTrn(request.Trn);
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var results = await dbContext.Database.SqlQueryRaw<OneLoginUserMatchQueryResult>(
                """
                WITH vars AS (
                    SELECT
                        (fn_split_names(:first_names, include_synonyms => true) collate "case_insensitive") first_names,
                        (fn_split_names(:last_names, include_synonyms => false) collate "case_insensitive") last_names,
                        :dobs dates_of_birth,
                        array_remove(ARRAY[:national_insurance_number] COLLATE "case_insensitive", null)::varchar[] national_insurance_numbers,
                        :trn trn
                )
                SELECT
                    p.person_id,
                    p.trn,
                    p.first_name,
                    p.last_name,
                    p.date_of_birth,
                    :national_insurance_number national_insurance_number,
                    CASE WHEN p.trn = vars.trn THEN true ELSE false END trn_matches,
                    CASE WHEN p.names && vars.first_names THEN true ELSE false END first_name_matches,
                    CASE WHEN p.names && vars.last_names THEN true ELSE false END last_name_matches,
                    CASE WHEN p.date_of_birth = ANY(vars.dates_of_birth) THEN true ELSE false END date_of_birth_matches,
                    array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers national_insurance_number_matches
                FROM persons p, vars
                WHERE p.status = 0 AND p.trn IS NOT NULL AND (
                    p.names && vars.first_names AND
                    p.names && vars.last_names AND
                    p.date_of_birth = ANY(vars.dates_of_birth) AND (
                        (array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers) OR
                        (vars.trn IS NOT NULL AND p.trn = vars.trn)
                    )
                )
                """,
                parameters:
                // ReSharper disable FormatStringProblem
                [
                    new NpgsqlParameter("first_names", firstNames),
                    new NpgsqlParameter("last_names", lastNames),
                    new NpgsqlParameter("dobs", request.DatesOfBirth.ToArray()),
                    new NpgsqlParameter("national_insurance_number", NpgsqlDbType.Varchar) { Value = (object?)nationalInsuranceNumber ?? DBNull.Value },
                    new NpgsqlParameter("trn", NpgsqlDbType.Varchar) { Value = (object?)trn ?? DBNull.Value }
                ]
                // ReSharper disable FormatStringProblem
            ).ToArrayAsync();

        return results switch
        {
#pragma warning disable format
            [OneLoginUserMatchQueryResult r] => new OneLoginUserMatchResult(
                r.person_id,
                r.trn,
                new (bool Matches, string? Value, PersonMatchedAttribute Attribute)[]
                {
                    (r.trn_matches, r.trn, PersonMatchedAttribute.Trn),
                    (r.first_name_matches, r.first_name, PersonMatchedAttribute.FirstName),
                    (r.last_name_matches, r.last_name, PersonMatchedAttribute.LastName),
                    (r.date_of_birth_matches, r.date_of_birth.ToString("yyyy-MM-dd"), PersonMatchedAttribute.DateOfBirth),
                    (r.national_insurance_number_matches, r.national_insurance_number, PersonMatchedAttribute.NationalInsuranceNumber)
                }
                .Where(t => t.Matches)
                .Select(t => KeyValuePair.Create(t.Attribute, t.Value!))
                .AsReadOnly()),
#pragma warning restore format
            _ => null
        };
    }

    public async Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedOneLoginUserMatchesAsync(GetSuggestedOneLoginUserMatchesRequest request)
    {
        return (await GetSuggestedOneLoginUserMatchesWithMatchedAttributesInfoAsync(request))
           .Select(r => new SuggestedMatch(
               r.PersonId,
               r.Trn,
               r.EmailAddress,
               r.FirstName,
               r.MiddleName,
               r.LastName,
               r.DateOfBirth,
               r.NationalInsuranceNumber))
           .ToArray();
    }

    public async Task<IReadOnlyCollection<SuggestedMatchWithMatchedAttributes>> GetSuggestedOneLoginUserMatchesWithMatchedAttributesInfoAsync(GetSuggestedOneLoginUserMatchesRequest request)
    {
        // Return any record that matches on last name and DOB OR NINO OR TRN.
        // Results should be ordered such that matches on TRN are returned before matches on NINO with matches on last name + DOB last.

        var lastNames = request.Names.Select(parts => parts.Last()).ToArray();
        var firstNames = request.Names.Select(parts => parts.First()).ToArray();
        var trns = new[] { request.Trn, request.TrnTokenTrnHint }.Where(trn => trn is not null).Distinct().Select(NormalizeTrn).ToArray();
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var results = await dbContext.Database.SqlQueryRaw<SuggestionsQueryResult>(
                """
                WITH matches AS (
                	SELECT a.person_id, array_agg(DISTINCT a.attribute_type) matched_attr_keys FROM person_search_attributes a
                	WHERE (a.attribute_type = 'LastName' AND a.attribute_value = ANY(:last_names))
                	OR (a.attribute_type = 'FirstName' AND a.attribute_value = ANY(:first_names))
                	OR (a.attribute_type = 'DateOfBirth' AND a.attribute_value = ANY(:dobs))
                	OR (a.attribute_type = 'NationalInsuranceNumber' AND a.attribute_value = :ni_number)
                	OR (a.attribute_type = 'Trn' AND a.attribute_value = ANY(:trns))
                	GROUP BY a.person_id
                )
                SELECT
                    m.matched_attr_keys,
                    p.person_id,
                    p.trn,
                    p.email_address,
                    p.first_name,
                    p.middle_name,
                    p.last_name,
                    p.date_of_birth,
                    p.national_insurance_number
                FROM matches m
                JOIN persons p ON m.person_id = p.person_id
                WHERE ((ARRAY['LastName', 'DateOfBirth']::varchar[] <@ m.matched_attr_keys
                OR ARRAY['NationalInsuranceNumber', 'Trn']::varchar[] && m.matched_attr_keys))
                AND p.status = 0
                """,
                // ReSharper disable FormatStringProblem
                parameters:
                [
                    new NpgsqlParameter("last_names", lastNames),
                    new NpgsqlParameter("first_names", firstNames),
                    new NpgsqlParameter("dobs", request.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
                    new NpgsqlParameter("ni_number", NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trns", trns)
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        return results
            .Select(r =>
            {
                var score = r.matched_attr_keys.Sum(
                    m => m switch
                    {
                        "Trn" => 20,
                        "NationalInsuranceNumber" => 10,
                        "DateOfBirth" => 2,
                        "LastName" => 2,
                        "FirstName" => 1,
                        _ => 0
                    });

                return (Result: r, Score: score);
            })
            .OrderByDescending(t => t.Score)
            .ThenBy(t => t.Result.trn)
            .Select(t => t.Result)
            //.Select(r => Enum.Parse<PersonMatchedAttribute>(r.matched_attr_keys))
            .Select(r => new SuggestedMatchWithMatchedAttributes(
                r.person_id,
                r.trn,
                r.email_address,
                r.first_name,
                r.middle_name,
                r.last_name,
                r.date_of_birth,
                r.national_insurance_number,
                r.matched_attr_keys.Select(m => Enum.Parse<PersonMatchedAttribute>(m)).ToArray()))
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>> GetMatchedAttributesAsync(GetSuggestedOneLoginUserMatchesRequest request, Guid personId)
    {
        var lastNames = request.Names.Select(parts => parts.Last()).ToArray();
        var firstNames = request.Names.Select(parts => parts.First()).ToArray();
        var trns = new[] { request.Trn, request.TrnTokenTrnHint }.Where(trn => trn is not null).Distinct().Select(NormalizeTrn).ToArray();
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var results = await dbContext.Database.SqlQueryRaw<MatchedAttributesQueryResult>(
                """
                SELECT
                    a.attribute_type,
                    a.attribute_value
                FROM person_search_attributes a
                WHERE ((a.attribute_type = 'LastName' AND a.attribute_value = ANY(:last_names))
                OR (a.attribute_type = 'FirstName' AND a.attribute_value = ANY(:first_names))
                OR (a.attribute_type = 'DateOfBirth' AND a.attribute_value = ANY(:dobs))
                OR (a.attribute_type = 'NationalInsuranceNumber' AND a.attribute_value = :ni_number)
                OR (a.attribute_type = 'Trn' AND a.attribute_value = ANY(:trns)))
                AND a.person_id = :person_id
                """,
                parameters:
                // ReSharper disable FormatStringProblem
                [
                    new NpgsqlParameter("person_id", personId),
                    new NpgsqlParameter("last_names", lastNames),
                    new NpgsqlParameter("first_names", firstNames),
                    new NpgsqlParameter("dobs", request.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
                    new NpgsqlParameter("ni_number", NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trns", trns)
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        return results
            .Select(r => KeyValuePair.Create(Enum.Parse<PersonMatchedAttribute>(r.attribute_type), r.attribute_value))
            .OrderBy(r => (int)r.Key)
            .ThenBy(r => r.Value)
            .AsReadOnly();
    }

    private static string? NormalizeTrn(string? value) =>
        string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());

#pragma warning disable IDE1006 // Naming Styles
    private record OneLoginUserMatchQueryResult(
        Guid person_id,
        string trn,
        string first_name,
        string last_name,
        DateOnly date_of_birth,
        string? national_insurance_number,
        bool trn_matches,
        bool first_name_matches,
        bool last_name_matches,
        bool date_of_birth_matches,
        bool national_insurance_number_matches);

    private record MatchedAttribute(string attribute_type, string attribute_value);

    private record SuggestionsQueryResult(
        string[] matched_attr_keys,
        Guid person_id,
        string trn,
        string? email_address,
        string first_name,
        string? middle_name,
        string last_name,
        DateOnly? date_of_birth,
        string? national_insurance_number);

    private record MatchedAttributesQueryResult(string attribute_type, string attribute_value);
#pragma warning restore IDE1006 // Naming Styles
}
