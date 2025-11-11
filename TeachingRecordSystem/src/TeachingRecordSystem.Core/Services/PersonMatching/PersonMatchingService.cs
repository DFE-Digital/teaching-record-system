using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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
            .Select(r => new SuggestedMatch(
                r.person_id,
                r.trn,
                r.email_address,
                r.first_name,
                r.middle_name,
                r.last_name,
                r.date_of_birth,
                r.national_insurance_number))
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

    public async Task<TrnRequestMatchResult> MatchFromTrnRequestAsync(TrnRequestMetadata request)
    {
        var results = await GetMatchesFromTrnRequestAsync(request);

        return (request, results) switch
        {
#pragma warning disable format
            ({NationalInsuranceNumber: null}, [{ first_name_matches: true, last_name_matches: true, date_of_birth_matches: true, email_address_matches: true, gender_matches: true } singleMatch]) =>
                TrnRequestMatchResult.DefiniteMatch(singleMatch.person_id, singleMatch.trn),
            (_, [{ date_of_birth_matches: true, national_insurance_number_matches: true } singleMatch]) =>
                TrnRequestMatchResult.DefiniteMatch(singleMatch.person_id, singleMatch.trn),
#pragma warning restore format
            (_, []) => TrnRequestMatchResult.NoMatches(),
            _ => TrnRequestMatchResult.PotentialMatches(results.Select(r => r.person_id))
        };
    }

    public async Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedMatchesFromTrnRequestAsync(TrnRequestMetadata request)
    {
        var results = await GetMatchesFromTrnRequestAsync(request);

        return results
            .Select(r =>
            {
                var score = (r.date_of_birth_matches ? 1 : 0) +
                    (r.first_name_matches ? 1 : 0) +
                    (r.middle_name_matches ? 1 : 0) +
                    (r.last_name_matches ? 1 : 0) +
                    (r.email_address_matches ? 5 : 0) +
                    (r.national_insurance_number_matches ? 10 : 0);

                return (Result: r, Score: score);
            })
            .OrderByDescending(t => t.Score)
            .ThenBy(t => t.Result.trn)
            .Select(t => t.Result)
            .Select(r => new SuggestedMatch(
                r.person_id,
                r.trn,
                r.email_address,
                r.first_name,
                r.middle_name,
                r.last_name,
                r.date_of_birth,
                r.national_insurance_number))
            .AsReadOnly();
    }

    private async Task<TrnRequestMatchQueryResult[]> GetMatchesFromTrnRequestAsync(TrnRequestMetadata request)
    {
        // Find all Active records with a TRN that match on:
        // - at least three of first name, middle name, last name, DOB *OR*
        // - NINO *OR*
        // - email address.

        var firstNames = new[] { request.FirstName, request.PreviousFirstName };
        var middleNames = new[] { request.MiddleName, request.PreviousMiddleName };
        var lastNames = new[] { request.LastName, request.PreviousLastName };

        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        return await dbContext.Database.SqlQueryRaw<TrnRequestMatchQueryResult>(
                """
                WITH vars AS (
                    SELECT
                        (fn_split_names(:first_names, include_synonyms => true) collate "case_insensitive") first_names,
                        (fn_split_names(:middle_names, include_synonyms => true) collate "case_insensitive") middle_names,
                        (fn_split_names(:last_names, include_synonyms => false) collate "case_insensitive") last_names,
                        :date_of_birth date_of_birth,
                        (:email_address COLLATE "case_insensitive") email_address,
                        array_remove(ARRAY[:national_insurance_number] COLLATE "case_insensitive", null)::varchar[] national_insurance_numbers,
                        :gender gender
                )
                SELECT
                    p.person_id,
                    p.trn,
                    p.first_name,
                    p.middle_name,
                    p.last_name,
                    p.date_of_birth,
                    p.email_address,
                    p.national_insurance_number,
                    p.gender,
                    CASE WHEN p.names && vars.first_names THEN true ELSE false END first_name_matches,
                    CASE WHEN p.names && vars.middle_names THEN true ELSE false END middle_name_matches,
                    CASE WHEN p.names && vars.last_names THEN true ELSE false END last_name_matches,
                    CASE WHEN p.date_of_birth = vars.date_of_birth THEN true ELSE false END date_of_birth_matches,
                    CASE WHEN vars.email_address IS NOT NULL AND p.email_address = vars.email_address THEN true ELSE false END email_address_matches,
                    array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers national_insurance_number_matches,
                    CASE WHEN vars.gender IS NOT NULL AND p.gender = vars.gender THEN true ELSE false END gender_matches
                FROM persons p, vars
                WHERE
                    p.status = 0 and p.trn IS NOT NULL AND (
                        (p.names && vars.first_names AND p.names && vars.middle_names AND p.names && vars.last_names) OR
                        (p.names && vars.first_names AND p.names && vars.middle_names AND p.date_of_birth = vars.date_of_birth) OR
                        (p.names && vars.middle_names AND p.names && vars.last_names AND p.date_of_birth = vars.date_of_birth) OR
                        (p.names && vars.first_names AND p.names && vars.last_names AND p.date_of_birth = vars.date_of_birth) OR
                        (vars.email_address IS NOT NULL AND p.email_address = vars.email_address) OR
                        (array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers)
                    )
                """,
                parameters:
                // ReSharper disable FormatStringProblem
                [
                    CreateArrayParameter("first_names", firstNames),
                    CreateArrayParameter("middle_names", middleNames),
                    CreateArrayParameter("last_names", lastNames),
                    new NpgsqlParameter("date_of_birth", NpgsqlDbType.Date) { Value = (object?)request.DateOfBirth ?? DBNull.Value },
                    new NpgsqlParameter("national_insurance_number", NpgsqlDbType.Varchar) { Value = (object?)nationalInsuranceNumber ?? DBNull.Value },
                    new NpgsqlParameter("email_address", NpgsqlDbType.Varchar) { Value = (object?)request.EmailAddress ?? DBNull.Value },
                    new NpgsqlParameter("gender", NpgsqlDbType.Integer) { Value = (object?)(int?)request.Gender ?? DBNull.Value }
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        static NpgsqlParameter CreateArrayParameter(string name, IEnumerable<string?> values) =>
            new(name, NpgsqlDbType.Array | NpgsqlDbType.Varchar) { Value = values.ToArray() };
    }

    private static IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>> MapMatchedAttrs(JsonDocument doc) =>
        doc.Deserialize<MatchedAttribute[]>()!
            .Select(a => new KeyValuePair<PersonMatchedAttribute, string>(
                Enum.Parse<PersonMatchedAttribute>(a.attribute_type),
                a.attribute_value))
            .AsReadOnly();

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

    private record TrnRequestMatchQueryResult(
        Guid person_id,
        string trn,
        string first_name,
        string? middle_name,
        string last_name,
        DateOnly? date_of_birth,
        string? email_address,
        string? national_insurance_number,
        Gender? gender,
        bool first_name_matches,
        bool middle_name_matches,
        bool last_name_matches,
        bool date_of_birth_matches,
        bool email_address_matches,
        bool national_insurance_number_matches,
        bool gender_matches);
#pragma warning restore IDE1006 // Naming Styles
}

file static class Extensions
{
    public static IEnumerable<T> ToSingleItemCollectionIfNotEmpty<T>(this T? source) where T : notnull =>
        new[] { source! }.ExceptEmpty();

    public static IEnumerable<T> ExceptEmpty<T>(this IEnumerable<T?> source) =>
        (IEnumerable<T>)source.Where(e => e is not null && (e is not string str || !string.IsNullOrWhiteSpace(str)));
}
