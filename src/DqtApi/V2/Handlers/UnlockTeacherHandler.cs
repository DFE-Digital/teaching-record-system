using System;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.V2.Requests;
using DqtApi.Validation;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class UnlockTeacherHandler : IRequestHandler<UnlockTeacherRequest>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public UnlockTeacherHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<Unit> Handle(UnlockTeacherRequest request, CancellationToken cancellationToken)
        {
            var contact = await _dataverseAdapter.GetTeacher(request.TeacherId, columnNames: new[]
            {
                Contact.Fields.dfeta_ActiveSanctions,
                Contact.Fields.dfeta_TRN
            });
            if (contact?.dfeta_ActiveSanctions == true)
            {
                throw new ErrorException(ErrorRegistry.TeacherHasActiveSanctions());
            }

            var found = await _dataverseAdapter.UnlockTeacherRecord(request.TeacherId);
            if (!found)
            {
                throw new NotFoundException(resourceName: Contact.EntityLogicalName, request.TeacherId);
            }

            return Unit.Value;
        }
    }
}
