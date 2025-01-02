using System.Diagnostics;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetCpdInductionStatusCommand(
    string Trn,
    InductionStatus Status,
    DateOnly? StartDate,
    DateOnly? CompletedDate,
    DateTime CpdModifiedOn);

public record SetCpdInductionStatusResult;

public class SetCpdInductionStatusHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsDataSyncHelper syncHelper,
    ICurrentUserProvider currentUserProvider,
    IClock clock)
{
    public async Task<ApiResult<SetCpdInductionStatusResult>> HandleAsync(SetCpdInductionStatusCommand command)
    {
        if (command.Status is not InductionStatus.RequiredToComplete and not InductionStatus.InProgress
            and not InductionStatus.Passed and not InductionStatus.Failed)
        {
            return ApiError.InvalidInductionStatus(command.Status);
        }

        if (command.Status.RequiresStartDate() && command.StartDate is null)
        {
            return ApiError.InductionStartDateIsRequired(command.Status);
        }

        if (!command.Status.RequiresStartDate() && command.StartDate is not null)
        {
            return ApiError.InductionStartDateIsNotPermitted(command.Status);
        }

        if (command.Status.RequiresCompletedDate() && command.CompletedDate is null)
        {
            return ApiError.InductionCompletedDateIsRequired(command.Status);
        }

        if (!command.Status.RequiresCompletedDate() && command.CompletedDate is not null)
        {
            return ApiError.InductionCompletedDateIsNotPermitted(command.Status);
        }

        var dqtContact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(command.Trn, new ColumnSet(Contact.Fields.dfeta_QTSDate)));

        if (dqtContact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        if (dqtContact.dfeta_QTSDate is null)
        {
            return ApiError.PersonDoesNotHaveQts(command.Trn);
        }

        await using var txn = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var person = await GetPersonAsync();

        if (person is null)
        {
            // The person record hasn't synced to TRS yet - force that to happen so we can assign induction status
            var synced = await syncHelper.SyncPersonAsync(dqtContact.Id, syncAudit: true);
            if (!synced)
            {
                throw new Exception($"Could not sync Person with contact ID: '{dqtContact.Id}'.");
            }

            person = await GetPersonAsync();
            Debug.Assert(person is not null);
        }

        if (person.CpdInductionCpdModifiedOn is DateTime cpdInductionCpdModifiedOn &&
            command.CpdModifiedOn < cpdInductionCpdModifiedOn)
        {
            return ApiError.StaleRequest(cpdInductionCpdModifiedOn);
        }

        var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        person.SetCpdInductionStatus(
            command.Status,
            command.StartDate,
            command.CompletedDate,
            command.CpdModifiedOn,
            currentUserId,
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
        }

        await dbContext.SaveChangesAsync();
        await txn.CommitAsync();

        return new SetCpdInductionStatusResult();

        Task<Person?> GetPersonAsync() => dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn);
    }
}
