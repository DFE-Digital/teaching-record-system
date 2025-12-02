using System.Diagnostics;
using Microsoft.Extensions.Options;
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
    IPersonMatchingService personMatchingService,
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

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequest);
        trnRequest.PotentialDuplicate = matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches;

        dbContext.TrnRequestMetadata.Add(trnRequest);

        await dbContext.SaveChangesAsync();

        string? trn = null;
        if (options.TryResolve)
        {
            if (matchResult.Outcome is TrnRequestMatchResultOutcome.DefiniteMatch)
            {
                // TODO Amend matchResult to include TRN so we don't have to re-query DB
                trn = await dbContext.Persons.Where(p => p.PersonId == matchResult.PersonId).Select(p => p.Trn!).SingleAsync();
                await CompleteTrnRequestWithMatchedPersonAsync(trnRequest, (matchResult.PersonId, trn), processContext);
            }
            else if (matchResult.Outcome is TrnRequestMatchResultOutcome.NoMatches)
            {
                trn = await CompleteTrnRequestWithNewRecordAsync(trnRequest, processContext);
            }
            else
            {
                Debug.Assert(matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches);
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

    public async Task CompleteTrnRequestWithMatchedPersonAsync(TrnRequestMetadata trnRequest, (Guid PersonId, string Trn) person, ProcessContext processContext)
    {
        var furtherChecksNeeded = await RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, trnRequest.ApplicationUserId);

        var status = furtherChecksNeeded ? TrnRequestStatus.Pending : TrnRequestStatus.Completed;
        trnRequest.SetResolvedPerson(person.PersonId, status);

        await TryEnsureTrnTokenAsync(trnRequest, person.Trn);

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
    }

    public async Task<string> CompleteTrnRequestWithNewRecordAsync(TrnRequestMetadata trnRequest, ProcessContext processContext)
    {
        var trn = await trnGenerator.GenerateTrnAsync();

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

        return trn;
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

    // TODO Make this private
    public async Task<string> CreateTrnTokenAsync(string trn, string emailAddress)
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
}
