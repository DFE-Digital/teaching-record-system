using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetTrnRequestHandler(
    ICrmQueryDispatcher _crmQueryDispatcher,
    TrsDbContext _trsDbContext,
    ICurrentClientProvider _currentClientProvider) : IRequestHandler<GetTrnRequest, TrnRequestInfo?>
{
    public async Task<TrnRequestInfo?> Handle(GetTrnRequest request, CancellationToken cancellationToken)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();
        var trnRequest = await _trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId.ToString());

        if (trnRequest != null)
        {
            var teacher = await _crmQueryDispatcher.ExecuteQuery(
                new GetContactDetailByIdQuery(
                    trnRequest.TeacherId,
                    new ColumnSet(
                        Contact.Fields.dfeta_TRN,
                        Contact.Fields.FirstName,
                        Contact.Fields.MiddleName,
                        Contact.Fields.LastName,
                        Contact.Fields.EMailAddress1,
                        Contact.Fields.dfeta_NINumber,
                        Contact.Fields.BirthDate)));

            var status = !string.IsNullOrEmpty(teacher!.Contact.dfeta_TRN) ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;
            return new TrnRequestInfo()
            {
                RequestId = request.RequestId.ToString(),
                Person = new TrnRequestPerson()
                {
                    FirstName = teacher.Contact.FirstName,
                    LastName = teacher.Contact.LastName,
                    MiddleName = teacher.Contact.MiddleName,
                    Email = teacher.Contact.EMailAddress1,
                    NationalInsuranceNumber = teacher.Contact.dfeta_NINumber,
                    DateOfBirth = teacher.Contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                },
                Trn = teacher.Contact.dfeta_TRN,
                Status = status,
            };
        }
        return null;
    }
}
