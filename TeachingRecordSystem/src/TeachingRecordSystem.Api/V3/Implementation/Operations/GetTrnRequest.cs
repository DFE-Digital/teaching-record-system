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

        var requestData = await dbContext.TrnRequestMetadata
            .SingleOrDefaultAsync(m => m.ApplicationUserId == currentApplicationUserId && m.RequestId == command.RequestId);

        if (requestData is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        var resolvedPersonTrn = requestData.ResolvedPersonId is Guid resolvedPersonId ?
            await dbContext.Persons
                .IgnoreQueryFilters()
                .Where(p => p.PersonId == resolvedPersonId)
                .Select(p => p.Trn)
                .SingleAsync() :
            null;

        var status = requestData.Status ?? TrnRequestStatus.Pending;
        var trn = status == TrnRequestStatus.Completed ? resolvedPersonTrn : null;

        if (await trnRequestService.TryEnsureTrnTokenAsync(requestData, resolvedPersonTrn))
        {
            await dbContext.SaveChangesAsync();
        }

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
#pragma warning disable TRS0001
            Person = new TrnRequestInfoPerson()
#pragma warning restore TRS0001
            {
                FirstName = requestData.FirstName!,
                LastName = requestData.LastName!,
                MiddleName = requestData.MiddleName,
                EmailAddress = requestData.EmailAddress,
                NationalInsuranceNumber = requestData.NationalInsuranceNumber,
                DateOfBirth = requestData.DateOfBirth
            },
            Trn = trn,
            Status = status,
            PotentialDuplicate = requestData.PotentialDuplicate ?? false,
            AccessYourTeachingQualificationsLink = requestData is { TrnToken: string trnToken, Status: TrnRequestStatus.Completed } ?
                trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) :
                null
        };
    }
}
