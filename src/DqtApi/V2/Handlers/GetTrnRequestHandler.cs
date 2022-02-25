using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql;
using DqtApi.Security;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DqtApi.V2.Handlers
{
    public class GetTrnRequestHandler : IRequestHandler<GetTrnRequest, TrnRequestInfo>
    {
        private readonly DqtContext _dqtContext;
        private readonly IDataverseAdapter _dataverseAdapter;
        private readonly ICurrentClientProvider _currentClientProvider;

        public GetTrnRequestHandler(
            DqtContext dqtContext,
            IDataverseAdapter dataverseAdapter,
            ICurrentClientProvider currentClientProvider)
        {
            _dqtContext = dqtContext;
            _dataverseAdapter = dataverseAdapter;
            _currentClientProvider = currentClientProvider;
        }

        public async Task<TrnRequestInfo> Handle(GetTrnRequest request, CancellationToken cancellationToken)
        {
            var currentClientId = _currentClientProvider.GetCurrentClientId();

            var trnRequest = await _dqtContext.TrnRequests
                .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId);

            if (trnRequest == null)
            {
                return null;
            }

            string trn = null;

            if (trnRequest.TeacherId.HasValue)
            {
                var teacher = await _dataverseAdapter.GetTeacher(trnRequest.TeacherId.Value, columnNames: Contact.Fields.dfeta_TRN);
                trn = teacher.dfeta_TRN;
            }

            var status = trnRequest.TeacherId.HasValue ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

            return new TrnRequestInfo()
            {
                RequestId = request.RequestId,
                Status = status,
                Trn = trn
            };
        }
    }
}
