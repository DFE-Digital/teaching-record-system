#nullable disable
using FluentValidation;
using FluentValidation.Results;
using Medallion.Threading;
using MediatR;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class UpdateTeacherHandler : IRequestHandler<UpdateTeacherRequest>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IDistributedLockProvider _distributedLockProvider;

    public UpdateTeacherHandler(IDataverseAdapter dataverseAdapter, IDistributedLockProvider distributedLockProvider)
    {
        _dataverseAdapter = dataverseAdapter;
        _distributedLockProvider = distributedLockProvider;
    }

    public async Task Handle(UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        await using var trnLock = await _distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Trn(request.Trn), _lockTimeout);

        await using var husidLock = !string.IsNullOrEmpty(request.HusId.ValueOrDefault()) ?
            (IAsyncDisposable)await _distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Husid(request.HusId.ValueOrDefault()), _lockTimeout) :
            NoopAsyncDisposable.Instance;

        var teachers = await GetTeacherByTrnDobOrSlugId(request.Trn, request.BirthDate, request.SlugId);

        if (teachers.Length == 0)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }
        else if (teachers.Length > 1)
        {
            throw new ErrorException(ErrorRegistry.MultipleTeachersFound());
        }

        Option<string> firstName = request.FirstName;
        Option<string> middleName = request.MiddleName;

        if (firstName.HasValue)
        {
            var firstAndMiddleNames = $"{request.FirstName.ValueOrFailure()} {request.MiddleName.ValueOr("")}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            firstName = Option.Some(firstAndMiddleNames.FirstOrDefault());
            middleName = Option.Some(string.Join(" ", firstAndMiddleNames.Skip(1)));
        }

        var updateTeacherResult = await _dataverseAdapter.UpdateTeacher(new UpdateTeacherCommand()
        {
            TeacherId = teachers[0].Id,
            Trn = request.Trn,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = request.InitialTeacherTraining.ProviderUkprn,
                ProgrammeStartDate = request.InitialTeacherTraining.ProgrammeStartDate,
                ProgrammeEndDate = request.InitialTeacherTraining.ProgrammeEndDate,
                ProgrammeType = request.InitialTeacherTraining.ProgrammeType.Value.ConvertToIttProgrammeType(),
                Subject1 = request.InitialTeacherTraining.Subject1,
                Subject2 = request.InitialTeacherTraining.Subject2,
                Subject3 = request.InitialTeacherTraining.Subject3,
                AgeRangeFrom = request.InitialTeacherTraining.AgeRangeFrom.HasValue ? AgeRange.ConvertFromValue(request.InitialTeacherTraining.AgeRangeFrom.Value) : null,
                AgeRangeTo = request.InitialTeacherTraining.AgeRangeTo.HasValue ? AgeRange.ConvertFromValue(request.InitialTeacherTraining.AgeRangeTo.Value) : null,
                IttQualificationValue = request.InitialTeacherTraining.IttQualificationType?.GetIttQualificationValue(),
                IttQualificationAim = request.InitialTeacherTraining.IttQualificationAim?.ConvertToIttQualficationAim(),
                TrainingCountryCode = request.InitialTeacherTraining.TrainingCountryCode,
                Result = request.InitialTeacherTraining.Outcome?.ConvertToITTResult()
            },
            Qualification = request.Qualification != null ?
                new UpdateTeacherCommandQualification()
                {
                    ProviderUkprn = request.Qualification.ProviderUkprn,
                    CountryCode = request.Qualification.CountryCode,
                    Subject = request.Qualification.Subject,
                    Class = request.Qualification.Class?.ConvertToClassDivision(),
                    Date = request.Qualification.Date,
                    HeQualificationValue = request.Qualification.HeQualificationType?.GetHeQualificationValue(),
                    Subject2 = request.Qualification.Subject2,
                    Subject3 = request.Qualification.Subject3,
                } :
                null,
            HusId = request.HusId,
            SlugId = Option.Some(request.SlugId),
            FirstName = firstName,
            MiddleName = middleName,
            LastName = request.LastName,
            EmailAddress = request.EmailAddress,
            GenderCode = request.GenderCode.Map(x => x.ConvertToContact_GenderCode()),
            DateOfBirth = request.DateOfBirth.Map(x => x.ToDateTime()),
            StatedFirstName = request.FirstName,
            StatedMiddleName = request.MiddleName,
            StatedLastName = request.LastName
        });

        if (!updateTeacherResult.Succeeded)
        {
            throw CreateValidationExceptionFromFailedReasons(updateTeacherResult.FailedReasons);
        }

        async Task<Contact[]> GetTeacherByTrnDobOrSlugId(string trn, DateOnly? dob, string slugId)
        {
            if (!string.IsNullOrEmpty(slugId))
            {
                var contacts = await _dataverseAdapter.GetTeachersBySlugIdAndTrn(request.SlugId, trn, columnNames: Array.Empty<string>(), true);

                //fallback to fetching teacher by trn/dob if slugid match doesn't return anything
                if (contacts.Length == 0)
                {
                    contacts = (await _dataverseAdapter.GetTeachersByTrnAndDoB(trn, dob.Value, columnNames: Array.Empty<string>(), activeOnly: true)).ToArray();
                }

                return contacts;
            }
            else
            {
                return (await _dataverseAdapter.GetTeachersByTrnAndDoB(trn, dob.Value, columnNames: Array.Empty<string>(), activeOnly: true)).ToArray();
            }
        }
    }

    private ValidationException CreateValidationExceptionFromFailedReasons(UpdateTeacherFailedReasons failedReasons)
    {
        var failures = new List<ValidationFailure>();

        ConsumeReason(
            UpdateTeacherFailedReasons.Subject1NotFound,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject1)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.Subject2NotFound,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject2)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.Subject3NotFound,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject3)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.QualificationSubjectNotFound,
            $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.QualificationSubject2NotFound,
            $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject2)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.QualificationSubject3NotFound,
            $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject3)}",
            ErrorRegistry.SubjectNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.QualificationCountryNotFound,
            $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.CountryCode)}",
            ErrorRegistry.CountryNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.QualificationProviderNotFound,
            $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.ProviderUkprn)}",
            ErrorRegistry.OrganisationNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.AlreadyHaveEytsDate,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProgrammeType)}",
            ErrorRegistry.TeacherAlreadyHasQtsDate().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.AlreadyHaveQtsDate,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProgrammeType)}",
            ErrorRegistry.TeacherAlreadyHasQtsDate().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.CannotChangeProgrammeType,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProgrammeType)}",
            ErrorRegistry.CannotChangeProgrammeType().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.NoMatchingIttRecord,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProviderUkprn)}",
            ErrorRegistry.TeacherHasNoIncompleteIttRecord().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.IttProviderNotFound,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProviderUkprn)}",
            ErrorRegistry.OrganisationNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.TrainingCountryNotFound,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.TrainingCountryCode)}",
            ErrorRegistry.CountryNotFound().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.DuplicateHusId,
            $"{nameof(UpdateTeacherRequest.HusId)}.{nameof(UpdateTeacherRequest.HusId)}",
            ErrorRegistry.ExistingTeacherAlreadyHasHusId().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.InTrainingResultNotPermittedForProgrammeType,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}",
            ErrorRegistry.InTrainingResultNotPermittedForProgrammeType().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.UnderAssessmentOnlyPermittedForProgrammeType,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}",
            ErrorRegistry.UnderAssessmentOnlyPermittedForProgrammeType().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.NoMatchingQtsRecord,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}",
            ErrorRegistry.TeacherHasNoQtsRecord().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.MultipleQtsRecords,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}",
            ErrorRegistry.TeacherHasMultipleQtsRecords().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.UnableToUnwithdrawToDeferredStatus,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}",
            ErrorRegistry.UnableToUnwithdrawToDeferredStatus().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.UnableToChangeFailedResult,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}",
            ErrorRegistry.UnableToChangeFailedResult().Title);

        ConsumeReason(
            UpdateTeacherFailedReasons.MultipleInTrainingIttRecords,
            $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}",
            ErrorRegistry.TeacherAlreadyMultipleIncompleteIttRecords().Title);

        if (failedReasons != UpdateTeacherFailedReasons.None)
        {
            throw new NotImplementedException($"Unknown {nameof(UpdateTeacherFailedReasons)}: '{failedReasons}.");
        }

        return new ValidationException(failures);

        void ConsumeReason(UpdateTeacherFailedReasons reason, string propertyName, string message)
        {
            if (failedReasons.HasFlag(reason))
            {
                failures.Add(new ValidationFailure(propertyName, message));
                failedReasons = failedReasons & ~reason;
            }
        }
    }
}
