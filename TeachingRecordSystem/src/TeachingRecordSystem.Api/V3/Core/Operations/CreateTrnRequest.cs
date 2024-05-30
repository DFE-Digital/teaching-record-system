using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record CreateTrnRequestCommand
{
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? Email { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
}

public class CreateTrnRequestHandler(
    ICrmQueryDispatcher _crmQueryDispatcher,
    TrsDbContext _trsDbContext,
    ICurrentClientProvider _currentClientProvider,
    ITrnGenerationApiClient _trnGenerationApiClient)
{
    public async Task<TrnRequestInfo> Handle(CreateTrnRequestCommand command)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();

        var trnRequest = await _trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == command.RequestId);

        if (trnRequest != null)
        {
            throw new ErrorException(ErrorRegistry.CannotResubmitRequest());
        }

        string? trn = null;

        var potentialDuplicates = await _crmQueryDispatcher.ExecuteQuery(
            new FindPotentialDuplicateContactsQuery()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName ?? "",
                LastName = command.LastName,
                DateOfBirth = command.DateOfBirth
            });

        if (potentialDuplicates.Length == 0)
        {
            trn = await _trnGenerationApiClient.GenerateTrn();
        }

        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames.First();
        var middleName = string.Join(' ', firstAndMiddleNames.Skip(1));

        var contactId = await _crmQueryDispatcher.ExecuteQuery(new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = command.LastName,
            StatedFirstName = command.FirstName,
            StatedMiddleName = command.MiddleName ?? "",
            StatedLastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            Email = command.Email,
            NationalInsuranceNumber = NationalInsuranceNumberHelper.NormalizeNationalInsuranceNumber(command.NationalInsuranceNumber),
            PotentialDuplicates = potentialDuplicates,
            Trn = trn
        });

        _trsDbContext.TrnRequests.Add(new TrnRequest()
        {
            ClientId = currentClientId,
            RequestId = command.RequestId,
            TeacherId = contactId,
            LinkedToIdentity = false
        });

        await _trsDbContext.SaveChangesAsync();

        var status = trn is not null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            Person = new TrnRequestPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                Email = command.Email,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
            Trn = trn,
            Status = status
        };
    }
}
