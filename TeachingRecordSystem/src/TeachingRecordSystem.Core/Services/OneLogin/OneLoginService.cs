using System.Diagnostics;
using System.Transactions;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Notify;
using static TeachingRecordSystem.Core.Services.OneLogin.IdModelTypes;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public class OneLoginService(
    TrsDbContext dbContext,
    IdDbContext idDbContext,
    INotificationSender notificationSender,
    IEventPublisher eventPublisher,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock)
{
    // ID's database has a user_id column to indicate that a TRN token has been used already.
    // This sentinel value indicates the token has been used by us, rather than a teacher ID user.
    private static readonly Guid _teacherAuthIdUserIdSentinel = Guid.Empty;

    public Task<string> GetRecordNotFoundEmailContentHtmlAsync(string personName)
    {
        return notificationSender.RenderEmailTemplateHtmlAsync(
            EmailTemplateIds.OneLoginCannotFindRecord,
            GetOneLoginCannotFindRecordEmailPersonalization(personName),
            stripLinks: true);
    }

    public async Task EnqueueNotVerifiedEmailAsync(string emailAddress, string personName, string reason, ProcessContext processContext)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.OneLoginNotVerified,
            EmailAddress = emailAddress,
            Personalization = GetOneLoginNotVerifiedEmailPersonalization(personName, reason).ToDictionary()
        };

        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
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

        var oldOneLoginUserEventModel = EventModels.OneLoginUser.FromModel(user);

        user.SetVerified(
            processContext.Now,
            options.VerificationRoute,
            verifiedByApplicationUserId: null,
            options.VerifiedNames,
            options.VerifiedDatesOfBirth,
            options.CoreIdentityClaimVc);

        await dbContext.SaveChangesAsync();

        var oneLoginUserEventModel = EventModels.OneLoginUser.FromModel(user);
        var updatedEvent = new OneLoginUserUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            OneLoginUser = oneLoginUserEventModel,
            OldOneLoginUser = oneLoginUserEventModel,
            Changes = OneLoginUserUpdatedEvent.GetChanges(oldOneLoginUserEventModel, oneLoginUserEventModel)
        };

        await eventPublisher.PublishEventAsync(updatedEvent, processContext);
    }

    public async Task SetUserMatchedAsync(SetUserMatchedOptions options, ProcessContext processContext)
    {
        var user = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == options.OneLoginUserSubject);

        if (user.VerifiedOn is null)
        {
            throw new InvalidOperationException("User must be verified.");
        }

        var oldOneLoginUserEventModel = EventModels.OneLoginUser.FromModel(user);

        user.SetMatched(processContext.Now, options.MatchedPersonId, options.MatchRoute, options.MatchedAttributes);

        await dbContext.SaveChangesAsync();

        var oneLoginUserEventModel = EventModels.OneLoginUser.FromModel(user);
        var updatedEvent = new OneLoginUserUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            OneLoginUser = oneLoginUserEventModel,
            OldOneLoginUser = oneLoginUserEventModel,
            Changes = OneLoginUserUpdatedEvent.GetChanges(oldOneLoginUserEventModel, oneLoginUserEventModel)
        };

        await eventPublisher.PublishEventAsync(updatedEvent, processContext);
    }

    public async Task SetUserVerifiedAndMatchedAsync(SetUserVerifiedAndMatchedOptions options, ProcessContext processContext)
    {
        var user = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == options.OneLoginUserSubject);

        if (user.VerifiedOn is not null)
        {
            throw new InvalidOperationException("User is already verified.");
        }

        var oldOneLoginUserEventModel = EventModels.OneLoginUser.FromModel(user);

        user.SetVerified(
            processContext.Now,
            options.VerificationRoute,
            verifiedByApplicationUserId: null,
            options.VerifiedNames,
            options.VerifiedDatesOfBirth,
            options.CoreIdentityClaimVc);

        user.SetMatched(processContext.Now, options.MatchedPersonId, options.MatchRoute, options.MatchedAttributes);

        await dbContext.SaveChangesAsync();

        var oneLoginUserEventModel = EventModels.OneLoginUser.FromModel(user);
        var updatedEvent = new OneLoginUserUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            OneLoginUser = oneLoginUserEventModel,
            OldOneLoginUser = oneLoginUserEventModel,
            Changes = OneLoginUserUpdatedEvent.GetChanges(oldOneLoginUserEventModel, oneLoginUserEventModel)
        };

        await eventPublisher.PublishEventAsync(updatedEvent, processContext);
    }

    public async Task<FindTeacherIdentityUserResult?> FindTeacherIdentityUserAsync(
        IEnumerable<string[]> verifiedNames,
        IEnumerable<DateOnly> verifiedDatesOfBirth,
        string? trnToken,
        string emailAddress)
    {
        Person? getAnIdentityPerson = null;
        OneLoginUserMatchRoute? matchRoute = null;
        IdTrnToken? trnTokenModel = null;

        // First try and match on TRN Token
        if (trnToken is not null)
        {
            trnTokenModel = await WithIdDbContextAsync(c => c.TrnTokens.SingleOrDefaultAsync(
                t => t.TrnToken == trnToken && t.ExpiresUtc > clock.UtcNow && t.UserId == null));
            if (trnTokenModel is not null)
            {
                getAnIdentityPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == trnTokenModel.Trn);
                matchRoute = getAnIdentityPerson is not null ? OneLoginUserMatchRoute.TrnToken : null;
            }
        }

        // Couldn't match on TRN Token, try and match on email and TRN
        if (getAnIdentityPerson is null)
        {
            var identityUser = await WithIdDbContextAsync(c => c.Users.SingleOrDefaultAsync(
                u => u.EmailAddress == emailAddress
                    && u.Trn != null
                    && u.IsDeleted == false
                    && (u.TrnVerificationLevel == TrnVerificationLevel.Medium
                        || u.TrnAssociationSource == TrnAssociationSource.TrnToken
                        || u.TrnAssociationSource == TrnAssociationSource.SupportUi)));
            if (identityUser is not null)
            {
                getAnIdentityPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == identityUser.Trn);
                matchRoute = getAnIdentityPerson is not null ? OneLoginUserMatchRoute.GetAnIdentityUser : null;
            }
        }

        if (getAnIdentityPerson is null)
        {
            return null;
        }

        // Check the record's last name and DOB match the verified details
        var matchedLastName = verifiedNames.Select(parts => parts.Last()).FirstOrDefault(name => name.Equals(getAnIdentityPerson.LastName, StringComparison.OrdinalIgnoreCase));
        var matchedDateOfBirth = verifiedDatesOfBirth.FirstOrDefault(dob => dob == getAnIdentityPerson.DateOfBirth);
        if (matchedLastName == default || matchedDateOfBirth == default)
        {
            return new(getAnIdentityPerson.PersonId, getAnIdentityPerson.Trn, MatchRoute: null, MatchedAttributes: null);
        }
        var matchedAttributes = new Dictionary<PersonMatchedAttribute, string>()
        {
            { PersonMatchedAttribute.LastName, matchedLastName },
            { PersonMatchedAttribute.DateOfBirth, matchedDateOfBirth.ToString("yyyy-MM-dd") }
        };

        if (trnTokenModel is not null)
        {
            // Invalidate the token
            trnTokenModel.UserId = _teacherAuthIdUserIdSentinel;
            await WithIdDbContextAsync(c => c.SaveChangesAsync());
        }

        return new(getAnIdentityPerson.PersonId, getAnIdentityPerson.Trn, MatchRoute: matchRoute, matchedAttributes);

        async Task<T> WithIdDbContextAsync<T>(Func<IdDbContext, Task<T>> action)
        {
            using var sc = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            return await action(idDbContext);
        }
    }

    private static IReadOnlyDictionary<string, string> GetOneLoginCannotFindRecordEmailPersonalization(string personName) =>
        new Dictionary<string, string> { ["name"] = personName };

    private static IReadOnlyDictionary<string, string> GetOneLoginNotVerifiedEmailPersonalization(string personName, string reason) =>
        new Dictionary<string, string>
        {
            ["name"] = personName,
            ["reason"] = reason
        };

    public virtual async Task<MatchPersonResult?> MatchPersonAsync(MatchPersonOptions options)
    {
        // A One Login is matched if there is exactly one Person with a matching
        // first name, last name, DOB AND either NINO or TRN.

        var suggestedMatches = await GetSuggestedPersonMatchesAsync(
            new GetSuggestedPersonMatchesOptions(
                options.Names,
                options.DatesOfBirth,
                options.EmailAddress,
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
                    (:email_address COLLATE "case_insensitive") email_address,
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
                    CASE WHEN vars.email_address IS NOT NULL AND p.email_address = vars.email_address THEN true ELSE false END email_address_matches,
                    p.email_address matched_email_address,
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
                    new NpgsqlParameter("email_address", NpgsqlDbType.Varchar)
                    {
                        Value = options.EmailAddress ?? (object)DBNull.Value
                    },
                    new NpgsqlParameter("dates_of_birth", options.DatesOfBirth.ToArray()),
                    new NpgsqlParameter("national_insurance_number", NpgsqlDbType.Varchar)
                    {
                        Value = nationalInsuranceNumber ?? (object)DBNull.Value
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
                AddMatchedAttribute(r.EmailAddressMatches, PersonMatchedAttribute.EmailAddress, () => r.MatchedEmailAddress!);
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

    public async Task<OneLoginUser> OnSignInAsync(string sub, string email, ProcessContext processContext)
    {
        EventModels.OneLoginUser? oldOneLoginUserEventModel;

        var oneLoginUser = await dbContext.OneLoginUsers
            .Include(u => u.Person)
            .SingleOrDefaultAsync(u => u.Subject == sub);

        if (oneLoginUser is null)
        {
            oldOneLoginUserEventModel = null;

            oneLoginUser = new()
            {
                Subject = sub,
                EmailAddress = email
            };
            dbContext.OneLoginUsers.Add(oneLoginUser);
        }
        else
        {
            oldOneLoginUserEventModel = EventModels.OneLoginUser.FromModel(oneLoginUser);

            oneLoginUser.EmailAddress = email;
        }

        if (oneLoginUser.PersonId is null)
        {
            await TryMatchToTrnRequestAsync(oneLoginUser, processContext);
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new OneLoginUserSignedInEvent
            {
                EventId = Guid.NewGuid(),
                Subject = oneLoginUser.Subject
            },
            processContext);

        var oneLoginUserEventModel = EventModels.OneLoginUser.FromModel(oneLoginUser);

        if (oldOneLoginUserEventModel is null)
        {
            await eventPublisher.PublishEventAsync(
                new OneLoginUserCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OneLoginUser = oneLoginUserEventModel
                },
                processContext);
        }
        else
        {
            var changes = OneLoginUserUpdatedEvent.GetChanges(oldOneLoginUserEventModel, oneLoginUserEventModel);

            if (changes is not OneLoginUserUpdatedEventChanges.None)
            {
                await eventPublisher.PublishEventAsync(
                    new OneLoginUserUpdatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        OneLoginUser = oneLoginUserEventModel,
                        OldOneLoginUser = oldOneLoginUserEventModel,
                        Changes = changes
                    },
                    processContext);
            }
        }

        return oneLoginUser;
    }

    private static string? NormalizeTrn(string? value) =>
        string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());

    private async Task<TryMatchToTrnRequestResult?> TryMatchToTrnRequestAsync(OneLoginUser oneLoginUser, ProcessContext processContext)
    {
        Debug.Assert(oneLoginUser.EmailAddress is not null);

        var requestAndResolvedPerson = await dbContext.TrnRequestMetadata
            .Join(dbContext.Persons, r => r.ResolvedPersonId, p => p.PersonId, (m, p) => new { TrnRequestMetadata = m, ResolvedPerson = p })
            .Where(m => m.TrnRequestMetadata.OneLoginUserSubject == oneLoginUser.Subject || m.TrnRequestMetadata.EmailAddress == oneLoginUser.EmailAddress)
            .ToArrayAsync();

        if (requestAndResolvedPerson is not [{ TrnRequestMetadata: var trnRequestMetadata, ResolvedPerson: var resolvedPerson }])
        {
            return null;
        }

        if (trnRequestMetadata.IdentityVerified != true)
        {
            return null;
        }

        if (trnRequestMetadata.Status is not TrnRequestStatus.Completed)
        {
            return null;
        }
        Debug.Assert(trnRequestMetadata.ResolvedPersonId.HasValue);

        oneLoginUser.SetVerified(
            verifiedOn: trnRequestMetadata.CreatedOn,
            route: OneLoginUserVerificationRoute.External,
            verifiedByApplicationUserId: trnRequestMetadata.ApplicationUserId,
            verifiedNames: [trnRequestMetadata.Name],
            verifiedDatesOfBirth: [trnRequestMetadata.DateOfBirth],
            coreIdentityClaimVc: null);

        oneLoginUser.SetMatched(
            processContext.Now,
            trnRequestMetadata.ResolvedPersonId!.Value,
            route: OneLoginUserMatchRoute.TrnRequest,
            matchedAttributes: null);

        return new(resolvedPerson.Trn);
    }

    public record FindTeacherIdentityUserResult(
        Guid PersonId,
        string Trn,
        OneLoginUserMatchRoute? MatchRoute,
        IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>? MatchedAttributes);

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
        string? MatchedEmailAddress,
        bool EmailAddressMatches,
        DateOnly[] MatchedDateOfBirth,
        bool NationalInsuranceNumberMatches,
        string[] MatchedNationalInsuranceNumber);

    [UsedImplicitly]
    private record MatchedAttributesQueryResult(string AttributeType, string AttributeValue);

    private record TryMatchToTrnRequestResult(string Trn);
}
