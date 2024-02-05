using Medallion.Threading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class CreateTrnRequestHandler(
    ICrmQueryDispatcher _crmQueryDispatcher,
    TrsDbContext _trsDbContext,
    ICurrentClientProvider _currentClientProvider,
    IDistributedLockProvider _distributedLockProvider,
    ITrnGenerationApiClient _trnGenerationApiClient) : IRequestHandler<CreateTrnRequestBody, TrnRequestInfo>
{
    private readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    public async Task<TrnRequestInfo> Handle(CreateTrnRequestBody request, CancellationToken cancellationToken)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();
        await using var requestIdLock = await _distributedLockProvider.AcquireLockAsync(
            DistributedLockKeys.TrnRequestId(currentClientId, request.RequestId),
            _lockTimeout);

        var trnRequest = await _trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId);

        string? trn = null;
        string firstName = string.Empty;
        string middleName = string.Empty;
        string lastName = string.Empty;
        DateOnly? dob = default(DateOnly?);
        string email = string.Empty;
        string ni = string.Empty;
        if (trnRequest != null)
        {
            throw new ErrorException(ErrorRegistry.CannotResubmitRequest());
        }

        var teachers = await _crmQueryDispatcher.ExecuteQuery(new FindExistingTrnQuery(request.Person.FirstName, request.Person.MiddleName, request.Person.LastName, request.Person.DateOfBirth));
        if (teachers == null)
        {
            trn = await _trnGenerationApiClient.GenerateTrn();
        }

        var contactId = await _crmQueryDispatcher.ExecuteQuery(new CreateTeacherQuery
        {
            FirstName = request.Person.FirstName,
            MiddleName = request.Person.MiddleName,
            LastName = request.Person.LastName,
            DateOfBirth = request.Person.DateOfBirth,
            Email = request.Person.Email,
            NationalInsuranceNumber = request.Person.NationalInsuranceNumber,
            ExistingTeacherResult = teachers,
            Trn = trn
        });

        //re-fetch teacher that was just created.
        var teacher = await _crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                contactId,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.BirthDate)));

        trn = !string.IsNullOrEmpty(teacher!.Contact.dfeta_TRN) ? teacher.Contact.dfeta_TRN : null;
        firstName = teacher.Contact.FirstName;
        middleName = teacher.Contact.MiddleName;
        lastName = teacher.Contact.LastName;
        dob = teacher.Contact.BirthDate.ToDateOnlyWithDqtBstFix(true);
        ni = teacher.Contact.dfeta_NINumber;
        email = teacher.Contact.EMailAddress1;

        _trsDbContext.TrnRequests.Add(new TrnRequest()
        {
            ClientId = currentClientId,
            RequestId = request.RequestId,
            TeacherId = contactId,
            LinkedToIdentity = false,
            TrnToken = trn,

        });
        await _trsDbContext.SaveChangesAsync();

        var status = !string.IsNullOrEmpty(trn) ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;
        return new TrnRequestInfo()
        {
            RequestId = request.RequestId,
            Person = new TrnRequestPerson()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                Email = email,
                DateOfBirth = dob!.Value,
                NationalInsuranceNumber = ni
            },
            Trn = trn ?? string.Empty,
            Status = status
        };

    }

}
