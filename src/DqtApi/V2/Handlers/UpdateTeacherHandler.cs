using System;
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
            var teachers = (await _dataverseAdapter.GetTeachersByTrnAndDoB(request.Trn, request.BirthDate, activeOnly: true)).ToArray();
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
                switch (updateTeacherResult.FailedReasons)
                {
                    case UpdateTeacherFailedReasons.AlreadyHaveEytsDate:
                    case UpdateTeacherFailedReasons.AlreadyHaveQtsDate:
                        throw new ErrorException(ErrorRegistry.TeacherAlreadyHasQtsDate());
                    case UpdateTeacherFailedReasons.NoMatchingIttRecord:
                        throw new ErrorException(ErrorRegistry.TeacherHasNoIncompleteIttRecord());
                    case UpdateTeacherFailedReasons.CannotChangeProgrammeType:
                        throw new ErrorException(ErrorRegistry.CannotChangeProgrammeType());
                    case UpdateTeacherFailedReasons.QualificationCountryNotFound:
                        throw new ErrorException(ErrorRegistry.CountryNotFound());;
                    case UpdateTeacherFailedReasons.Subject1NotFound:
                    case UpdateTeacherFailedReasons.QualificationSubjectNotFound:
                    case UpdateTeacherFailedReasons.Subject2NotFound:
                        throw new ErrorException(ErrorRegistry.SubjectNotFound());
                    default:
                        throw new NotImplementedException($"Unknown {nameof(UpdateTeacherFailedReasons)}: '{updateTeacherResult.FailedReasons}.");
                }
            }
            return Unit.Value;
        }
    }
}
