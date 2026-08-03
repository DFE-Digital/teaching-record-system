using Optional;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Operations.Common;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.Api.V3.Operations;

public record SetQtlsCommand(string Trn, DateOnly? QtsDate) : ICommand<QtlsResult>;

public class SetQtlsHandler(
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    RoutesToProfessionalStatusService routesToProfessionalStatusService) :
    ICommandHandler<SetQtlsCommand, QtlsResult>
{
    private static readonly DateOnly QtsCutoff = new(2012, 4, 1);

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
        DateOnly? adjustedQtsDate = command.QtsDate is not null && command.QtsDate < QtsCutoff
            ? QtsCutoff
            : command.QtsDate;

        if (command.QtsDate is not null)
        {
            if (existingQualification is null)
            {
                await routesToProfessionalStatusService.CreateRouteToProfessionalStatusAsync(
                    new CreateRouteToProfessionalStatusOptions
                    {
                        PersonId = person.PersonId,
                        RouteToProfessionalStatusTypeId = qtlsRouteId,
                        Status = RouteToProfessionalStatusStatus.Holds,
                        CreatedBy = currentUserId,
                        HoldsFrom = adjustedQtsDate,
                        IsExemptFromInduction = true
                    });
            }
            else
            {
                await routesToProfessionalStatusService.UpdateRouteToProfessionalStatusAsync(
                    new UpdateRouteToProfessionalStatusOptions
                    {
                        QualificationId = existingQualification.QualificationId,
                        UpdatedBy = currentUserId,
                        HoldsFrom = Option.Some(adjustedQtsDate)
                    });
            }
        }
        else if (existingQualification is not null)
        {
            await routesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
                new DeleteRouteToProfessionalStatusOptions
                {
                    QualificationId = existingQualification.QualificationId,
                    DeletedBy = currentUserId
                });
        }

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = command.QtsDate
        };
    }
}
