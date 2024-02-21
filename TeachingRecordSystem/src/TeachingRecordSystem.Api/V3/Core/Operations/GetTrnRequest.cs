using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(
    ICrmQueryDispatcher _crmQueryDispatcher,
    TrsDbContext _trsDbContext,
    ICurrentClientProvider _currentClientProvider)
{
    public async Task<TrnRequestInfo?> Handle(GetTrnRequestCommand command)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();

        var trnRequest = await _trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == command.RequestId.ToString());

        if (trnRequest is null)
        {
            return null;
        }

        var (contact, parent) = (await _crmQueryDispatcher.ExecuteQuery(
            new GetContactWithParentById(
                trnRequest.TeacherId,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.BirthDate,
                    Contact.Fields.Merged,
                    Contact.Fields.MasterId))))!;

        var status = !string.IsNullOrEmpty(contact.dfeta_TRN ?? parent?.dfeta_TRN) ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId.ToString(),
            Person = new TrnRequestPerson()
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                Email = contact.EMailAddress1,
                NationalInsuranceNumber = contact.dfeta_NINumber,
                DateOfBirth = contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false)
            },
            Trn = contact.dfeta_TRN ?? parent?.dfeta_TRN,
            Status = status,
        };
    }
}
