using System.Diagnostics;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record TrnRequestInfo(TrnRequestMetadata TrnRequest, string? ResolvedPersonTrn);

public class TrnRequestService(
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    SupportTaskService supportTaskService,
    IGetAnIdentityApiClient idApiClient,
    ITrnGenerator trnGenerator,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptionsAccessor,
    IOptions<TrnRequestOptions> trnRequestOptionsAccessor)
{
    public async Task<TrnRequestInfo> CreateTrnRequestAsync(CreateTrnRequestOptions options, ProcessContext processContext)
    {
        var trnRequest = new TrnRequestMetadata
        {
            ApplicationUserId = options.ApplicationUserId,
            RequestId = options.RequestId,
            CreatedOn = processContext.Now,
            IdentityVerified = options.OneLoginUserInfo?.IdentityVerified,
            EmailAddress = options.EmailAddress,
            OneLoginUserSubject = options.OneLoginUserInfo?.OneLoginUserSubject,
            FirstName = options.FirstName,
            MiddleName = options.MiddleName,
            LastName = options.LastName,
            PreviousFirstName = options.PreviousFirstName,
            PreviousMiddleName = options.PreviousMiddleName,
            PreviousLastName = options.PreviousLastName,
            Name = new[] { options.FirstName, options.MiddleName, options.LastName }.Where(n => n is not null).ToArray()!,
            DateOfBirth = options.DateOfBirth,
            PotentialDuplicate = false,  // We'll fix this below; we need to run the matching process first
            NationalInsuranceNumber = options.NationalInsuranceNumber,
            Gender = options.Gender,
            NpqWorkingInEducationalSetting = options.NpqWorkingInEducationalSetting,
            NpqApplicationId = options.NpqApplicationId,
            NpqName = options.NpqName,
            NpqTrainingProvider = options.NpqTrainingProvider,
            NpqEvidenceFileId = options.NpqEvidenceFileId,
            NpqEvidenceFileName = options.NpqEvidenceFileName,
            WorkEmailAddress = options.WorkEmailAddress
        };

        var matchResult = await MatchPersonsAsync(trnRequest);

        dbContext.TrnRequestMetadata.Add(trnRequest);

        await dbContext.SaveChangesAsync();

        string? trn = null;
        if (options.TryResolve)
        {
            if (matchResult.Outcome is MatchPersonsResultOutcome.DefiniteMatch)
            {
                trn = matchResult.Trn;
                await CompleteTrnRequestWithMatchedPersonAsync(trnRequest, (matchResult.PersonId, trn), publishTrnRequestUpdatedEvent: false, processContext);
            }
            else if (matchResult.Outcome is MatchPersonsResultOutcome.NoMatches)
            {
                trn = await CompleteTrnRequestWithNewRecordAsync(trnRequest, publishTrnRequestUpdatedEvent: false, processContext);
            }
            else
            {
                Debug.Assert(matchResult.Outcome is MatchPersonsResultOutcome.PotentialMatches);
            }
        }

        await eventPublisher.PublishEventAsync(
            new TrnRequestCreatedEvent
            {
                EventId = Guid.NewGuid(),
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest)
            },
            processContext);

        return new TrnRequestInfo(trnRequest, trn);
    }

    public Task CompleteTrnRequestWithMatchedPersonAsync(
            TrnRequestMetadata trnRequest,
            (Guid PersonId, string Trn) person,
            ProcessContext processContext) =>
        CompleteTrnRequestWithMatchedPersonAsync(
            trnRequest,
            (person.PersonId, person.Trn),
            publishTrnRequestUpdatedEvent: true,
            processContext);

    public async Task CompleteTrnRequestWithMatchedPersonAsync(
        TrnRequestMetadata trnRequest,
        Person person,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate,
        ProcessContext processContext)
    {
        await CompleteTrnRequestWithMatchedPersonAsync(
            trnRequest,
            (person.PersonId, person.Trn!),
            publishTrnRequestUpdatedEvent: true,
            processContext);

        await UpdatePersonFromTrnRequestAsync(person, trnRequest, attributesToUpdate, processContext);
    }

    private async Task CompleteTrnRequestWithMatchedPersonAsync(
        TrnRequestMetadata trnRequest,
        (Guid PersonId, string Trn) person,
        bool publishTrnRequestUpdatedEvent,
        ProcessContext processContext)
    {
        if (trnRequest.Status is not TrnRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} requests can be completed.");
        }

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        var furtherChecksNeeded = await RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, trnRequest.ApplicationUserId);

        var status = furtherChecksNeeded ? TrnRequestStatus.Pending : TrnRequestStatus.Completed;
        trnRequest.SetResolvedPerson(person.PersonId, status);
        await TryEnsureTrnTokenAsync(trnRequest, person.Trn!);
        await dbContext.SaveChangesAsync();

        if (furtherChecksNeeded)
        {
            await supportTaskService.CreateSupportTaskAsync(
                new CreateSupportTaskOptions
                {
                    SupportTaskType = SupportTaskType.TrnRequestManualChecksNeeded,
                    Data = new TrnRequestManualChecksNeededData(),
                    PersonId = person.PersonId,
                    OneLoginUserSubject = trnRequest.OneLoginUserSubject,
                    TrnRequest = (trnRequest.ApplicationUserId, trnRequest.RequestId)
                },
                processContext);
        }

        if (publishTrnRequestUpdatedEvent)
        {
            await eventPublisher.PublishEventAsync(
                new TrnRequestUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SourceApplicationUserId = trnRequest.ApplicationUserId,
                    RequestId = trnRequest.RequestId,
                    Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
                    TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                    OldTrnRequest = oldTrnRequestEventModel,
                    ReasonDetails = null
                },
                processContext);
        }
    }

    public Task<string> CompleteTrnRequestWithNewRecordAsync(TrnRequestMetadata trnRequest, ProcessContext processContext) =>
        CompleteTrnRequestWithNewRecordAsync(trnRequest, publishTrnRequestUpdatedEvent: true, processContext);

    private async Task<string> CompleteTrnRequestWithNewRecordAsync(
        TrnRequestMetadata trnRequest,
        bool publishTrnRequestUpdatedEvent,
        ProcessContext processContext)
    {
        if (trnRequest.Status is not TrnRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} requests can be completed.");
        }

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        var trn = await trnGenerator.GenerateTrnAsync();

        // TODO Use PersonService when we have one
        var (person, _) = Person.Create(
            trn,
            trnRequest.FirstName!,
            trnRequest.MiddleName ?? string.Empty,
            trnRequest.LastName!,
            trnRequest.DateOfBirth,
            trnRequest.EmailAddress is string emailAddress && !string.IsNullOrEmpty(emailAddress)
                ? EmailAddress.Parse(emailAddress)
                : null,
            trnRequest.NationalInsuranceNumber is string nationalInsuranceNumber && !string.IsNullOrEmpty(nationalInsuranceNumber)
                ? NationalInsuranceNumber.Parse(nationalInsuranceNumber)
                : null,
            trnRequest.Gender,
            processContext.Now,
            (trnRequest.ApplicationUserId, trnRequest.RequestId));

        dbContext.Persons.Add(person);

        trnRequest.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);

        await TryEnsureTrnTokenAsync(trnRequest, trn);

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                CreateReason = null,
                CreateReasonDetail = null,
                EvidenceFile = null,
                TrnRequestMetadata = EventModels.TrnRequestMetadata.FromModel(trnRequest)
            },
            processContext);

        if (publishTrnRequestUpdatedEvent)
        {
            await eventPublisher.PublishEventAsync(
                new TrnRequestUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SourceApplicationUserId = trnRequest.ApplicationUserId,
                    RequestId = trnRequest.RequestId,
                    Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
                    TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                    OldTrnRequest = oldTrnRequestEventModel,
                    ReasonDetails = null
                },
                processContext);
        }

        return trn;
    }

    public async Task RejectTrnRequestAsync(TrnRequestMetadata trnRequest, ProcessContext processContext)
    {
        if (trnRequest.Status is not TrnRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} requests can be rejected.");
        }

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        trnRequest.SetRejected();

        await eventPublisher.PublishEventAsync(
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = trnRequest.ApplicationUserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                OldTrnRequest = oldTrnRequestEventModel,
                ReasonDetails = null
            },
            processContext);
    }

    public async Task<TrnRequestInfo?> GetTrnRequestAsync(Guid applicationUserId, string requestId)
    {
        // TODO Use LeftJoin when we've moved to EF Core 10
        var result = await (
                from m in dbContext.TrnRequestMetadata
                join person in dbContext.Persons on m.ResolvedPersonId equals person.PersonId into pg
                from p in pg.DefaultIfEmpty()
                where m.ApplicationUserId == applicationUserId && m.RequestId == requestId
                select new { TrnRequest = m, Trn = p.Trn })
            .SingleOrDefaultAsync();

        if (result is null)
        {
            return null;
        }

        if (await TryEnsureTrnTokenAsync(result.TrnRequest, result.Trn!))
        {
            await dbContext.SaveChangesAsync();
        }

        return new(result.TrnRequest, result.Trn);
    }

    public string GetAccessYourTeachingQualificationsLink(string trnToken) =>
        $"{aytqOptionsAccessor.Value.BaseAddress}{aytqOptionsAccessor.Value.StartUrlPath}?trn_token={Uri.EscapeDataString(trnToken)}";

    public async Task<bool> RequiresFurtherChecksNeededSupportTaskAsync(Guid personId, Guid trnRequestApplicationUserId)
    {
        if (!trnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds.Contains(trnRequestApplicationUserId))
        {
            return false;
        }

        var personFlags = await dbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new { HasQts = p.QtsDate != null, HasEyts = p.EytsDate != null, HasOpenAlert = p.Alerts!.Any(a => a.IsOpen) })
            .SingleAsync();

        if (personFlags is { HasQts: false, HasEyts: false, HasOpenAlert: false })
        {
            return false;
        }

        return true;
    }

    // TODO Remove this
    public CreatePersonResult CreatePersonFromTrnRequest(TrnRequestMetadata trnRequest, string trn, DateTime now) =>
        Person.Create(
            trn,
            trnRequest.FirstName!,
            trnRequest.MiddleName ?? string.Empty,
            trnRequest.LastName!,
            trnRequest.DateOfBirth,
            trnRequest.EmailAddress is string emailAddress && !string.IsNullOrEmpty(emailAddress)
                ? EmailAddress.Parse(emailAddress)
                : null,
            trnRequest.NationalInsuranceNumber is string nationalInsuranceNumber && !string.IsNullOrEmpty(nationalInsuranceNumber)
                ? NationalInsuranceNumber.Parse(nationalInsuranceNumber)
                : null,
            trnRequest.Gender,
            now,
            (trnRequest.ApplicationUserId, trnRequest.RequestId));

    private async Task UpdatePersonFromTrnRequestAsync(
        Person person,
        TrnRequestMetadata trnRequest,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate,
        ProcessContext processContext)
    {
        var oldPersonDetailsEventModel = EventModels.PersonDetails.FromModel(person);

        // TODO Use PersonService when we have one
        var updateResult = person.UpdateDetails(
            firstName: attributesToUpdate.Contains(PersonMatchedAttribute.FirstName)
                ? Option.Some(trnRequest.FirstName!)
                : Option.None<string>(),
            middleName: attributesToUpdate.Contains(PersonMatchedAttribute.MiddleName)
                ? Option.Some(trnRequest.MiddleName ?? string.Empty)
                : Option.None<string>(),
            lastName: attributesToUpdate.Contains(PersonMatchedAttribute.LastName)
                ? Option.Some(trnRequest.LastName!)
                : Option.None<string>(),
            dateOfBirth: attributesToUpdate.Contains(PersonMatchedAttribute.DateOfBirth)
                ? Option.Some<DateOnly?>(trnRequest.DateOfBirth)
                : Option.None<DateOnly?>(),
            emailAddress: attributesToUpdate.Contains(PersonMatchedAttribute.EmailAddress)
                ? Option.Some(trnRequest.EmailAddress is string emailAddress ? EmailAddress.Parse(emailAddress) : null)
                : Option.None<EmailAddress?>(),
            nationalInsuranceNumber: attributesToUpdate.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                ? Option.Some(trnRequest.NationalInsuranceNumber is string nationalInsuranceNumber
                    ? NationalInsuranceNumber.Parse(nationalInsuranceNumber)
                    : null)
                : Option.None<NationalInsuranceNumber?>(),
            gender: attributesToUpdate.Contains(PersonMatchedAttribute.Gender)
                ? Option.Some(trnRequest.Gender)
                : Option.None<Gender?>(),
            processContext.Now);

        await dbContext.SaveChangesAsync();

        if (updateResult.Changes != 0)
        {
            var changes = PersonDetailsUpdatedEventChanges.None |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.FirstName) ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.MiddleName) ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.LastName) ? PersonDetailsUpdatedEventChanges.LastName : 0) |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.DateOfBirth) ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.EmailAddress) ? PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.NationalInsuranceNumber) ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
                (updateResult.Changes.HasFlag(LegacyEvents.PersonAttributesChanges.Gender) ? PersonDetailsUpdatedEventChanges.Gender : 0);

            await eventPublisher.PublishEventAsync(
                new PersonDetailsUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = person.PersonId,
                    PersonDetails = EventModels.PersonDetails.FromModel(person),
                    OldPersonDetails = oldPersonDetailsEventModel,
                    NameChangeReason = null,
                    NameChangeEvidenceFile = null,
                    DetailsChangeReason = null,
                    DetailsChangeReasonDetail = null,
                    DetailsChangeEvidenceFile = null,
                    Changes = changes
                },
                processContext);
        }
    }

    // TODO Remove this
    public UpdatePersonDetailsResult UpdatePersonFromTrnRequest(
        Person person,
        TrnRequestMetadata trnRequest,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate,
        DateTime now)
    {
        Debug.Assert(person.PersonId == trnRequest.ResolvedPersonId);

        return person.UpdateDetails(
            firstName: attributesToUpdate.Contains(PersonMatchedAttribute.FirstName)
                ? Option.Some(trnRequest.FirstName!)
                : Option.None<string>(),
            middleName: attributesToUpdate.Contains(PersonMatchedAttribute.MiddleName)
                ? Option.Some(trnRequest.MiddleName ?? string.Empty)
                : Option.None<string>(),
            lastName: attributesToUpdate.Contains(PersonMatchedAttribute.LastName)
                ? Option.Some(trnRequest.LastName!)
                : Option.None<string>(),
            dateOfBirth: attributesToUpdate.Contains(PersonMatchedAttribute.DateOfBirth)
                ? Option.Some<DateOnly?>(trnRequest.DateOfBirth)
                : Option.None<DateOnly?>(),
            emailAddress: attributesToUpdate.Contains(PersonMatchedAttribute.EmailAddress)
                ? Option.Some(trnRequest.EmailAddress is string emailAddress ? EmailAddress.Parse(emailAddress) : null)
                : Option.None<EmailAddress?>(),
            nationalInsuranceNumber: attributesToUpdate.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                ? Option.Some(trnRequest.NationalInsuranceNumber is string nationalInsuranceNumber
                    ? NationalInsuranceNumber.Parse(nationalInsuranceNumber)
                    : null)
                : Option.None<NationalInsuranceNumber?>(),
            gender: attributesToUpdate.Contains(PersonMatchedAttribute.Gender)
                ? Option.Some(trnRequest.Gender)
                : Option.None<Gender?>(),
            now);
    }

    private async Task<string> CreateTrnTokenAsync(string trn, string emailAddress)
    {
        var response = await idApiClient.CreateTrnTokenAsync(new CreateTrnTokenRequest() { Email = emailAddress, Trn = trn });
        return response.TrnToken;
    }

    // internal for testing
    internal async Task<bool> TryEnsureTrnTokenAsync(TrnRequestMetadata trnRequest, string resolvedPersonTrn)
    {
        if (trnRequest.Status is not TrnRequestStatus.Completed || trnRequest.TrnToken is not null || trnRequest.EmailAddress is null)
        {
            return false;
        }

        trnRequest.TrnToken = await CreateTrnTokenAsync(resolvedPersonTrn, trnRequest.EmailAddress);
        return true;
    }

    public async Task<MatchPersonsResult> MatchPersonsAsync(TrnRequestMetadata request, params Guid[] excludePersonIds)
    {
        request.PotentialDuplicate = false;

        var results = (await GetMatchesFromTrnRequestAsync(request)).ToList();
        results.RemoveAll(r => excludePersonIds.Contains(r.person_id));

        if (results.Count == 0)
        {
            return MatchPersonsResult.NoMatches();
        }

        var matchedOnDobAndNino = results.Where(r => r is { date_of_birth_matches: true, national_insurance_number_matches: true }).ToArray();

        if (matchedOnDobAndNino is [var singleDobAndNinoMatch])
        {
            return MatchPersonsResult.DefiniteMatch(singleDobAndNinoMatch.person_id, singleDobAndNinoMatch.trn);
        }

        var matchedOnNameDateOfBirthEmailAndGender = results
            .Where(r => r is
            {
                first_name_matches: true,
                last_name_matches: true,
                date_of_birth_matches: true,
                email_address_matches: true,
                gender_matches: true
            })
            .ToArray();

        if (matchedOnNameDateOfBirthEmailAndGender is [var singleNameDobEmailGenderMatch] && string.IsNullOrEmpty(request.NationalInsuranceNumber))
        {
            return MatchPersonsResult.DefiniteMatch(singleNameDobEmailGenderMatch.person_id, singleNameDobEmailGenderMatch.trn);
        }

        request.PotentialDuplicate = true;
        return MatchPersonsResult.PotentialMatches(results.Select(r => r.person_id));
    }

    public async Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedPersonMatchesAsync(TrnRequestMetadata request)
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

#pragma warning disable IDE1006 // Naming Styles
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
