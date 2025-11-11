using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Services.Something;
using Gender = TeachingRecordSystem.Core.Models.Gender;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

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
    SomethingService somethingService,
    ICurrentUserProvider currentUserProvider) :
    ICommandHandler<CreateTrnRequestCommand, TrnRequestInfo>
{
    public async Task<ApiResult<TrnRequestInfo>> ExecuteAsync(CreateTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();
        var emailAddress = command.EmailAddresses.FirstOrDefault();

        var request = new CreateTrnRequestInfo
        {
            ApplicationUserId = currentApplicationUserId,
            RequestId = command.RequestId,
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = command.NationalInsuranceNumber,
            IdentityVerified = command.IdentityVerified,
            OneLoginUserSubject = command.OneLoginUserSubject,
            Gender = command.Gender
        };

        try
        {
            var response = await somethingService.CreateTrnRequestAsync(request);

            return new TrnRequestInfo()
            {
                RequestId = command.RequestId,
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
                Trn = response.Status is TrnRequestStatus.Completed ? response.Trn : null,
                Status = response.Status,
                PotentialDuplicate = response.PotentialDuplicate,
                AccessYourTeachingQualificationsLink = response.Status is TrnRequestStatus.Completed ? response.AytqLink : null
            };
        }
        catch (TrnRequestAlreadyCreatedException ex)
        {
            return ApiError.TrnRequestAlreadyCreated(ex.RequestId);
        }
    }
}
