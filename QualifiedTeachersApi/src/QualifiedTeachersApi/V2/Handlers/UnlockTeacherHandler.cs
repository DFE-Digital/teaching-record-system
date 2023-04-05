#nullable disable
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V2.Handlers;

public class UnlockTeacherHandler : IRequestHandler<UnlockTeacherRequest, UnlockTeacherResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public UnlockTeacherHandler(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public async Task<UnlockTeacherResponse> Handle(UnlockTeacherRequest request, CancellationToken cancellationToken)
    {
        var contact = await _dataverseAdapter.GetTeacher(request.TeacherId, columnNames: new[]
        {
            Contact.Fields.dfeta_ActiveSanctions,
            Contact.Fields.dfeta_TRN,
            Contact.Fields.dfeta_loginfailedcounter
        });

        if (contact is null)
        {
            throw new NotFoundException(resourceName: Contact.EntityLogicalName, request.TeacherId);
        }

        if (contact.dfeta_ActiveSanctions == true)
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
