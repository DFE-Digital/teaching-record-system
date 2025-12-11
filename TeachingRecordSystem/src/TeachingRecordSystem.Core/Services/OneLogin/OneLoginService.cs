using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public class OneLoginService(
    TrsDbContext dbContext,
    INotificationSender notificationSender,
    IBackgroundJobScheduler backgroundJobScheduler)
{
    public Task<string> GetRecordNotFoundEmailContentAsync(string personName)
    {
        return notificationSender.RenderEmailTemplateAsync(
            EmailTemplateIds.OneLoginCannotFindRecord,
            GetOneLoginCannotFindRecordEmailPersonalization(personName));
    }

    public async Task EnqueueRecordNotFoundEmailAsync(string emailAddress, string personName, ProcessContext processContext)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.OneLoginCannotFindRecord,
            EmailAddress = emailAddress,
            Personalization = GetOneLoginCannotFindRecordEmailPersonalization(personName).ToDictionary()
        };

        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
    }

    public async Task EnqueueRecordMatchedEmailAsync(string emailAddress, string personName, ProcessContext processContext)
    {
        var personalization = new Dictionary<string, string>
        {
            ["name"] = personName
        };

        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.OneLoginRecordMatched,
            EmailAddress = emailAddress,
            Personalization = personalization
        };

        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
    }

    public async Task SetUserVerifiedAsync(SetUserVerifiedOptions options, ProcessContext processContext)
    {
        var user = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == options.OneLoginUserSubject);

        if (user.VerifiedOn is not null)
        {
            throw new InvalidOperationException("User is already verified.");
        }

        user.SetVerified(
            processContext.Now,
            options.VerificationRoute,
            verifiedByApplicationUserId: null,
            options.VerifiedNames,
            options.VerifiedDatesOfBirth);

        await dbContext.SaveChangesAsync();

        // TODO Emit an event when we've figured out what they should look like
    }

    public async Task SetUserMatchedAsync(SetUserMatchedOptions options, ProcessContext processContext)
    {
        var user = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == options.OneLoginUserSubject);

        if (user.VerifiedOn is null)
        {
            throw new InvalidOperationException("User must be verified.");
        }

        user.SetMatched(processContext.Now, options.MatchedPersonId, options.MatchRoute, options.MatchedAttributes);

        await dbContext.SaveChangesAsync();

        // TODO Emit an event when we've figured out what they should look like
    }

    private static IReadOnlyDictionary<string, string> GetOneLoginCannotFindRecordEmailPersonalization(string personName) =>
        new Dictionary<string, string> { ["name"] = personName };

    public virtual async Task<MatchPersonResult?> MatchPersonAsync(MatchPersonOptions options)
    {
        var firstNames = options.Names.Select(parts => parts.First()).ToArray();
        var lastNames = options.Names.Where(parts => parts.Length > 1).Select(parts => parts.Last()).ToArray();

        var trn = NormalizeTrn(options.Trn);
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(options.NationalInsuranceNumber);

        var results = await dbContext.Database.SqlQueryRaw<MatchOneLoginUserQueryResult>(
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
                    new NpgsqlParameter("dobs", options.DatesOfBirth.ToArray()),
                    new NpgsqlParameter("national_insurance_number", NpgsqlDbType.Varchar) { Value = (object?)nationalInsuranceNumber ?? DBNull.Value },
                    new NpgsqlParameter("trn", NpgsqlDbType.Varchar) { Value = (object?)trn ?? DBNull.Value }
                ]
                // ReSharper disable FormatStringProblem
            ).ToArrayAsync();

        return results switch
        {
#pragma warning disable format
            [MatchOneLoginUserQueryResult r] => new MatchPersonResult(
                r.PersonId,
                r.Trn,
                new (bool Matches, string? Value, PersonMatchedAttribute Attribute)[]
                {
                    (r.TrnMatches, r.Trn, PersonMatchedAttribute.Trn),
                    (r.FirstNameMatches, r.FirstName, PersonMatchedAttribute.FirstName),
                    (r.LastNameMatches, r.LastName, PersonMatchedAttribute.LastName),
                    (r.DateOfBirthMatches, r.DateOfBirth.ToString("yyyy-MM-dd"), PersonMatchedAttribute.DateOfBirth),
                    (r.NationalInsuranceNumberMatches, r.NationalInsuranceNumber, PersonMatchedAttribute.NationalInsuranceNumber)
                }
                .Where(t => t.Matches)
                .Select(t => KeyValuePair.Create(t.Attribute, t.Value!))
                .AsReadOnly()),
#pragma warning restore format
            _ => null
        };
    }

    public async Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedPersonMatchesAsync(GetSuggestedPersonMatchesOptions options)
    {
        // Return any record that matches on last name and DOB OR NINO OR TRN.
        // Results should be ordered such that matches on TRN are returned before matches on NINO with matches on last name + DOB last.

        var lastNames = options.Names.Select(parts => parts.Last()).ToArray();
        var firstNames = options.Names.Select(parts => parts.First()).ToArray();
        var trns = new[] { options.Trn, options.TrnTokenTrnHint }.Where(trn => trn is not null).Distinct().Select(NormalizeTrn).ToArray();
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(options.NationalInsuranceNumber);

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
                    new NpgsqlParameter("dobs", options.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
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
                var score = r.MatchedAttrKeys.Sum(
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
            .ThenBy(t => t.Result.Trn)
            .Select(t => t.Result)
            .Select(r => new SuggestedMatch(
                r.PersonId,
                r.Trn,
                r.EmailAddress,
                r.FirstName,
                r.MiddleName,
                r.LastName,
                r.DateOfBirth,
                r.NationalInsuranceNumber,
                r.MatchedAttrKeys.Select(Enum.Parse<PersonMatchedAttribute>).ToArray()))
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>> GetMatchedAttributesAsync(GetSuggestedPersonMatchesOptions options, Guid personId)
    {
        var lastNames = options.Names.Select(parts => parts.Last()).ToArray();
        var firstNames = options.Names.Select(parts => parts.First()).ToArray();
        var trns = new[] { options.Trn, options.TrnTokenTrnHint }.Where(trn => trn is not null).Distinct().Select(NormalizeTrn).ToArray();
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(options.NationalInsuranceNumber);

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
                    new NpgsqlParameter("dobs", options.DatesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToArray()),
                    new NpgsqlParameter("ni_number", NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trns", trns)
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        return results
            .Select(r => KeyValuePair.Create(Enum.Parse<PersonMatchedAttribute>(r.AttributeType), r.AttributeValue))
            .OrderBy(r => (int)r.Key)
            .ThenBy(r => r.Value)
            .AsReadOnly();
    }

    private static string? NormalizeTrn(string? value) =>
        string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());

    [UsedImplicitly]
    private record MatchOneLoginUserQueryResult(
        Guid PersonId,
        string Trn,
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string? NationalInsuranceNumber,
        bool TrnMatches,
        bool FirstNameMatches,
        bool LastNameMatches,
        bool DateOfBirthMatches,
        bool NationalInsuranceNumberMatches);

    [UsedImplicitly]
    private record SuggestionsQueryResult(
        string[] MatchedAttrKeys,
        Guid PersonId,
        string Trn,
        string? EmailAddress,
        string FirstName,
        string? MiddleName,
        string LastName,
        DateOnly? DateOfBirth,
        string? NationalInsuranceNumber);

    [UsedImplicitly]
    private record MatchedAttributesQueryResult(string AttributeType, string AttributeValue);
}
