using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Sql;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using DqtApi.Validation;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class SetIttOutcomeHandler : IRequestHandler<SetIttOutcomeRequest, SetIttOutcomeResponse>
    {
        private readonly IDataverseAdapter _dataverseAdapter;
        private readonly DqtContext _dqtContext;

        public SetIttOutcomeHandler(IDataverseAdapter dataverseAdapter, DqtContext dqtContext)
        {
            _dataverseAdapter = dataverseAdapter;
            _dqtContext = dqtContext;
        }

        public async Task<SetIttOutcomeResponse> Handle(SetIttOutcomeRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await _dqtContext.Database.BeginTransactionAsync();

            await transaction.AcquireAdvisoryLock(request.Trn);

            var teachers = (await _dataverseAdapter.GetTeachersByTrnAndDoB(request.Trn, request.BirthDate.Value, activeOnly: true)).ToArray();

            if (teachers.Length == 0)
            {
                throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
            }
            else if (teachers.Length > 1)
            {
                throw new ErrorException(ErrorRegistry.MultipleTeachersFoundWithSpecifiedTrn());
            }

            var teacherId = teachers[0].Id;
            var ittResult = request.Outcome.Value.ConvertToITTResult();

            var result = await _dataverseAdapter.SetIttResultForTeacher(
                teacherId,
                request.IttProviderUkprn,
                ittResult,
                request.AssessmentDate);

            if (!result.Succeeded)
            {
                switch (result.FailedReason)
                {
                    case SetIttResultForTeacherFailedReason.AlreadyHaveEytsDate:
                    case SetIttResultForTeacherFailedReason.AlreadyHaveQtsDate:
                        throw new ErrorException(ErrorRegistry.TeacherAlreadyHasQtsDate());
                    case SetIttResultForTeacherFailedReason.MultipleInTrainingIttRecords:
                        throw new ErrorException(ErrorRegistry.TeacherAlreadyMultipleIncompleteIttRecords());
                    case SetIttResultForTeacherFailedReason.NoMatchingIttRecord:
                        throw new ErrorException(ErrorRegistry.TeacherHasNoIncompleteIttRecord());
                    case SetIttResultForTeacherFailedReason.NoMatchingQtsRecord:
                        throw new ErrorException(ErrorRegistry.TeacherHasNoQtsRecord());
                    case SetIttResultForTeacherFailedReason.MultipleQtsRecords:
                        throw new ErrorException(ErrorRegistry.TeacherHasMultipleQtsRecords());
                    default:
                        throw new NotImplementedException($"Unknown {nameof(SetIttResultForTeacherFailedReason)}: '{result.FailedReason}.");
                }
            }

            return new SetIttOutcomeResponse()
            {
                Trn = request.Trn,
                QtsDate = result.QtsDate
            };
        }
    }
}
