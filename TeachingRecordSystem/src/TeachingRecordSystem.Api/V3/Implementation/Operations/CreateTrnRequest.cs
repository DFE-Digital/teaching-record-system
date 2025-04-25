#pragma warning disable TRS0001
using System.Transactions;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record CreateTrnRequestCommand
{
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required Gender? Gender { get; init; }
}

public class CreateTrnRequestHandler(
    IDbContextFactory<TrsDbContext> dbContextFactory, 
    IBackgroundJobScheduler backgroundJobScheduler,
    IPersonMatchingService personMatchingService,
    TrnRequestHelper trnRequestHelper,
    ICurrentUserProvider currentUserProvider,
    ITrnGenerator trnGenerationApiClient,
    IClock clock,
    IConfiguration configuration)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(CreateTrnRequestCommand command)
    {
        var (currentApplicationUserId, currentApplicationUserName) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is not null)
        {
            return ApiError.TrnRequestAlreadyCreated(command.RequestId);
        }

        var emailAddress = command.EmailAddresses.FirstOrDefault();
        
        var trnRequestMetadata = new PostgresModels.TrnRequestMetadata
        {
            ApplicationUserId = currentApplicationUserId,
            RequestId = command.RequestId,
            CreatedOn = clock.UtcNow,
            IdentityVerified = command.IdentityVerified,
            OneLoginUserSubject = command.OneLoginUserSubject,
            Name = GetNonEmptyValues(
                command.FirstName,
                command.MiddleName,
                command.LastName),
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = command.NationalInsuranceNumber,
            Gender = (int?)command.Gender
        };

        var matches = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);

        Guid? resolvedPersonId = null;
        string? trn = null;
        
        if (matches is [{ DefiniteMatch: true } definiteMatch])
        {
            // FUTURE: consider whether we should be updating any missing attributes here
            // TODO Consider alerts, QTS, EYTS etc.

            resolvedPersonId = definiteMatch.PersonId;
            trn = definiteMatch.Trn;
        }
        
        var potentialDuplicate = resolvedPersonId is null && matches.Count > 0;
        var createRecord = resolvedPersonId is null && !potentialDuplicate;
        var contactId = Guid.NewGuid();

        if (createRecord && !potentialDuplicate)
        {
            trn = await trnGenerationApiClient.GenerateTrnAsync();
            resolvedPersonId = contactId;
        }

        var trnToken = emailAddress is not null && trn is not null ? await trnRequestHelper.CreateTrnTokenAsync(trn, emailAddress) : null;
        var aytqLink = trnToken is not null ? trnRequestHelper.GetAccessYourTeachingQualificationsLink(trnToken) : null;
        
        trnRequestMetadata.PotentialDuplicate = potentialDuplicate;
        trnRequestMetadata.TrnToken = trnToken;
        trnRequestMetadata.ResolvedPersonId = resolvedPersonId;

        // Create a TransactionScope so that enqueued Hangfire jobs are in the transaction as our DB additions
        using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
        
        if (createRecord)
        {
            var allowContactPiiUpdatesFromUserIds = configuration.GetSection("AllowContactPiiUpdatesFromUserIds").Get<string[]>() ?? [];
            var gender = command.Gender?.ConvertToContact_GenderCode() ?? Contact_GenderCode.Notavailable;
            
            // TODO Move this into TrnRequestHelper
            await backgroundJobScheduler.EnqueueAsync<ICrmQueryDispatcher>(crmQueryDispatcher =>
                crmQueryDispatcher.ExecuteQueryAsync(new CreateContactQuery()
                {
                    ContactId = contactId,
                    FirstName = command.FirstName,
                    MiddleName = command.MiddleName ?? "",
                    LastName = command.LastName,
                    StatedFirstName = command.FirstName,
                    StatedMiddleName = command.MiddleName ?? "",
                    StatedLastName = command.LastName,
                    DateOfBirth = command.DateOfBirth,
                    Gender = gender,
                    EmailAddress = emailAddress,
                    NationalInsuranceNumber = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber),
                    ApplicationUserName = currentApplicationUserName,
                    Trn = trn,
                    TrnRequestId = TrnRequestHelper.GetCrmTrnRequestId(currentApplicationUserId, command.RequestId),
                    AllowPiiUpdates = allowContactPiiUpdatesFromUserIds.Contains(currentApplicationUserId.ToString())
                }));
        }
        else if (potentialDuplicate)
        {
            // TODO Create support task    
        }
        
        await dbContext.SaveChangesAsync();
        txn.Complete();

        var status = trn is not null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            Person = new TrnRequestInfoPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                EmailAddress = emailAddress,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
            Trn = trn,
            Status = status,
            PotentialDuplicate = potentialDuplicate,
            AccessYourTeachingQualificationsLink = aytqLink
        };

        static string[] GetNonEmptyValues(params string?[] values)
        {
            var result = new List<string>(values.Length);

            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }

            return result.ToArray();
        }
    }
}
