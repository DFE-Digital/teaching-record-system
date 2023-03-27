using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

public class UserUpdatedHandler : IRequestHandler<UserUpdatedRequest>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IDistributedLockService _distributedLockService;
    private readonly ILogger<UserUpdatedHandler> _logger;

    public UserUpdatedHandler(
        IDataverseAdapter dataverseAdapter,
        IDistributedLockService distributedLockService,
        ILogger<UserUpdatedHandler> logger)
    {
        _dataverseAdapter = dataverseAdapter;
        _distributedLockService = distributedLockService;
        _logger = logger;
    }

    public async Task<Unit> Handle(UserUpdatedRequest request, CancellationToken cancellationToken)
    {
        await using var trnLock = await _distributedLockService.AcquireLock(request.Trn, _lockTimeout);

        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_TSPersonID,
                Contact.Fields.EMailAddress1,
                Contact.Fields.MobilePhone,
                Contact.Fields.dfeta_LastIdentityUpdate
            });

        if (teacher == null)
        {
            _logger.LogWarning("No active contact record found for TRN {trn}", request.Trn);
            return Unit.Value;
        }

        if (request.UpdateTimeUtc > (teacher.dfeta_LastIdentityUpdate ?? DateTime.MinValue))
        {
            await _dataverseAdapter.UpdateTeacherIdentityInfo(new UpdateTeacherIdentityInfoCommand()
            {
                TeacherId = teacher.Id,
                IdentityUserId = request.UserId,
                EmailAddress = request.EmailAddress,
                MobilePhone = request.MobileNumber,
                UpdateTimeUtc = request.UpdateTimeUtc
            });
        }

        return Unit.Value;
    }
}
