using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record ActivateTrnRequestCommand(string RequestId) : ICommand<ActivateTrnRequestResult>;

public record ActivateTrnRequestResult;

public class ActivateTrnRequestHandler(TrnRequestService trnRequestService, TimeProvider timeProvider, ICurrentUserProvider currentUserProvider) :
    ICommandHandler<ActivateTrnRequestCommand, ActivateTrnRequestResult>
{
    public async Task<ApiResult<ActivateTrnRequestResult>> ExecuteAsync(ActivateTrnRequestCommand command)
    {
        var currentApplicationUserId = currentUserProvider.GetCurrentApplicationUserId();

        var trnRequestInfo = await trnRequestService.GetTrnRequestAsync(currentApplicationUserId, command.RequestId);

        if (trnRequestInfo is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        if (trnRequestInfo.TrnRequest.Status is not TrnRequestStatus.Dormant)
        {
            return new ActivateTrnRequestResult();
        }

        var processContext = new ProcessContext(ProcessType.TrnRequestActivating, timeProvider.UtcNow, currentApplicationUserId);

        await trnRequestService.ActivateTrnRequestAsync(trnRequestInfo.TrnRequest, processContext);

        return new ActivateTrnRequestResult();
    }
}
