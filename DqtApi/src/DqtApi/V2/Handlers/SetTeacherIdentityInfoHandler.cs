using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using DqtApi.Validation;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class SetTeacherIdentityInfoHandler : IRequestHandler<SetTeacherIdentityInfoRequest, TeacherIdentityInfo>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public SetTeacherIdentityInfoHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<TeacherIdentityInfo> Handle(SetTeacherIdentityInfoRequest request, CancellationToken cancellationToken)
        {
            var teachersWithTrn = await _dataverseAdapter.GetTeachersByTrn(request.Trn, columnNames: new[] { Contact.Fields.dfeta_TSPersonID });

            if (teachersWithTrn.Length == 0)
            {
                throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
            }
            else if (teachersWithTrn.Length > 1)
            {
                throw new ErrorException(ErrorRegistry.MultipleTeachersFoundWithSpecifiedTrn());
            }

            var teacher = teachersWithTrn.Single();

            if (teacher.dfeta_TSPersonID != request.TsPersonId)
            {
                await _dataverseAdapter.SetTsPersonId(teacher.Id, request.TsPersonId);
            }

            return new TeacherIdentityInfo()
            {
                Trn = request.Trn,
                TsPersonId = request.TsPersonId
            };
        }
    }
}
