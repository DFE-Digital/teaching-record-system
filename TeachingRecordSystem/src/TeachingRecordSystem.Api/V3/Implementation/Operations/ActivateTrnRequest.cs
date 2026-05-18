using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record ActivateTrnRequestCommand(string RequestId) : ICommand<ActivateTrnRequestResult>;

public record ActivateTrnRequestResult(bool WasActivated, Dtos.TrnRequestInfo TrnRequestInfo);

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

        var trnRequest = trnRequestInfo.TrnRequest;
        var needsActivating = trnRequest.Status is TrnRequestStatus.Dormant;

        if (needsActivating)
        {
            var processContext = new ProcessContext(ProcessType.TrnRequestActivating, timeProvider.UtcNow, currentApplicationUserId);

            trnRequestInfo = await trnRequestService.ActivateTrnRequestAsync(trnRequest, processContext);
        }

        return new ActivateTrnRequestResult(
            WasActivated: needsActivating,
            new Dtos.TrnRequestInfo()
            {
                RequestId = command.RequestId,
                OneLoginUserSubject = trnRequest.OneLoginUserSubject,
#pragma warning disable TRS0001
                Person = new TrnRequestInfoPerson()
#pragma warning restore TRS0001
                {
                    FirstName = trnRequest.FirstName!,
                    LastName = trnRequest.LastName!,
                    MiddleName = trnRequest.MiddleName,
                    EmailAddress = trnRequest.EmailAddress,
                    NationalInsuranceNumber = trnRequest.NationalInsuranceNumber,
                    DateOfBirth = trnRequest.DateOfBirth
                },
                Trn = trnRequestInfo.ResolvedPersonTrn,
                Status = trnRequest.Status,
                PotentialDuplicate = trnRequest.PotentialDuplicate,
                AccessYourTeachingQualificationsLink = trnRequest is { TrnToken: string trnToken, Status: TrnRequestStatus.Completed } ?
                    trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) :
                    null
            });
    }
}
