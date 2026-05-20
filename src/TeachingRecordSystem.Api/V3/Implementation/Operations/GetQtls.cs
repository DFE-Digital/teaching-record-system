using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetQtlsCommand(string Trn) : ICommand<QtlsResult>;

public class GetQtlsHandler(TrsDbContext dbContext) : ICommandHandler<GetQtlsCommand, QtlsResult>
{
    public async Task<ApiResult<QtlsResult>> ExecuteAsync(GetQtlsCommand command)
    {
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
}
