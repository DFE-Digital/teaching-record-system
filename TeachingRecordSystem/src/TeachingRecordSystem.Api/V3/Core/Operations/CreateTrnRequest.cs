using Medallion.Threading;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
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
    IDistributedLockProvider _distributedLockProvider,
    ITrnGenerationApiClient _trnGenerationApiClient)
{
    private readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    public async Task<TrnRequestInfo> Handle(CreateTrnRequestCommand command)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();

        await using var requestIdLock = await _distributedLockProvider.AcquireLockAsync(
            DistributedLockKeys.TrnRequestId(currentClientId, command.RequestId),
            _lockTimeout);

        var trnRequest = await _trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == command.RequestId);

        if (trnRequest != null)
        {
            throw new ErrorException(ErrorRegistry.CannotResubmitRequest());
        }

        string? trn = null;

        var duplicateCheckResult = await _crmQueryDispatcher.ExecuteQuery(
            new FindExistingTrnQuery(command.FirstName, command.MiddleName, command.LastName, command.DateOfBirth));

        if (duplicateCheckResult is null)
        {
            trn = await _trnGenerationApiClient.GenerateTrn();
        }

        var contactId = await _crmQueryDispatcher.ExecuteQuery(new CreateContactQuery
        {
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            Email = command.Email,
            NationalInsuranceNumber = NationalInsuranceNumberHelper.NormalizeNationalInsuranceNumber(command.NationalInsuranceNumber),
            ExistingTeacherResult = duplicateCheckResult,
            Trn = trn
        });

        // re-fetch teacher that was just created.
        var teacher = (await _crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                contactId,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.BirthDate))))!;

        trn = !string.IsNullOrEmpty(teacher!.Contact.dfeta_TRN) ? teacher.Contact.dfeta_TRN : null;
        var firstName = teacher.Contact.FirstName;
        var middleName = teacher.Contact.MiddleName ?? "";
        var lastName = teacher.Contact.LastName;
        var dob = teacher.Contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false);
        var nationalInsuranceNumber = teacher.Contact.dfeta_NINumber;
        var email = teacher.Contact.EMailAddress1;

        _trsDbContext.TrnRequests.Add(new TrnRequest()
        {
            ClientId = currentClientId,
            RequestId = command.RequestId,
            TeacherId = contactId,
            LinkedToIdentity = false
        });

        await _trsDbContext.SaveChangesAsync();

        var status = !string.IsNullOrEmpty(trn) ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            Person = new TrnRequestPerson()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                Email = email,
                DateOfBirth = dob!.Value,
                NationalInsuranceNumber = nationalInsuranceNumber
            },
            Trn = trn,
            Status = status
        };
    }
}
