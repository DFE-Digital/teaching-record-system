#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Infrastructure.Security;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V2.Handlers;

public class GetOrCreateTrnRequestHandler : IRequestHandler<GetOrCreateTrnRequest, TrnRequestInfo>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly DqtContext _dqtContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICurrentClientProvider _currentClientProvider;
    private readonly IDistributedLockService _distributedLockService;
    private readonly IGetAnIdentityApiClient _identityApiClient;

    public GetOrCreateTrnRequestHandler(
        DqtContext dqtContext,
        IDataverseAdapter dataverseAdapter,
        ICurrentClientProvider currentClientProvider,
        IDistributedLockService distributedLockService,
        IGetAnIdentityApiClient identityApiClient)
    {
        _dqtContext = dqtContext;
        _dataverseAdapter = dataverseAdapter;
        _currentClientProvider = currentClientProvider;
        _distributedLockService = distributedLockService;
        _identityApiClient = identityApiClient;
    }

    public async Task<TrnRequestInfo> Handle(GetOrCreateTrnRequest request, CancellationToken cancellationToken)
    {
        var currentClientId = _currentClientProvider.GetCurrentClientId();

        await using var requestIdLock = await _distributedLockService.AcquireLock(key: $"{currentClientId}:{request.RequestId}", _lockTimeout);

        await using var husidLock = !string.IsNullOrEmpty(request.HusId) ?
            await _distributedLockService.AcquireLock(request.HusId, _lockTimeout) :
            NoopAsyncDisposable.Instance;

        var trnRequest = await _dqtContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == request.RequestId);

        bool wasCreated;
        string trn;
        DateOnly? qtsDate = null;

        if (trnRequest != null)
        {
            var teacher = trnRequest.TeacherId.HasValue ?
                await _dataverseAdapter.GetTeacher(trnRequest.TeacherId.Value, columnNames: new[] { Contact.Fields.dfeta_TRN, Contact.Fields.dfeta_QTSDate }) :
                null;

            wasCreated = false;
            trn = teacher?.dfeta_TRN;
            qtsDate = teacher?.dfeta_QTSDate?.ToDateOnly();
        }
        else
        {
            var lastName = request.LastName;
            var firstAndMiddleNames = $"{request.FirstName} {request.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var firstName = firstAndMiddleNames[0];
            var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));

            if (request.IdentityUserId.HasValue)
            {
                var user = await _identityApiClient.GetUserById(request.IdentityUserId.Value);
                if (user is null)
                {
                    throw CreateValidationExceptionFromFailedReasons(CreateTeacherFailedReasons.IdentityUserNotFound);
                }
            }


            var createTeacherResult = await _dataverseAdapter.CreateTeacher(new CreateTeacherCommand()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                StatedFirstName = request.FirstName,
                StatedMiddleName = request.MiddleName,
                StatedLastName = request.LastName,
                BirthDate = request.BirthDate.ToDateTime(),
                EmailAddress = request.EmailAddress,
                Address = new CreateTeacherCommandAddress()
                {
                    AddressLine1 = request.Address?.AddressLine1,
                    AddressLine2 = request.Address?.AddressLine2,
                    AddressLine3 = request.Address?.AddressLine3,
                    City = request.Address?.City,
                    PostalCode = request.Address?.PostalCode,
                    Country = request.Address?.Country
                },
                GenderCode = request.GenderCode.ConvertToContact_GenderCode(),
                InitialTeacherTraining = new CreateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = request.InitialTeacherTraining.ProviderUkprn,
                    ProgrammeStartDate = request.InitialTeacherTraining.ProgrammeStartDate.Value,
                    ProgrammeEndDate = request.InitialTeacherTraining.ProgrammeEndDate.Value,
                    ProgrammeType = request.InitialTeacherTraining.ProgrammeType?.ConvertToIttProgrammeType(),
                    Subject1 = request.InitialTeacherTraining.Subject1,
                    Subject2 = request.InitialTeacherTraining.Subject2,
                    Subject3 = request.InitialTeacherTraining.Subject3,
                    AgeRangeFrom = request.InitialTeacherTraining.AgeRangeFrom.HasValue ? AgeRange.ConvertFromValue(request.InitialTeacherTraining.AgeRangeFrom.Value) : null,
                    AgeRangeTo = request.InitialTeacherTraining.AgeRangeTo.HasValue ? AgeRange.ConvertFromValue(request.InitialTeacherTraining.AgeRangeTo.Value) : null,
                    IttQualificationValue = request.InitialTeacherTraining.IttQualificationType?.GetIttQualificationValue(),
                    IttQualificationAim = request.InitialTeacherTraining.IttQualificationAim?.ConvertToIttQualficationAim(),
                    TrainingCountryCode = request.InitialTeacherTraining.TrainingCountryCode
                },
                Qualification = request.Qualification != null ?
                    new CreateTeacherCommandQualification()
                    {
                        ProviderUkprn = request.Qualification.ProviderUkprn,
                        CountryCode = request.Qualification.CountryCode,
                        Subject = request.Qualification.Subject,
                        Class = request.Qualification.Class?.ConvertToClassDivision(),
                        Date = request.Qualification.Date,
                        HeQualificationValue = request.Qualification.HeQualificationType?.GetHeQualificationValue(),
                        Subject2 = request.Qualification.Subject2,
                        Subject3 = request.Qualification.Subject3
                    } :
                    null,
                HusId = request.HusId,
                TeacherType = EnumHelper.ConvertToEnum<Requests.CreateTeacherType, DataStore.Crm.CreateTeacherType>(request.TeacherType),
                RecognitionRoute = request.RecognitionRoute.HasValue ?
                    EnumHelper.ConvertToEnum<Requests.CreateTeacherRecognitionRoute, DataStore.Crm.CreateTeacherRecognitionRoute>(request.RecognitionRoute.Value) :
                    null,
                QtsDate = request.QtsDate,
                InductionRequired = request.InductionRequired,
                UnderNewOverseasRegulations = request.UnderNewOverseasRegulations
            });

            if (!createTeacherResult.Succeeded)
            {
                throw CreateValidationExceptionFromFailedReasons(createTeacherResult.FailedReasons);
            }

            _dqtContext.TrnRequests.Add(new TrnRequest()
            {
                ClientId = currentClientId,
                RequestId = request.RequestId,
                TeacherId = createTeacherResult.TeacherId,
                LinkedToIdentity = false,
                IdentityUserId = request.IdentityUserId
            });

            await _dqtContext.SaveChangesAsync();

            wasCreated = true;
            trn = createTeacherResult.Trn;
            qtsDate = request.QtsDate;
        }

        var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            WasCreated = wasCreated,
            RequestId = request.RequestId,
            Trn = trn,
            Status = status,
            QtsDate = qtsDate,
            PotentialDuplicate = status == TrnRequestStatus.Pending
        };

    }

    private ValidationException CreateValidationExceptionFromFailedReasons(CreateTeacherFailedReasons failedReasons)
    {
        var failures = new List<ValidationFailure>();

        ConsumeReason(
            CreateTeacherFailedReasons.IttProviderNotFound,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.ProviderUkprn)}",
            ErrorRegistry.OrganisationNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.Subject1NotFound,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject1)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.Subject2NotFound,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject2)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.Subject3NotFound,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.Subject3)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.IttQualificationNotFound,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.IttQualificationType)}",
            ErrorRegistry.IttQualificationNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.QualificationCountryNotFound,
            $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.CountryCode)}",
            ErrorRegistry.CountryNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.QualificationSubjectNotFound,
            $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.QualificationSubject2NotFound,
            $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject2)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.QualificationSubject3NotFound,
            $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.Subject3)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.QualificationProviderNotFound,
            $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.ProviderUkprn)}",
            ErrorRegistry.OrganisationNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.TrainingCountryNotFound,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.TrainingCountryCode)}",
            ErrorRegistry.CountryNotFound().Title);

        ConsumeReason(
            CreateTeacherFailedReasons.IdentityUserNotFound,
            $"{nameof(GetOrCreateTrnRequest.IdentityUserId)}",
            ErrorRegistry.IdentityUserNotFound().Title);

        if (failedReasons != CreateTeacherFailedReasons.None)
        {
            throw new NotImplementedException($"Unknown {nameof(CreateTeacherFailedReasons)}: '{failedReasons}.");
        }

        return new ValidationException(failures);

        void ConsumeReason(CreateTeacherFailedReasons reason, string propertyName, string message)
        {
            if (failedReasons.HasFlag(reason))
            {
                failures.Add(new ValidationFailure(propertyName, message));
                failedReasons = failedReasons & ~reason;
            }
        }
    }
}
