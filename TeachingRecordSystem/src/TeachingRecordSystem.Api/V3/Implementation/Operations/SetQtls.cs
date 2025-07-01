using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetQtlsCommand(string Trn, DateOnly? QtsDate);

public class SetQtlsHandler(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    ICurrentUserProvider currentUserProvider,
    IClock clock,
    IFeatureProvider featureProvider,
    ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<ApiResult<QtlsResult>> HandleAsync(SetQtlsCommand command)
    {
        if (!featureProvider.IsEnabled(FeatureNames.RoutesToProfessionalStatus))
        {
            return await HandleOverDqtAsync(command);
        }

        var person = await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleOrDefaultAsync(p => p.Trn == command.Trn);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var qtlsRouteId = PostgresModels.RouteToProfessionalStatusType.QtlsAndSetMembershipId;
        var qtlsQualifications = person.Qualifications!.OfType<PostgresModels.RouteToProfessionalStatus>()
            .Where(p => p.RouteToProfessionalStatusTypeId == qtlsRouteId)
            .ToArray();

        if (qtlsQualifications.Length > 1)
        {
            throw new InvalidOperationException("Cannot update multiple QTLS routes.");
        }

        var existingQualification = qtlsQualifications.SingleOrDefault();
        var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        if (command.QtsDate is not null)
        {
            if (existingQualification is null)
            {
                var professionalStatus = PostgresModels.RouteToProfessionalStatus.Create(
                    person,
                    allRouteTypes: await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                    routeToProfessionalStatusTypeId: qtlsRouteId,
                    sourceApplicationUserId: null,
                    sourceApplicationReference: null,
                    status: RouteToProfessionalStatusStatus.Holds,
                    holdsFrom: command.QtsDate,
                    trainingStartDate: null,
                    trainingEndDate: null,
                    trainingSubjectIds: null,
                    trainingAgeSpecialismType: null,
                    trainingAgeSpecialismRangeFrom: null,
                    trainingAgeSpecialismRangeTo: null,
                    trainingCountryId: null,
                    trainingProviderId: null,
                    degreeTypeId: null,
                    isExemptFromInduction: true,
                    createdBy: currentUserId,
                    now: clock.UtcNow,
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    @event: out var @event);

                dbContext.Qualifications.Add(professionalStatus);
                await dbContext.AddEventAndBroadcastAsync(@event);
            }
            else
            {
                existingQualification.Update(
                    allRouteTypes: await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                    ps => ps.HoldsFrom = command.QtsDate,
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    updatedBy: currentUserId,
                    now: clock.UtcNow,
                    out var @event);

                if (@event is not null)
                {
                    await dbContext.AddEventAndBroadcastAsync(@event);
                }
            }
        }
        else if (existingQualification is not null)
        {
            existingQualification.Delete(
                allRouteTypes: await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: currentUserId,
                now: clock.UtcNow,
                out var @event);

            await dbContext.AddEventAndBroadcastAsync(@event);
        }

        await dbContext.SaveChangesAsync();

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = command.QtsDate
        };
    }

    private async Task<ApiResult<QtlsResult>> HandleOverDqtAsync(SetQtlsCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_qtlsdate,
                    Contact.Fields.dfeta_QTSDate)));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var hasActiveAlert = await dbContext.Alerts.Where(a => a.PersonId == contact.Id && a.IsOpen).AnyAsync();

        await crmQueryDispatcher.ExecuteQueryAsync(
            new SetQtlsDateQuery(contact.Id, command.QtsDate, hasActiveAlert, clock.UtcNow));

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = command.QtsDate
        };
    }
}

