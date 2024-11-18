#nullable disable
using MediatR;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetTrnRequestHandler : IRequestHandler<GetTrnRequest, TrnRequestInfo>
{
    private readonly TrnRequestHelper _trnRequestHelper;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly AccessYourTeachingQualificationsOptions _accessYourTeachingQualificationsOptions;

    public GetTrnRequestHandler(
        TrnRequestHelper trnRequestHelper,
        TrsDbContext TrsDbContext,
        IDataverseAdapter dataverseAdapter,
        ICurrentUserProvider currentUserProvider,
        IOptions<AccessYourTeachingQualificationsOptions> accessYourTeachingQualificationsOptions)
    {
        _trnRequestHelper = trnRequestHelper;
        _dataverseAdapter = dataverseAdapter;
        _currentUserProvider = currentUserProvider;
        _accessYourTeachingQualificationsOptions = accessYourTeachingQualificationsOptions.Value;
    }

    public async Task<TrnRequestInfo> Handle(GetTrnRequest request, CancellationToken cancellationToken)
    {
        var (currentApplicationUserId, _) = _currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await _trnRequestHelper.GetTrnRequestInfo(currentApplicationUserId, request.RequestId);
        if (trnRequest == null)
        {
            return null;
        }

        var contact = trnRequest.Contact;

        var trn = contact.dfeta_TRN;
        var qtsDate = contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = request.RequestId,
            Status = status,
            Trn = trn,
            QtsDate = qtsDate,
            PotentialDuplicate = status == TrnRequestStatus.Pending,
            SlugId = contact.dfeta_SlugId,
            AccessYourTeachingQualificationsLink = trnRequest.TrnToken is not null ?
                Option.Some($"{_accessYourTeachingQualificationsOptions.BaseAddress}{_accessYourTeachingQualificationsOptions.StartUrlPath}?trn_token={trnRequest.TrnToken}") :
                default
        };
    }
}
