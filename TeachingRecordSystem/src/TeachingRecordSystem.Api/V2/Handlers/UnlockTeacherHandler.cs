#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class UnlockTeacherHandler : IRequestHandler<UnlockTeacherRequest, UnlockTeacherResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly TrsDbContext _dbContext;

    public UnlockTeacherHandler(IDataverseAdapter dataverseAdapter, TrsDbContext dbContext)
    {
        _dataverseAdapter = dataverseAdapter;
        _dbContext = dbContext;
    }

    public async Task<UnlockTeacherResponse> Handle(UnlockTeacherRequest request, CancellationToken cancellationToken)
    {
        var contact = await _dataverseAdapter.GetTeacher(request.TeacherId, columnNames: new[]
        {
            Contact.Fields.dfeta_TRN,
            Contact.Fields.dfeta_loginfailedcounter
        });

        if (contact is null)
        {
            throw new NotFoundException(resourceName: Contact.EntityLogicalName, request.TeacherId);
        }

        var hasActiveAlert = await _dbContext.Alerts.Where(a => a.PersonId == request.TeacherId && a.IsOpen).AnyAsync();

        if (hasActiveAlert)
        {
            throw new ErrorException(ErrorRegistry.TeacherHasActiveSanctions());
        }

        if (contact.dfeta_loginfailedcounter is null || contact.dfeta_loginfailedcounter < 3)
        {
            return new UnlockTeacherResponse()
            {
                HasBeenUnlocked = false
            };
        }

        await _dataverseAdapter.UnlockTeacherRecord(request.TeacherId);

        return new UnlockTeacherResponse()
        {
            HasBeenUnlocked = true
        };
    }
}
