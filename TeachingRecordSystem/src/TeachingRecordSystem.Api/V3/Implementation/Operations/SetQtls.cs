using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetQtlsCommand(string Trn, DateOnly? QtsDate) : ICommand<QtlsResult>;

public class SetQtlsHandler(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    ICurrentUserProvider currentUserProvider,
    IClock clock) :
    ICommandHandler<SetQtlsCommand, QtlsResult>
{
    public async Task<ApiResult<QtlsResult>> ExecuteAsync(SetQtlsCommand command)
    {
        var person = await dbContext.Persons
            .Include(p => p.Qualifications)
            .ThenInclude(q => ((PostgresModels.RouteToProfessionalStatus?)q)!.RouteToProfessionalStatusType)
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
        var currentUserId = currentUserProvider.GetCurrentApplicationUserId();

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
}

