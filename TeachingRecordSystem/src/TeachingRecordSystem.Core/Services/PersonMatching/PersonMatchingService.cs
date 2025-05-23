using System.Text.Json;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.PersonMatching;

public class PersonMatchingService(TrsDbContext dbContext) : IPersonMatchingService
{
    public async Task<OneLoginUserMatchResult?> MatchOneLoginUserAsync(OneLoginUserMatchRequest request)
    {
        var fullNames = request.Names.Where(parts => parts.Length > 1).Select(parts => $"{parts.First()} {parts.Last()}").ToArray();
        if (fullNames.Length == 0 || !request.DatesOfBirth.Any() || (request.NationalInsuranceNumber is null && request.Trn is null))
        {
            return null;
        }

        var trn = NormalizeTrn(request.Trn);
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var results = await dbContext.Database.SqlQueryRaw<OneLoginUserMatchQueryResult>(
                """
                WITH matches AS (
                	SELECT
                        a.person_id,
                        array_agg(a.attribute_type) matched_attr_keys,
                        json_agg(json_build_object('attribute_type', a.attribute_type, 'attribute_value', a.attribute_value)) matched_attrs
                    FROM person_search_attributes a
                	WHERE (a.attribute_type = 'FullName' AND a.attribute_value = ANY(:full_names))
                	OR (a.attribute_type = 'DateOfBirth' AND a.attribute_value = ANY(:dobs))
                	OR (a.attribute_type = 'NationalInsuranceNumber' AND a.attribute_value = :ni_number)
                	OR (a.attribute_type = 'Trn' AND a.attribute_value = :trn)
                	GROUP BY a.person_id
                )
                SELECT p.person_id, p.trn, m.matched_attrs
                FROM matches m
                JOIN persons p ON m.person_id = p.person_id
                WHERE ARRAY['FullName', 'DateOfBirth']::varchar[] <@ m.matched_attr_keys
                AND ARRAY['NationalInsuranceNumber', 'Trn']::varchar[] && m.matched_attr_keys
                AND p.dqt_state = 0
                """,
                parameters:
                [
                    new NpgsqlParameter("full_names", fullNames),
                    new NpgsqlParameter("dobs", request.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
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

        return results switch
        {
            [OneLoginUserMatchQueryResult r] => new OneLoginUserMatchResult(r.person_id, r.trn, MapMatchedAttrs(r.matched_attrs)),
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
                AND p.dqt_state = 0
                """,
                parameters:
                [
                    new NpgsqlParameter("last_names", lastNames),
                    new NpgsqlParameter("first_names", firstNames),
                    new NpgsqlParameter("dobs", request.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
                    new NpgsqlParameter("ni_number", NpgsqlTypes.NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trns", trns)
                ]
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
        var fullNames = request.Names.Where(parts => parts.Length > 1).Select(parts => $"{parts.First()} {parts.Last()}").ToArray();
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
                WHERE ((a.attribute_type = 'FullName' AND a.attribute_value = ANY(:full_names))
                OR (a.attribute_type = 'LastName' AND a.attribute_value = ANY(:last_names))
                OR (a.attribute_type = 'FirstName' AND a.attribute_value = ANY(:first_names))
                OR (a.attribute_type = 'DateOfBirth' AND a.attribute_value = ANY(:dobs))
                OR (a.attribute_type = 'NationalInsuranceNumber' AND a.attribute_value = :ni_number)
                OR (a.attribute_type = 'Trn' AND a.attribute_value = ANY(:trns)))
                AND a.person_id = :person_id
                """,
                parameters:
                [
                    new NpgsqlParameter("person_id", personId),
                    new NpgsqlParameter("full_names", fullNames),
                    new NpgsqlParameter("last_names", lastNames),
                    new NpgsqlParameter("first_names", firstNames),
                    new NpgsqlParameter("dobs", request.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
                    new NpgsqlParameter("ni_number", NpgsqlTypes.NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trns", trns)
                ]
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

        return results switch
        {
            [var singleMatch]
                when singleMatch.matched_attr_keys
                    .Select(Enum.Parse<PersonMatchedAttribute>)
                    .ToHashSet()
                    .IsSupersetOf([PersonMatchedAttribute.DateOfBirth, PersonMatchedAttribute.NationalInsuranceNumber]) =>
                TrnRequestMatchResult.DefiniteMatch(singleMatch.person_id, singleMatch.trn),
            [] => TrnRequestMatchResult.NoMatches(),
            _ => TrnRequestMatchResult.PotentialMatches()
        };
    }

    public async Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedMatchesFromTrnRequestAsync(TrnRequestMetadata request)
    {
        var results = await GetMatchesFromTrnRequestAsync(request);

        return results
            .Select(r =>
            {
                var score = r.matched_attr_keys.Sum(
                    m => m switch
                    {
                        "NationalInsuranceNumber" => 10,
                        _ => 1
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

    private Task<SuggestionsQueryResult[]> GetMatchesFromTrnRequestAsync(TrnRequestMetadata request)
    {
        // Find all Active records with a TRN that match on:
        // - at least three of first name, middle name, last name, DOB, email address, NINO *OR*
        // - both DOB & NINO.

        var firstNames = new[] { request.FirstName, request.PreviousFirstName }.ExceptEmpty();
        var lastNames = new[] { request.LastName, request.PreviousLastName }.ExceptEmpty();
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        return dbContext.Database.SqlQueryRaw<SuggestionsQueryResult>(
                """
                WITH matches AS (
                    SELECT
                        a.person_id,
                        array_agg(DISTINCT a.attribute_type) matched_attr_keys,
                        json_agg(json_build_object('attribute_type', a.attribute_type, 'attribute_value', a.attribute_value)) matched_attrs
                    FROM person_search_attributes a
                    WHERE (a.attribute_type = 'FirstName' AND a.attribute_value = ANY(:first_names))
                    OR (a.attribute_type = 'MiddleName' AND a.attribute_value = ANY(:middle_names))
                    OR (a.attribute_type = 'LastName' AND a.attribute_value = ANY(:last_names))
                    OR (a.attribute_type = 'DateOfBirth' AND a.attribute_value = ANY(:dates_of_birth))
                    OR (a.attribute_type = 'NationalInsuranceNumber' AND a.attribute_value = ANY(:ni_numbers))
                    OR (a.attribute_type = 'EmailAddress' AND a.attribute_value = ANY(:email_addresses))
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
                WHERE (
                    ARRAY['NationalInsuranceNumber', 'DateOfBirth']::varchar[] <@ m.matched_attr_keys
                    OR array_length(m.matched_attr_keys, 1) >= 3)
                AND p.dqt_state = 0 AND p.trn IS NOT NULL
                """,
                parameters:
                [
                    CreateArrayParameter("first_names", firstNames),
                    CreateArrayParameter("middle_names", request.MiddleName.ToSingleItemCollectionIfNotEmpty()),
                    CreateArrayParameter("last_names", lastNames),
                    CreateArrayParameter("dates_of_birth", request.DateOfBirth.ToSingleItemCollectionIfNotEmpty().Select(d => d.ToString("yyyy-MM-dd"))),
                    CreateArrayParameter("ni_numbers", nationalInsuranceNumber.ToSingleItemCollectionIfNotEmpty()),
                    CreateArrayParameter("email_addresses", request.EmailAddress.ToSingleItemCollectionIfNotEmpty())
                ]
            ).ToArrayAsync();

        static NpgsqlParameter CreateArrayParameter(string name, IEnumerable<string> values) =>
            new(name, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Varchar) { Value = values.ToArray() };
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
    private record OneLoginUserMatchQueryResult(Guid person_id, string trn, JsonDocument matched_attrs);

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
        string[] matched_attr_keys);
#pragma warning restore IDE1006 // Naming Styles
}

file static class Extensions
{
    public static IEnumerable<T> ToSingleItemCollectionIfNotEmpty<T>(this T? source) where T : notnull =>
        new[] { source! }.ExceptEmpty();

    public static IEnumerable<T> ExceptEmpty<T>(this IEnumerable<T?> source) =>
        (IEnumerable<T>)source.Where(e => e is not null && (e is not string str || !string.IsNullOrWhiteSpace(str)));
}
