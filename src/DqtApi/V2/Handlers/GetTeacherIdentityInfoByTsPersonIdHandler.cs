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
    public class GetTeacherIdentityInfoByTsPersonIdHandler : IRequestHandler<GetTeacherIdentityInfoByTsPersonIdRequest, TeacherIdentityInfo>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public GetTeacherIdentityInfoByTsPersonIdHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<TeacherIdentityInfo> Handle(GetTeacherIdentityInfoByTsPersonIdRequest request, CancellationToken cancellationToken)
        {
            var teacher = await _dataverseAdapter.GetTeacherByTsPersonId(
                request.TsPersonId,
                columnNames: new[] { Contact.Fields.dfeta_TRN, Contact.Fields.dfeta_TSPersonID });

            if (teacher is null)
            {
                throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTsPersonIdNotFound());
            }

            return new TeacherIdentityInfo()
            {
                Trn = teacher.dfeta_TRN,
                TsPersonId = teacher.dfeta_TSPersonID
            };
        }
    }
}
