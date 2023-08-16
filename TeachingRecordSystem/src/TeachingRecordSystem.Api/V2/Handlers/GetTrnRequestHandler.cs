#nullable disable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.AccessYourQualifications;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetTrnRequestHandler : IRequestHandler<GetTrnRequest, TrnRequestInfo>
{
    private readonly TrsDbContext _trsDbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICurrentClientProvider _currentClientProvider;
    private readonly AccessYourQualificationsOptions _accessYourQualificationsOptions;

    public GetTrnRequestHandler(
        TrsDbContext TrsDbContext,
        IDataverseAdapter dataverseAdapter,
        ICurrentClientProvider currentClientProvider,
        IOptions<AccessYourQualificationsOptions> accessYourQualificationsOptions)
    {
        _trsDbContext = TrsDbContext;
        _dataverseAdapter = dataverseAdapter;
        _currentClientProvider = currentClientProvider;
        _accessYourQualificationsOptions = accessYourQualificationsOptions.Value;
    }

    public async Task<TrnRequestInfo> Handle(GetTrnRequest request, CancellationToken cancellationToken)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();

        var trnRequest = await _trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId);

        if (trnRequest == null)
        {
            return null;
        }

        var teacher = await _dataverseAdapter.GetTeacher(
            trnRequest.TeacherId,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_QTSDate
            });

        if (teacher is null)
        {
            throw new Exception($"Failed retrieving contact '{trnRequest.TeacherId}' for request ID '{request.RequestId}'.");
        }

        var trn = teacher.dfeta_TRN;
        var qtsDate = teacher.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = request.RequestId,
            Status = status,
            Trn = trn,
            QtsDate = qtsDate,
            PotentialDuplicate = status == TrnRequestStatus.Pending,
            SlugId = teacher.dfeta_SlugId,
            AccessYourTeachingQualificationsLink = trnRequest.TrnToken is not null ? Option.Some($"{_accessYourQualificationsOptions.BaseAddress}/qualifications/start?trn_token={trnRequest.TrnToken}") : default
        };
    }
}
