using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(TrsDbContext dbContext, TrnRequestService trnRequestService, ICurrentUserProvider currentUserProvider)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(GetTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestService.GetTrnRequestInfoAsync(dbContext, currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        var metadata = trnRequest.Metadata;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
#pragma warning disable TRS0001
            Person = new TrnRequestInfoPerson()
#pragma warning restore TRS0001
            {
                FirstName = metadata.FirstName!,
                LastName = metadata.LastName!,
                MiddleName = metadata.MiddleName,
                EmailAddress = metadata.EmailAddress,
                NationalInsuranceNumber = metadata.NationalInsuranceNumber,
                DateOfBirth = metadata.DateOfBirth
            },
            Trn = trnRequest.ResolvedPersonTrn,
            Status = metadata.Status ?? TrnRequestStatus.Pending,
            PotentialDuplicate = metadata.PotentialDuplicate ?? false,
            AccessYourTeachingQualificationsLink = metadata is { TrnToken: string trnToken, Status: TrnRequestStatus.Completed } ?
                trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) :
                null
        };
    }
}
