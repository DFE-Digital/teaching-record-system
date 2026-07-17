using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record TrnRequestInfo(TrnRequestMetadata TrnRequest, string? ResolvedPersonTrn);

public class TrnRequestService(
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    OneLoginService oneLoginService,
    SupportTaskService supportTaskService,
    PersonService personService,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptionsAccessor,
    IOptions<TrnRequestOptions> trnRequestOptionsAccessor)
{
    /// The set of SupportTaskTypes that can resolve the attached TRN request
    private static readonly HashSet<SupportTaskType> _trnRequestResolvingSupportTaskTypes =
    [
        SupportTaskType.TrnRequest,
        SupportTaskType.OneLoginUserRecordMatching,
        SupportTaskType.OneLoginUserIdVerification
    ];

    public async Task<TrnRequestInfo> CreateTrnRequestAsync(CreateTrnRequestOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

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
            WorkEmailAddress = options.WorkEmailAddress,
            Status = options.TryResolve ? TrnRequestStatus.Pending : TrnRequestStatus.Dormant
        };

        dbContext.TrnRequestMetadata.Add(trnRequest);

        await dbContext.SaveChangesAsync();

        var result = new TrnRequestInfo(trnRequest, ResolvedPersonTrn: null);

        if (options.TryResolve)
        {
            result = await TryResolveAsync(trnRequest, processContext);
        }

        await eventScope.PublishEventAsync(
            new TrnRequestCreatedEvent
            {
                EventId = Guid.NewGuid(),
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest)
            });

        return result;
    }

    public async Task ResolveTrnRequestWithMatchedPersonAsync(
        Guid applicationUserId,
        string requestId,
        Guid personId,
        ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        var person = await dbContext.Persons.FindOrThrowAsync(personId);

        await ResolveTrnRequestWithMatchedPersonAsync(
            trnRequest,
            person,
            publishTrnRequestUpdatedEvent: true,
            processContext);
    }

    public async Task ResolveTrnRequestWithMatchedPersonAsync(
        Guid applicationUserId,
        string requestId,
        Guid personId,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate,
        ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        var person = await dbContext.Persons.FindOrThrowAsync(personId);

        await ResolveTrnRequestWithMatchedPersonAsync(
            trnRequest,
            person,
            publishTrnRequestUpdatedEvent: true,
            processContext);

        if (attributesToUpdate.Count != 0)
        {
            await personService.UpdatePersonDetailsAsync(
                new UpdatePersonDetailsOptions
                {
                    PersonId = person.PersonId,
                    CreatePreviousName = false,
                    FirstName =
                        attributesToUpdate.Contains(PersonMatchedAttribute.FirstName)
                            ? Option.Some(trnRequest.FirstName!)
                            : default,
                    MiddleName =
                        attributesToUpdate.Contains(PersonMatchedAttribute.MiddleName)
                            ? Option.Some(trnRequest.MiddleName ?? string.Empty)
                            : default,
                    LastName =
                        attributesToUpdate.Contains(PersonMatchedAttribute.LastName)
                            ? Option.Some(trnRequest.LastName!)
                            : default,
                    DateOfBirth =
                        attributesToUpdate.Contains(PersonMatchedAttribute.DateOfBirth)
                            ? Option.Some<DateOnly?>(trnRequest.DateOfBirth)
                            : default,
                    EmailAddress =
                        attributesToUpdate.Contains(PersonMatchedAttribute.EmailAddress) &&
                        !string.IsNullOrEmpty(trnRequest.EmailAddress)
                            ? Option.Some<EmailAddress?>(EmailAddress.Parse(trnRequest.EmailAddress))
                            : default,
                    NationalInsuranceNumber =
                        attributesToUpdate.Contains(PersonMatchedAttribute.NationalInsuranceNumber) &&
                        !string.IsNullOrEmpty(trnRequest.NationalInsuranceNumber)
                            ? Option.Some<NationalInsuranceNumber?>(
                                NationalInsuranceNumber.Parse(trnRequest.NationalInsuranceNumber))
                            : default,
                    Gender = attributesToUpdate.Contains(PersonMatchedAttribute.Gender)
                        ? Option.Some(trnRequest.Gender)
                        : default
                },
                processContext);
        }
    }

    private async Task ResolveTrnRequestWithMatchedPersonAsync(
        TrnRequestMetadata trnRequest,
        Person person,
        bool publishTrnRequestUpdatedEvent,
        ProcessContext processContext)
    {
        if (trnRequest.Status is not (TrnRequestStatus.Pending or TrnRequestStatus.Dormant))
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} or {TrnRequestStatus.Dormant} requests can be completed.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        var furtherChecksNeeded = await RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, trnRequest.ApplicationUserId);

        trnRequest.ResolvedPersonId = person.PersonId;
        trnRequest.Status = furtherChecksNeeded ? TrnRequestStatus.Pending : TrnRequestStatus.Completed;

        await TryEnsureTrnTokenAsync(trnRequest, person.Trn);

        await dbContext.SaveChangesAsync();

        await EnsureOneLoginUserConnectedAsync(trnRequest, processContext);

        if (furtherChecksNeeded)
        {
            await CreateManualChecksNeededSupportTaskAsync(
                new CreateManualChecksNeededSupportTaskOptions
                {
                    Person = person,
                    TrnRequest = trnRequest
                },
                processContext);
        }

        if (publishTrnRequestUpdatedEvent)
        {
            await eventScope.PublishEventAsync(
                new TrnRequestUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SourceApplicationUserId = trnRequest.ApplicationUserId,
                    RequestId = trnRequest.RequestId,
                    Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
                    TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                    OldTrnRequest = oldTrnRequestEventModel,
                    ReasonDetails = null
                });
        }
    }

    public async Task<string> ResolveTrnRequestWithNewRecordAsync(
        Guid applicationUserId,
        string requestId,
        ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        return await ResolveTrnRequestWithNewRecordAsync(trnRequest, publishTrnRequestUpdatedEvent: true, processContext);
    }

    private async Task<string> ResolveTrnRequestWithNewRecordAsync(
        TrnRequestMetadata trnRequest,
        bool publishTrnRequestUpdatedEvent,
        ProcessContext processContext)
    {
        if (trnRequest.Status is not TrnRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} requests can be completed.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await personService.CreatePersonAsync(
            new CreatePersonOptions
            {
                SourceTrnRequest = (trnRequest.ApplicationUserId, trnRequest.RequestId),
                FirstName = trnRequest.FirstName!,
                MiddleName = trnRequest.MiddleName ?? string.Empty,
                LastName = trnRequest.LastName!,
                DateOfBirth = trnRequest.DateOfBirth,
                EmailAddress = !string.IsNullOrEmpty(trnRequest.EmailAddress) ? EmailAddress.Parse(trnRequest.EmailAddress) : null,
                NationalInsuranceNumber = !string.IsNullOrEmpty(trnRequest.NationalInsuranceNumber) ? NationalInsuranceNumber.Parse(trnRequest.NationalInsuranceNumber) : null,
                Gender = trnRequest.Gender
            },
            processContext);

        trnRequest.ResolvedPersonId = person.PersonId;
        trnRequest.Status = TrnRequestStatus.Completed;

        await TryEnsureTrnTokenAsync(trnRequest, person.Trn);

        await dbContext.SaveChangesAsync();

        await EnsureOneLoginUserConnectedAsync(trnRequest, processContext);

        if (publishTrnRequestUpdatedEvent)
        {
            var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

            await eventScope.PublishEventAsync(
                new TrnRequestUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SourceApplicationUserId = trnRequest.ApplicationUserId,
                    RequestId = trnRequest.RequestId,
                    Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
                    TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                    OldTrnRequest = oldTrnRequestEventModel,
                    ReasonDetails = null
                });
        }

        return person.Trn;
    }

    public async Task RejectTrnRequestAsync(Guid applicationUserId, string requestId, ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        if (trnRequest.Status is not TrnRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} requests can be rejected.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        trnRequest.Status = TrnRequestStatus.Rejected;

        await eventScope.PublishEventAsync(
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = trnRequest.ApplicationUserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                OldTrnRequest = oldTrnRequestEventModel,
                ReasonDetails = null
            });
    }

    public async Task CompleteResolvedTrnRequestAsync(Guid applicationUserId, string requestId, ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        if (trnRequest.Status is not TrnRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Pending} requests can be completed.");
        }

        if (trnRequest.ResolvedPersonId is null)
        {
            throw new InvalidOperationException("Only resolved requests can be completed.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        trnRequest.Status = TrnRequestStatus.Completed;

        await eventScope.PublishEventAsync(
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = trnRequest.ApplicationUserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                OldTrnRequest = oldTrnRequestEventModel,
                ReasonDetails = null
            });
    }

    /// Resolves the request to the record in <paramref name="options"/> (or a new one) and closes its support task,
    /// returning the ID of the record it resolved to.
    public async Task<Guid> ResolveTrnRequestAsync(ResolveTrnRequestOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(options.ApplicationUserId, options.RequestId);

        TrnRequestDataPersonAttributes? selectedPersonAttributes = null;
        TrnRequestDataPersonAttributes resolvedAttributes;

        if (options.PersonId is Guid personId)
        {
            var person = await dbContext.Persons.FindOrThrowAsync(personId);

            // Snapshot the record before resolving, which updates it.
            selectedPersonAttributes = GetPersonAttributes(person);
            resolvedAttributes = GetResolvedAttributes(options.AttributeSources, selectedPersonAttributes, trnRequest);

            await ResolveTrnRequestWithMatchedPersonAsync(
                options.ApplicationUserId,
                options.RequestId,
                personId,
                options.AttributeSources.GetAttributesToUpdate(),
                processContext);
        }
        else
        {
            // A new record takes every value from the request.
            resolvedAttributes = GetRequestAttributes(trnRequest);

            await ResolveTrnRequestWithNewRecordAsync(options.ApplicationUserId, options.RequestId, processContext);
        }

        await ResolveTrnRequestSupportTaskAsync(
            new ResolveTrnRequestSupportTaskOptions
            {
                SupportTaskReference = options.SupportTaskReference,
                ResolvedAttributes = resolvedAttributes,
                SelectedPersonAttributes = selectedPersonAttributes,
                Comments = options.Comments
            },
            processContext);

        // Resolving tracked the request's resolved record on the same entity.
        Debug.Assert(trnRequest.ResolvedPersonId is not null);
        return trnRequest.ResolvedPersonId.Value;
    }

    private static TrnRequestDataPersonAttributes GetPersonAttributes(Person person) =>
        new()
        {
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = person.EmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender
        };

    private static TrnRequestDataPersonAttributes GetRequestAttributes(TrnRequestMetadata trnRequest) =>
        new()
        {
            FirstName = trnRequest.FirstName!,
            MiddleName = trnRequest.MiddleName ?? string.Empty,
            LastName = trnRequest.LastName!,
            DateOfBirth = trnRequest.DateOfBirth,
            EmailAddress = trnRequest.EmailAddress,
            NationalInsuranceNumber = trnRequest.NationalInsuranceNumber,
            Gender = trnRequest.Gender
        };

    /// The attributes the record ends up with: only a TrnRequest source changes a value, so anything else
    /// keeps the existing one.
    private static TrnRequestDataPersonAttributes GetResolvedAttributes(
        PersonAttributeSources sources,
        TrnRequestDataPersonAttributes existingAttributes,
        TrnRequestMetadata trnRequest)
    {
        var requestAttributes = GetRequestAttributes(trnRequest);

        return new TrnRequestDataPersonAttributes()
        {
            FirstName = sources.FirstName is PersonAttributeSource.TrnRequest ? requestAttributes.FirstName : existingAttributes.FirstName,
            MiddleName = sources.MiddleName is PersonAttributeSource.TrnRequest ? requestAttributes.MiddleName : existingAttributes.MiddleName,
            LastName = sources.LastName is PersonAttributeSource.TrnRequest ? requestAttributes.LastName : existingAttributes.LastName,
            DateOfBirth = sources.DateOfBirth is PersonAttributeSource.TrnRequest ? requestAttributes.DateOfBirth : existingAttributes.DateOfBirth,
            EmailAddress = sources.EmailAddress is PersonAttributeSource.TrnRequest ? requestAttributes.EmailAddress : existingAttributes.EmailAddress,
            NationalInsuranceNumber = sources.NationalInsuranceNumber is PersonAttributeSource.TrnRequest ? requestAttributes.NationalInsuranceNumber : existingAttributes.NationalInsuranceNumber,
            Gender = sources.Gender is PersonAttributeSource.TrnRequest ? requestAttributes.Gender : existingAttributes.Gender
        };
    }

    public async Task CompleteManualChecksNeededTrnRequestAsync(
        Guid applicationUserId,
        string requestId,
        string supportTaskReference,
        ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        await CompleteResolvedTrnRequestAsync(applicationUserId, requestId, processContext);

        await CompleteManualChecksNeededSupportTaskAsync(
            new CompleteManualChecksNeededSupportTaskOptions
            {
                SupportTaskReference = supportTaskReference
            },
            processContext);
    }

    public Task<SupportTask> CreateTrnRequestSupportTaskAsync(
        CreateTrnRequestSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.TrnRequest,
                Data = new TrnRequestData(),
                PersonId = null,
                OneLoginUserSubject = null,
                TrnRequest = (options.TrnRequest.ApplicationUserId, options.TrnRequest.RequestId),
                Subject = SupportTask.Subject.FromTrnRequest(options.TrnRequest)
            },
            processContext);

    public Task<SupportTask> CreateManualChecksNeededSupportTaskAsync(
        CreateManualChecksNeededSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.TrnRequestManualChecksNeeded,
                Data = new TrnRequestManualChecksNeededData(),
                PersonId = options.Person.PersonId,
                OneLoginUserSubject = null,
                TrnRequest = (options.TrnRequest.ApplicationUserId, options.TrnRequest.RequestId),
                Subject = SupportTask.Subject.FromPerson(options.Person)
            },
            processContext);

    public Task ResolveTrnRequestSupportTaskAsync(
        ResolveTrnRequestSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.UpdateSupportTaskAsync<TrnRequestData>(
            new UpdateSupportTaskOptions<TrnRequestData>
            {
                SupportTaskReference = options.SupportTaskReference,
                UpdateData = data => data with
                {
                    ResolvedAttributes = options.ResolvedAttributes,
                    SelectedPersonAttributes = options.SelectedPersonAttributes
                },
                Status = SupportTaskStatus.Closed,
                Comments = options.Comments
            },
            processContext);

    public Task CompleteManualChecksNeededSupportTaskAsync(
        CompleteManualChecksNeededSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.UpdateSupportTaskAsync<TrnRequestManualChecksNeededData>(
            new UpdateSupportTaskOptions<TrnRequestManualChecksNeededData>
            {
                SupportTaskReference = options.SupportTaskReference,
                UpdateData = data => data,
                Status = SupportTaskStatus.Closed
            },
            processContext);

    public async Task<TrnRequestInfo?> GetTrnRequestAsync(Guid applicationUserId, string requestId)
    {
        var result = await dbContext.TrnRequestMetadata
            .Where(m => m.ApplicationUserId == applicationUserId && m.RequestId == requestId)
            .LeftJoin(dbContext.Persons, m => m.ResolvedPersonId, p => p.PersonId, (m, p) => new { TrnRequest = m, Trn = p!.Trn })
            .SingleOrDefaultAsync();

        if (result is null)
        {
            return null;
        }

        if (await TryEnsureTrnTokenAsync(result.TrnRequest, result.Trn))
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

    public async Task<string> CreateTrnTokenAsync(string trn, string emailAddress)
    {
        var trnToken = await GenerateTrnTokenAsync();
        dbContext.AuthzRegistrationTokens.Add(new AuthzRegistrationToken
        {
            Trn = trn,
            EmailAddress = emailAddress,
            Token = trnToken,
            CreatedUtc = default,
            ExpiresUtc = default,
        });
        return trnToken;

        async Task<string> GenerateTrnTokenAsync()
        {
            string token;
            do
            {
                var buffer = new byte[8];
                RandomNumberGenerator.Fill(buffer);
                token = Convert.ToHexString(buffer).ToLower();
            } while (await dbContext.AuthzRegistrationTokens.AnyAsync(t => t.Token == token));

            return token;
        }
    }

    private async Task EnsureOneLoginUserConnectedAsync(TrnRequestMetadata trnRequest, ProcessContext processContext)
    {
        Debug.Assert(trnRequest.ResolvedPersonId.HasValue);

        if (trnRequest.OneLoginUserSubject is null || trnRequest.IdentityVerified is not true)
        {
            return;
        }

        var oneLoginUserSubject = trnRequest.OneLoginUserSubject;

        if (await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == oneLoginUserSubject) is { PersonId: null } oneLoginUser)
        {
            var personId = trnRequest.ResolvedPersonId.Value;
            var verifiedInfo = trnRequest.GetVerifiedInfo()!.Value;
            var matchRoute = OneLoginUserMatchRoute.TrnRequest;

            var matchedAttributes = await oneLoginService.GetMatchedAttributesAsync(
                new GetMatchedAttributesOptions
                {
                    PersonId = personId,
                    Names = verifiedInfo.Names,
                    DatesOfBirth = verifiedInfo.DatesOfBirth,
                    EmailAddress = trnRequest.EmailAddress
                });

            if (oneLoginUser.VerificationRoute is null)
            {
                // It's possible we have a One Login user that's been created via TeacherAuth that is currently unverified and unmatched
                // then we get a TRN Request with that same user from, say, AfQTS.
                // In that case they won't yet be marked as verified, so we handle that here.

                await oneLoginService.SetUserVerifiedAndMatchedAsync(
                    new SetUserVerifiedAndMatchedOptions
                    {
                        OneLoginUserSubject = oneLoginUserSubject,
                        VerificationRoute = OneLoginUserVerificationRoute.External,
                        VerifiedByApplicationUserId = trnRequest.ApplicationUserId,
                        VerifiedDatesOfBirth = verifiedInfo.DatesOfBirth,
                        VerifiedNames = verifiedInfo.Names,
                        CoreIdentityClaimVc = null,
                        MatchedPersonId = personId,
                        MatchRoute = matchRoute,
                        MatchedAttributes = matchedAttributes
                    },
                    processContext);
            }
            else
            {
                await oneLoginService.SetUserMatchedAsync(
                    new SetUserMatchedOptions
                    {
                        OneLoginUserSubject = oneLoginUserSubject,
                        MatchedPersonId = personId,
                        MatchRoute = matchRoute,
                        MatchedAttributes = matchedAttributes
                    },
                    processContext);
            }
        }
    }

    // internal for testing.
    // Takes the entity rather than the request's keys as it's called mid-update by the Resolve* methods and relies on
    // them to save the token it assigns.
    internal async Task<bool> TryEnsureTrnTokenAsync(TrnRequestMetadata trnRequest, string resolvedPersonTrn)
    {
        if (trnRequest.Status is not TrnRequestStatus.Completed || trnRequest.TrnToken is not null || trnRequest.EmailAddress is null)
        {
            return false;
        }

        // We've had some invalid emails creep in that the TRN token API will reject
        if (!EmailAddress.TryParse(trnRequest.EmailAddress, out _))
        {
            return false;
        }

        trnRequest.TrnToken = await CreateTrnTokenAsync(resolvedPersonTrn, trnRequest.EmailAddress);
        return true;
    }

    public async Task<MatchPersonsResult> MatchPersonsAsync(TrnRequestMetadata request, params Guid[] excludePersonIds)
    {
        request.PotentialDuplicate = false;

        // If a One Login ID is provided then ignore other matching rules and match only on that if it's associated with a teaching record
        if (!string.IsNullOrEmpty(request.OneLoginUserSubject))
        {
            var oneLoginUser = await dbContext.OneLoginUsers
                .Include(o => o.Person)
                .SingleOrDefaultAsync(u => u.Subject == request.OneLoginUserSubject);

            if (oneLoginUser?.Person is Person person)
            {
                var match = (await GetMatchesFromTrnRequestAsync(request, person.PersonId))[0];
                var matchedAttributes = GetMatchedAttributes(match);
                return MatchPersonsResult.DefiniteMatch(person.PersonId, person.Trn, matchedAttributes);
            }
        }

        var results = (await GetMatchesFromTrnRequestAsync(request)).ToList();
        results.RemoveAll(r => excludePersonIds.Contains(r.PersonId));

        if (results.Count == 0)
        {
            return MatchPersonsResult.NoMatches();
        }

        var matchedOnDobAndNino = results.Where(r => r is { DateOfBirthMatches: true, NationalInsuranceNumberMatches: true }).ToArray();

        if (matchedOnDobAndNino is [var singleDobAndNinoMatch])
        {
            var matchedAttributes = GetMatchedAttributes(singleDobAndNinoMatch);
            return MatchPersonsResult.DefiniteMatch(singleDobAndNinoMatch.PersonId, singleDobAndNinoMatch.Trn, matchedAttributes);
        }

        var matchedOnNameDateOfBirthEmailAndGender = results
            .Where(r => r is
            {
                FirstNameMatches: true,
                LastNameMatches: true,
                DateOfBirthMatches: true,
                EmailAddressMatches: true,
                GenderMatches: true
            })
            .ToArray();

        if (matchedOnNameDateOfBirthEmailAndGender is [var singleNameDobEmailGenderMatch] && string.IsNullOrEmpty(request.NationalInsuranceNumber))
        {
            var matchedAttributes = GetMatchedAttributes(singleNameDobEmailGenderMatch);
            return MatchPersonsResult.DefiniteMatch(singleNameDobEmailGenderMatch.PersonId, singleNameDobEmailGenderMatch.Trn, matchedAttributes);
        }

        request.PotentialDuplicate = true;

        return MatchPersonsResult.PotentialMatches(
            results
                .Select(r =>
                {
                    var score = (r.DateOfBirthMatches ? 1 : 0) +
                        (r.FirstNameMatches ? 1 : 0) +
                        (r.MiddleNameMatches ? 1 : 0) +
                        (r.LastNameMatches ? 1 : 0) +
                        (r.EmailAddressMatches ? 5 : 0) +
                        (r.NationalInsuranceNumberMatches ? 10 : 0);

                    var potentialMatch = GetMatchedPerson(r);

                    return (potentialMatch, score);
                })
                .OrderByDescending(r => r.score)
                // Tie-break on PersonId so equally-scored matches have a deterministic order
                .ThenBy(r => r.potentialMatch.PersonId)
                .Select(r => r.potentialMatch));
    }

    public async Task<TrnRequestInfo> ActivateTrnRequestAsync(Guid applicationUserId, string requestId, ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        if (trnRequest.Status is not TrnRequestStatus.Dormant)
        {
            throw new InvalidOperationException($"Only {TrnRequestStatus.Dormant} requests can be activated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldTrnRequestEventModel = EventModels.TrnRequestMetadata.FromModel(trnRequest);

        trnRequest.Status = TrnRequestStatus.Pending;
        await dbContext.SaveChangesAsync();

        var changes = TrnRequestUpdatedChanges.Status;

        TrnRequestInfo result;

        // If there's an outstanding support task for this request then leave resolution to that task
        if (await dbContext.SupportTasks.AnyAsync(
            t => t.TrnRequestApplicationUserId == trnRequest.ApplicationUserId &&
                t.TrnRequestId == trnRequest.RequestId &&
                t.IsOutstanding &&
                _trnRequestResolvingSupportTaskTypes.Contains(t.SupportTaskType)))
        {
            result = new TrnRequestInfo(trnRequest, ResolvedPersonTrn: null);
        }
        else
        {
            result = await TryResolveAsync(trnRequest, processContext);

            if (result.TrnRequest.ResolvedPersonId is not null)
            {
                changes |= TrnRequestUpdatedChanges.ResolvedPersonId;
            }
        }

        await eventScope.PublishEventAsync(
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = trnRequest.ApplicationUserId,
                RequestId = trnRequest.RequestId,
                Changes = changes,
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                OldTrnRequest = oldTrnRequestEventModel,
                ReasonDetails = null
            });

        return result;
    }

    private Task<TrnRequestMatchQueryResult[]> GetMatchesFromTrnRequestAsync(TrnRequestMetadata request, Guid? personId = null)
    {
        // Find all Active records with a TRN that match on:
        // person ID (if provided) *OR*
        // - at least three of first name, middle name, last name, DOB *OR*
        // - NINO *OR*
        // - email address.

        var firstNames = new[] { request.FirstName, request.PreviousFirstName };
        var middleNames = new[] { request.MiddleName, request.PreviousMiddleName };
        var lastNames = new[] { request.LastName, request.PreviousLastName };

        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        return dbContext.Database.SqlQueryRaw<TrnRequestMatchQueryResult>(
                """
                WITH vars AS (
                    SELECT
                        (fn_split_names(:first_names, include_synonyms => true) collate "case_insensitive") first_names,
                        (fn_split_names(:middle_names, include_synonyms => true) collate "case_insensitive") middle_names,
                        (fn_split_names(:last_names, include_synonyms => false) collate "case_insensitive") last_names,
                        :date_of_birth date_of_birth,
                        (:email_address COLLATE "case_insensitive") email_address,
                        array_remove(ARRAY[:national_insurance_number] COLLATE "case_insensitive", null)::varchar[] national_insurance_numbers,
                        :gender gender,
                        :person_id person_id
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
                    (vars.person_id IS NOT NULL AND p.person_id = vars.person_id) OR
                    (vars.person_id IS NULL AND p.status = 0 AND (
                        (p.names && vars.first_names AND p.names && vars.middle_names AND p.names && vars.last_names) OR
                        (p.names && vars.first_names AND p.names && vars.middle_names AND p.date_of_birth = vars.date_of_birth) OR
                        (p.names && vars.middle_names AND p.names && vars.last_names AND p.date_of_birth = vars.date_of_birth) OR
                        (p.names && vars.first_names AND p.names && vars.last_names AND p.date_of_birth = vars.date_of_birth) OR
                        (vars.email_address IS NOT NULL AND p.email_address = vars.email_address) OR
                        (array_length(vars.national_insurance_numbers, 1) > 0 AND p.national_insurance_numbers && vars.national_insurance_numbers)
                    ))
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
                    new NpgsqlParameter("gender", NpgsqlDbType.Integer) { Value = (object?)(int?)request.Gender ?? DBNull.Value },
                    new NpgsqlParameter("person_id", NpgsqlDbType.Uuid) { Value = (object?)personId ?? DBNull.Value }
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        static NpgsqlParameter CreateArrayParameter(string name, IEnumerable<string?> values) =>
            new(name, NpgsqlDbType.Array | NpgsqlDbType.Varchar) { Value = values.ToArray() };
    }

    private MatchPersonsResultPerson GetMatchedPerson(TrnRequestMatchQueryResult result) =>
        new(
            PersonId: result.PersonId,
            MatchedAttributes: GetMatchedAttributes(result)
        );

    private IReadOnlyCollection<PersonMatchedAttribute> GetMatchedAttributes(TrnRequestMatchQueryResult result) =>
        new[]
        {
            result.FirstNameMatches ? PersonMatchedAttribute.FirstName : (PersonMatchedAttribute?)null,
            result.MiddleNameMatches ? PersonMatchedAttribute.MiddleName : null,
            result.LastNameMatches ? PersonMatchedAttribute.LastName : null,
            result.DateOfBirthMatches ? PersonMatchedAttribute.DateOfBirth : null,
            result.EmailAddressMatches ? PersonMatchedAttribute.EmailAddress : null,
            result.NationalInsuranceNumberMatches ? PersonMatchedAttribute.NationalInsuranceNumber : null,
            result.GenderMatches ? PersonMatchedAttribute.Gender : null
        }
        .Where(a => a is not null)
        .Select(a => a!.Value)
        .ToArray();

    public async Task<TrnRequestInfo> TryResolveAsync(Guid applicationUserId, string requestId, ProcessContext processContext)
    {
        var trnRequest = await dbContext.TrnRequestMetadata.FindOrThrowAsync(applicationUserId, requestId);

        return await TryResolveAsync(trnRequest, processContext);
    }

    private async Task<TrnRequestInfo> TryResolveAsync(TrnRequestMetadata trnRequest, ProcessContext processContext)
    {
        string? trn = null;
        var matchResult = await MatchPersonsAsync(trnRequest);

        if (matchResult.Outcome is MatchPersonsResultOutcome.DefiniteMatch)
        {
            trn = matchResult.Trn;
            var person = (await dbContext.Persons.FindAsync(matchResult.PersonId))!;
            await ResolveTrnRequestWithMatchedPersonAsync(trnRequest, person, publishTrnRequestUpdatedEvent: false, processContext);
        }
        else if (matchResult.Outcome is MatchPersonsResultOutcome.NoMatches)
        {
            trn = await ResolveTrnRequestWithNewRecordAsync(trnRequest, publishTrnRequestUpdatedEvent: false, processContext);
        }
        else
        {
            Debug.Assert(matchResult.Outcome is MatchPersonsResultOutcome.PotentialMatches);

            await CreateTrnRequestSupportTaskAsync(
                new CreateTrnRequestSupportTaskOptions
                {
                    TrnRequest = trnRequest
                },
                processContext);
        }

        return new TrnRequestInfo(trnRequest, trn);
    }

    private record TrnRequestMatchQueryResult(
        Guid PersonId,
        string Trn,
        string FirstName,
        string? MiddleName,
        string LastName,
        DateOnly? DateOfBirth,
        string? EmailAddress,
        string? NationalInsuranceNumber,
        Gender? Gender,
        bool FirstNameMatches,
        bool MiddleNameMatches,
        bool LastNameMatches,
        bool DateOfBirthMatches,
        bool EmailAddressMatches,
        bool NationalInsuranceNumberMatches,
        bool GenderMatches);
}
