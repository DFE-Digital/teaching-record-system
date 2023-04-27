#nullable disable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V2.Handlers;

public class SetIttOutcomeHandler : IRequestHandler<SetIttOutcomeRequest, SetIttOutcomeResponse>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IDistributedLockService _distributedLockService;

    public SetIttOutcomeHandler(IDataverseAdapter dataverseAdapter, IDistributedLockService distributedLockService)
    {
        _dataverseAdapter = dataverseAdapter;
        _distributedLockService = distributedLockService;
    }

    public async Task<SetIttOutcomeResponse> Handle(SetIttOutcomeRequest request, CancellationToken cancellationToken)
    {
        await using var trnLock = await _distributedLockService.AcquireLock(request.Trn, _lockTimeout);

        var teachers = (await _dataverseAdapter.GetTeachersByTrnAndDoB(request.Trn, request.BirthDate.Value, columnNames: Array.Empty<string>(), activeOnly: true)).ToArray();

        if (teachers.Length == 0)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }
        else if (teachers.Length > 1)
        {
            throw new ErrorException(ErrorRegistry.MultipleTeachersFound());
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
                case SetIttResultForTeacherFailedReason.QtsDateMismatch:
                case SetIttResultForTeacherFailedReason.EytsDateMismatch:
                    throw new ErrorException(ErrorRegistry.TeacherAlreadyHasQtsDate());
                case SetIttResultForTeacherFailedReason.MultipleIttRecords:
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
