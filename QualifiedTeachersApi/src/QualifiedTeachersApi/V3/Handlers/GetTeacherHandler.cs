using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V3.Requests;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Handlers;

public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public GetTeacherHandler(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_QTSDate
            });

        if (teacher is null)
        {
            return null;
        }

        return new GetTeacherResponse()
        {
            Trn = request.Trn,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            QtsDate = teacher.dfeta_QTSDate?.ToDateOnly()
        };
    }
}
