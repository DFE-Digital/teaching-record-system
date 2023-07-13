#nullable disable
using Medallion.Threading;
using MediatR;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Dqt;
using TeachingRecordSystem.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class SetIttOutcomeHandler : IRequestHandler<SetIttOutcomeRequest, SetIttOutcomeResponse>
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IDistributedLockProvider _distributedLockProvider;

    public SetIttOutcomeHandler(IDataverseAdapter dataverseAdapter, IDistributedLockProvider distributedLockProvider)
    {
        _dataverseAdapter = dataverseAdapter;
        _distributedLockProvider = distributedLockProvider;
    }

    public async Task<SetIttOutcomeResponse> Handle(SetIttOutcomeRequest request, CancellationToken cancellationToken)
    {
        await using var trnLock = await _distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Trn(request.Trn), _lockTimeout);

        var teachers = await GetTeacherByTrnDobOrSlugId(request.Trn, request.BirthDate, request.SlugId);

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
            request.AssessmentDate,
            request.SlugId);

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

    private async Task<Contact[]> GetTeacherByTrnDobOrSlugId(string trn, DateOnly? dob, string slugId)
    {
        if (!string.IsNullOrEmpty(slugId))
        {
            var contacts = await _dataverseAdapter.GetTeachersBySlugIdAndTrn(slugId, trn, columnNames: Array.Empty<string>(), true);

            //fallback to fetching teacher by trn/dob if slugid match doesn't return anything
            if (contacts.Count() == 0)
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
