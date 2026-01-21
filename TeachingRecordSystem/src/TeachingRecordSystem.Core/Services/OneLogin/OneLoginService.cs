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
        // A One Login is matched if there is exactly one Person with a matching
        // first name, last name, DOB AND either NINO or TRN.

        var suggestedMatches = await GetSuggestedPersonMatchesAsync(
            new GetSuggestedPersonMatchesOptions(
                options.Names,
                options.DatesOfBirth,
                options.NationalInsuranceNumber,
                options.Trn,
                options.TrnTokenTrnHint));

        if (suggestedMatches.Count == 1)
        {
            var singleMatch = suggestedMatches.Single();
            var matchedAttributes = singleMatch.MatchedAttributes;
            var matchedAttributeTypes = matchedAttributes.Select(kvp => kvp.Key).ToArray();

            var requiredAttributeTypes = new[]
            {
                PersonMatchedAttribute.FirstName,
                PersonMatchedAttribute.LastName,
                PersonMatchedAttribute.DateOfBirth
            };

            if (!requiredAttributeTypes.Except(matchedAttributeTypes).Any() &&
                (matchedAttributeTypes.Contains(PersonMatchedAttribute.NationalInsuranceNumber) ||
                 matchedAttributeTypes.Contains(PersonMatchedAttribute.Trn)))
            {
                return new MatchPersonResult(
                    singleMatch.PersonId,
                    singleMatch.Trn,
                    singleMatch.MatchedAttributes);
            }
        }

        return null;
    }

    public async Task<IReadOnlyCollection<MatchPersonResult>> GetSuggestedPersonMatchesAsync(GetSuggestedPersonMatchesOptions options)
    {
        // Return any record that matches on last name and DOB OR NINO OR TRN.
        // Results should be ordered such that matches on TRN are returned before matches on NINO with matches on last name + DOB last.

        var firstNames = options.Names.Select(parts => parts.First()).ToArray();
        var lastNames = options.Names.Select(parts => parts.Skip(1).LastOrDefault()).Where(n => !string.IsNullOrEmpty(n)).ToArray();
        var trns = new[] { options.Trn, options.TrnTokenTrnHint }.Where(trn => trn is not null).Distinct().Select(NormalizeTrn).ToArray();
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(options.NationalInsuranceNumber);

        var results = await dbContext.Database.SqlQueryRaw<SuggestionsQueryResult>(
                """
                WITH vars AS (
                    SELECT
                    case when array_length(:first_names, 1) > 0 then (fn_split_names(:first_names, include_synonyms => true) collate "case_insensitive") else ARRAY[]::varchar[] end first_names,
                    case when array_length(:last_names, 1) > 0 then (fn_split_names(:last_names, include_synonyms => false) collate "case_insensitive") else ARRAY[]::varchar[] end last_names,
                    :dates_of_birth dates_of_birth,
                    :trns trns,
                    array_remove(ARRAY[:national_insurance_number] COLLATE "case_insensitive", null)::varchar[] national_insurance_numbers
                )
                SELECT
                    p.person_id,
                    p.trn,
                    p.trn = ANY(vars.trns) trn_matches,
                    (SELECT ARRAY(SELECT p.trn INTERSECT SELECT UNNEST(vars.trns))) matched_trn,
                    array_length(vars.first_names, 1) > 0 AND p.names && vars.first_names first_name_matches,
                    (SELECT ARRAY(SELECT UNNEST(p.names) INTERSECT SELECT UNNEST(vars.first_names))) matched_first_name,
                    array_length(vars.last_names, 1) > 0 AND p.names && vars.last_names last_name_matches,
                    (SELECT ARRAY(SELECT UNNEST(p.names) INTERSECT SELECT UNNEST(vars.last_names))) matched_last_name,
                    p.date_of_birth = ANY(vars.dates_of_birth) date_of_birth_matches,
                    (SELECT ARRAY(SELECT p.date_of_birth INTERSECT SELECT UNNEST(vars.dates_of_birth))) matched_date_of_birth,
                    (array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers) national_insurance_number_matches,
                    (SELECT ARRAY(SELECT UNNEST(p.national_insurance_numbers) INTERSECT SELECT UNNEST(vars.national_insurance_numbers))) matched_national_insurance_number
                FROM persons p, vars
                WHERE p.status = 0 AND (
                    (array_length(vars.last_names, 1) > 0 AND p.date_of_birth = ANY(vars.dates_of_birth) AND p.names && vars.last_names) OR
                    p.trn = ANY(vars.trns) OR
                    (array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers)
                )
                """,
                // ReSharper disable FormatStringProblem
                parameters:
                [
                    new NpgsqlParameter("last_names", lastNames),
                    new NpgsqlParameter("first_names", firstNames),
                    new NpgsqlParameter("dates_of_birth", options.DatesOfBirth.ToArray()),
                    new NpgsqlParameter("national_insurance_number", NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber is not null ? nationalInsuranceNumber : DBNull.Value
                    },
                    new NpgsqlParameter("trns", NpgsqlDbType.Varchar | NpgsqlDbType.Array) { Value = trns }
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        return results
            .Select(r =>
            {
                var matchedAttributes = new List<KeyValuePair<PersonMatchedAttribute, string>>();

                void AddMatchedAttribute(bool isMatched, PersonMatchedAttribute attributeType, Func<string> getValue)
                {
                    if (isMatched)
                    {
                        matchedAttributes.Add(KeyValuePair.Create(attributeType, getValue()));
                    }
                }

                AddMatchedAttribute(r.FirstNameMatches, PersonMatchedAttribute.FirstName, () => r.MatchedFirstName.First());
                AddMatchedAttribute(r.LastNameMatches, PersonMatchedAttribute.LastName, () => r.MatchedLastName.First());
                AddMatchedAttribute(r.DateOfBirthMatches, PersonMatchedAttribute.DateOfBirth, () => r.MatchedDateOfBirth.First().ToString("yyyy-MM-dd"));
                AddMatchedAttribute(r.NationalInsuranceNumberMatches, PersonMatchedAttribute.NationalInsuranceNumber, () => r.MatchedNationalInsuranceNumber.First());
                AddMatchedAttribute(r.TrnMatches, PersonMatchedAttribute.Trn, () => r.MatchedTrn.First());

                var matchedAttributeTypes = matchedAttributes.Select(a => a.Key).ToArray();
                var score = matchedAttributeTypes.Sum(m => m switch
                {
                    PersonMatchedAttribute.Trn => 20,
                    PersonMatchedAttribute.NationalInsuranceNumber => 10,
                    PersonMatchedAttribute.DateOfBirth => 2,
                    PersonMatchedAttribute.LastName => 2,
                    PersonMatchedAttribute.FirstName => 1,
                    _ => 0
                });

                var result = new MatchPersonResult(
                    r.PersonId,
                    r.Trn,
                    matchedAttributes.AsReadOnly());

                return new { Result = result, Score = score, MatchedAttributes = matchedAttributes };
            })
            .OrderByDescending(t => t.Score)
            .Select(t => t.Result)
            .AsReadOnly();
    }

    public async Task<string?> GetPendingSupportTaskReferenceByUserAsync(string oneLoginUserSubject)
    {
        var task = await dbContext.SupportTasks
            .Where(t => t.OneLoginUserSubject == oneLoginUserSubject && t.Status != SupportTaskStatus.Closed)
            .OrderBy(t => t.CreatedOn)
            .FirstOrDefaultAsync();

        return task?.SupportTaskReference;
    }

    private static string? NormalizeTrn(string? value) =>
        string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());

    [UsedImplicitly]
    private record SuggestionsQueryResult(
        Guid PersonId,
        string Trn,
        bool TrnMatches,
        string[] MatchedTrn,
        bool FirstNameMatches,
        string[] MatchedFirstName,
        bool LastNameMatches,
        string[] MatchedLastName,
        bool DateOfBirthMatches,
        DateOnly[] MatchedDateOfBirth,
        bool NationalInsuranceNumberMatches,
        string[] MatchedNationalInsuranceNumber);

    [UsedImplicitly]
    private record MatchedAttributesQueryResult(string AttributeType, string AttributeValue);
}
