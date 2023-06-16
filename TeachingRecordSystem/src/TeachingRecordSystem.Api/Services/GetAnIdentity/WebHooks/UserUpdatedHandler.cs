using Medallion.Threading;
using MediatR;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;

namespace TeachingRecordSystem.Api.Services.GetAnIdentity.WebHooks;

public class UserUpdatedHandler : IRequestHandler<UserUpdatedRequest>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly ILogger<UserUpdatedHandler> _logger;

    public UserUpdatedHandler(
        IDataverseAdapter dataverseAdapter,
        IDistributedLockProvider distributedLockProvider,
        ILogger<UserUpdatedHandler> logger)
    {
        _dataverseAdapter = dataverseAdapter;
        _distributedLockProvider = distributedLockProvider;
        _logger = logger;
    }

    public async Task Handle(UserUpdatedRequest request, CancellationToken cancellationToken)
    {
        if (request.Trn is null)
        {
            return;
        }

        await using var trnLock = await _distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Trn(request.Trn), _lockTimeout);

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
            return;
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
    }
}
