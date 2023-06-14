﻿#nullable disable
using MediatR;
using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Infrastructure.Security;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Handlers;

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
        var qtsDate = teacher.dfeta_QTSDate.ToDateOnly();
        var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = request.RequestId,
            Status = status,
            Trn = trn,
            QtsDate = qtsDate,
            PotentialDuplicate = status == TrnRequestStatus.Pending,
            SlugId = teacher.dfeta_SlugId
        };
    }
}
