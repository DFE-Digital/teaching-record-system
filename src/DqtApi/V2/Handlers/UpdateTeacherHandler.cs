using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql;
using DqtApi.Security;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using DqtApi.Validation;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class UpdateTeacherHandler : IRequestHandler<UpdateTeacherRequest>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public UpdateTeacherHandler(
            IDataverseAdapter dataverseAdapter,
            ICurrentClientProvider currentClientProvider)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<Unit> Handle(UpdateTeacherRequest request, CancellationToken cancellationToken)
        {
            var teachers = (await _dataverseAdapter.GetTeachersByTrnAndDoB(request.Trn, request.BirthDate.Value, activeOnly: true)).ToArray();
            if (teachers.Length == 0)
            {
                throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
            }
            else if (teachers.Length > 1)
            {
                throw new ErrorException(ErrorRegistry.MultipleTeachersFoundWithSpecifiedTrn());
            }

            var updateTeacherResult = await _dataverseAdapter.UpdateTeacher(new UpdateTeacherCommand()
            {
                TeacherId = teachers[0].Id.ToString(),
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
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    ProviderUkprn = request.Qualification.ProviderUkprn,
                    CountryCode = request.Qualification.CountryCode,
                    Subject = request.Qualification.Subject,
                    Class = request.Qualification.Class?.ConvertToClassDivision(),
                    Date = request.Qualification.Date
                }
            });

            if (!updateTeacherResult.Succeeded)
            {
                throw CreateValidationExceptionFromFailedReasons(updateTeacherResult.FailedReasons);
            }
            return Unit.Value;
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
}
