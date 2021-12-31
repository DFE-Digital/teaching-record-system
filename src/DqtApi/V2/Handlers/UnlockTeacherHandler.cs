using System.Threading;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using DqtApi.V2.Requests;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class UnlockTeacherHandler : IRequestHandler<UnlockTeacherRequest>
    {
        private readonly IDataverseAdaptor _dataverseAdaptor;

        public UnlockTeacherHandler(IDataverseAdaptor dataverseAdaptor)
        {
            _dataverseAdaptor = dataverseAdaptor;
        }

        public async Task<Unit> Handle(UnlockTeacherRequest request, CancellationToken cancellationToken)
        {
            var found = await _dataverseAdaptor.UnlockTeacherRecordAsync(request.TeacherId);

            if (!found)
            {
                throw new NotFoundException(resourceName: Contact.EntityLogicalName, request.TeacherId);
            }

            return Unit.Value;
        }
    }
}
