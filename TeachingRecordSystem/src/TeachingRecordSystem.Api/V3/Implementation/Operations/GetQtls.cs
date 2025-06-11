using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetQtlsCommand(string Trn);

public class GetQtlsHandler(
    TrsDbContext dbContext,
    IFeatureProvider featureProvider,
    ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<ApiResult<QtlsResult>> HandleAsync(GetQtlsCommand command)
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

        var qtlsQualification = person.Qualifications!
            .OfType<RouteToProfessionalStatus>()
            .SingleOrDefault(p => p.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId);

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = qtlsQualification?.HoldsFrom
        };
    }

    private async Task<ApiResult<QtlsResult>> HandleOverDqtAsync(GetQtlsCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_qtlsdate)));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false)
        };
    }
}
