using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TrnRequestInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.TrnRequestInfo;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnRequestCommand(string RequestId) : ICommand<TrnRequestInfo>;

public class GetTrnRequestHandler(TrnRequestService trnRequestService, ICurrentUserProvider currentUserProvider) :
    ICommandHandler<GetTrnRequestCommand, TrnRequestInfo>
{
    public async Task<ApiResult<TrnRequestInfo>> ExecuteAsync(GetTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequestInfo = await trnRequestService.GetTrnRequestAsync(currentApplicationUserId, command.RequestId);

        if (trnRequestInfo is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        var trnRequest = trnRequestInfo.TrnRequest;
        var status = trnRequest.Status;
        var trn = status == TrnRequestStatus.Completed ? trnRequestInfo.ResolvedPersonTrn : null;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
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
            Trn = trn,
            Status = status,
            PotentialDuplicate = trnRequest.PotentialDuplicate,
            AccessYourTeachingQualificationsLink = trnRequest is { TrnToken: string trnToken, Status: TrnRequestStatus.Completed } ?
                trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) :
                null
        };
    }
}
