using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Operations.Common;
using TeachingRecordSystem.Core.Services.TrnRequests;
using Gender = TeachingRecordSystem.Core.Models.Gender;
using TrnRequestInfo = TeachingRecordSystem.Api.V3.Operations.Common.TrnRequestInfo;

namespace TeachingRecordSystem.Api.V3.Operations;

public record CreateTrnRequestCommand : ICommand<TrnRequestInfo>
{
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required Gender? Gender { get; init; }
}

public class CreateTrnRequestHandler(
    TrnRequestService trnRequestService,
    ICurrentUserProvider currentUserProvider,
    TimeProvider timeProvider) :
    ICommandHandler<CreateTrnRequestCommand, TrnRequestInfo>
{
    public async Task<ApiResult<TrnRequestInfo>> ExecuteAsync(CreateTrnRequestCommand command)
    {
        var currentApplicationUserId = currentUserProvider.GetCurrentApplicationUserId();

        var existingRequest = await trnRequestService.GetTrnRequestAsync(currentApplicationUserId, command.RequestId);
        if (existingRequest is not null)
        {
            return ApiError.TrnRequestAlreadyCreated(command.RequestId);
        }

        var normalizedNino = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber);
        var emailAddress = command.EmailAddresses.FirstOrDefault();

        var processContext = new ProcessContext(ProcessType.TrnRequestCreating, timeProvider.UtcNow, currentApplicationUserId);

        var (trnRequest, resolvedPersonTrn) = await trnRequestService.CreateTrnRequestAsync(
            new CreateTrnRequestOptions
            {
                TryResolve = true,
                ApplicationUserId = currentApplicationUserId,
                RequestId = command.RequestId,
                OneLoginUserInfo = command.OneLoginUserSubject is not null ? new(command.OneLoginUserSubject, command.IdentityVerified ?? false) : null,
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                DateOfBirth = command.DateOfBirth,
                EmailAddress = emailAddress,
                NationalInsuranceNumber = normalizedNino,
                Gender = command.Gender
            },
            processContext);

        var trnToken = trnRequest.TrnToken;
        var aytqLink = trnToken is not null ? trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) : null;
        var status = trnRequest.Status;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            OneLoginUserSubject = trnRequest.OneLoginUserSubject,
#pragma warning disable TRS0001
            Person = new TrnRequestInfoPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                EmailAddress = emailAddress,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
#pragma warning restore TRS0001
            Trn = status is TrnRequestStatus.Completed ? resolvedPersonTrn : null,
            Status = status,
            PotentialDuplicate = trnRequest.PotentialDuplicate,
            AccessYourTeachingQualificationsLink = status is TrnRequestStatus.Completed ? aytqLink : null
        };
    }
}
