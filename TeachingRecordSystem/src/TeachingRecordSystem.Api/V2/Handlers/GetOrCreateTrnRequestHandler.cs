#nullable disable
using FluentValidation;
using FluentValidation.Results;
using Medallion.Threading;
using MediatR;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetOrCreateTrnRequestHandler : IRequestHandler<GetOrCreateTrnRequest, TrnRequestInfo>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly TrnRequestHelper _trnRequestHelper;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDistributedLockProvider _distributedLockProvider;
    private readonly IGetAnIdentityApiClient _identityApiClient;
    private readonly AccessYourTeachingQualificationsOptions _accessYourTeachingQualificationsOptions;

    public GetOrCreateTrnRequestHandler(
        TrnRequestHelper trnRequestHelper,
        IDataverseAdapter dataverseAdapter,
        ICurrentUserProvider currentUserProvider,
        IDistributedLockProvider distributedLockProvider,
        IGetAnIdentityApiClient identityApiClient,
        IOptions<AccessYourTeachingQualificationsOptions> accessYourTeachingQualificationsOptions)
    {
        _trnRequestHelper = trnRequestHelper;
        _dataverseAdapter = dataverseAdapter;
        _currentUserProvider = currentUserProvider;
        _distributedLockProvider = distributedLockProvider;
        _identityApiClient = identityApiClient;
        _accessYourTeachingQualificationsOptions = accessYourTeachingQualificationsOptions.Value;
    }

    public async Task<TrnRequestInfo> Handle(GetOrCreateTrnRequest request, CancellationToken cancellationToken)
    {
        var (currentApplicationUserId, _) = _currentUserProvider.GetCurrentApplicationUser();

        await using var requestIdLock = await _distributedLockProvider.AcquireLockAsync(
            DistributedLockKeys.TrnRequestId(currentApplicationUserId, request.RequestId),
            _lockTimeout);

        await using var husidLock = !string.IsNullOrEmpty(request.HusId) ?
            (IAsyncDisposable)await _distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Husid(request.HusId), _lockTimeout) :
            NoopAsyncDisposable.Instance;

        var trnRequest = await _trnRequestHelper.GetTrnRequestInfoAsync(currentApplicationUserId, request.RequestId);

        bool wasCreated;
        string trn;
        DateOnly? qtsDate = null;
        string trnToken = null;

        if (trnRequest is not null)
        {
            var teacher = trnRequest.Contact;

            wasCreated = false;
            trn = teacher?.dfeta_TRN;
            qtsDate = teacher?.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true);
            trnToken = qtsDate is not null ? trnRequest.TrnToken : null;
        }
        else
        {
            var lastName = request.LastName;
            var firstAndMiddleNames = $"{request.FirstName} {request.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var firstName = firstAndMiddleNames[0];
            var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));

            var createTeacherResult = await _dataverseAdapter.CreateTeacherAsync(new CreateTeacherCommand()
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
                TeacherType = EnumHelper.ConvertToEnumByValue<Requests.CreateTeacherType, Core.Dqt.Models.CreateTeacherType>(request.TeacherType),
                RecognitionRoute = request.RecognitionRoute.HasValue ?
                    EnumHelper.ConvertToEnumByValue<Requests.CreateTeacherRecognitionRoute, TeachingRecordSystem.Core.Dqt.Models.CreateTeacherRecognitionRoute>(request.RecognitionRoute.Value) :
                    null,
                QtsDate = request.QtsDate,
                InductionRequired = request.InductionRequired,
                UnderNewOverseasRegulations = request.UnderNewOverseasRegulations,
                SlugId = request.SlugId,
                TrnRequestId = request.RequestId,
                GetTrnToken = GetTrnTokenAsync,
                ApplicationUserId = currentApplicationUserId,
                IdentityVerified = request.IdentityVerified,
                OneLoginUserSubject = request.OneLoginUserSubject
            });

            if (!createTeacherResult.Succeeded)
            {
                throw CreateValidationExceptionFromFailedReasons(createTeacherResult.FailedReasons);
            }

            wasCreated = true;
            trn = createTeacherResult.Trn;
            trnToken = createTeacherResult.TrnToken;
            qtsDate = request.QtsDate;

            async Task<string> GetTrnTokenAsync(string trn)
            {
                if (request.QtsDate is not null && trn is not null)
                {
                    var trnTokenRequest = new CreateTrnTokenRequest
                    {
                        Trn = trn,
                        Email = request.EmailAddress
                    };

                    var trnTokenResponse = await _identityApiClient.CreateTrnTokenAsync(trnTokenRequest);
                    return trnTokenResponse.TrnToken;
                }

                return null;
            }
        }

        var status = trn != null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            WasCreated = wasCreated,
            RequestId = request.RequestId,
            Trn = trn,
            Status = status,
            QtsDate = qtsDate,
            PotentialDuplicate = status == TrnRequestStatus.Pending,
            SlugId = request.SlugId,
            AccessYourTeachingQualificationsLink = trnToken is not null ? Option.Some($"{_accessYourTeachingQualificationsOptions.BaseAddress}{_accessYourTeachingQualificationsOptions.StartUrlPath}?trn_token={trnToken}") : default
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
            CreateTeacherFailedReasons.QualificationNotFound,
            $"{nameof(GetOrCreateTrnRequest.Qualification)}.{nameof(GetOrCreateTrnRequest.Qualification.HeQualificationType)}",
            ErrorRegistry.HeQualificationNotFound().Title);

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
