using Medallion.Threading;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Api.Validation;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetProfessionalStatusCommand(
    string Trn,
    string SlugId,
    Guid RouteTypeId,
    ProfessionalStatusStatus Status,
    DateOnly? AwardedDate,
    DateOnly? TrainingStartDate,
    DateOnly? TrainingEndDate,
    string[]? TrainingSubjectReferences,
    SetProfessionalStatusTrainingAgeSpecialismCommand? TrainingAgeSpecialism,
    string? TrainingCountryReference,
    string? TrainingProviderUkprn,
    Guid? DegreeTypeId);

public record SetProfessionalStatusTrainingAgeSpecialismCommand(
    TrainingAgeSpecialismType Type,
    int? From,
    int? To);

public record SetProfessionalStatusResult;

public class SetProfessionalStatusHandler(IDataverseAdapter dataverseAdapter, IDistributedLockProvider distributedLockProvider)
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(1);

    public async Task<ApiResult<SetProfessionalStatusResult>> HandleAsync(SetProfessionalStatusCommand command)
    {
        await using var trnLock = await distributedLockProvider.AcquireLockAsync(DistributedLockKeys.Trn(command.Trn), _lockTimeout);

        var teachers = await dataverseAdapter.GetTeachersBySlugIdAndTrnAsync(command.SlugId, command.Trn, columnNames: Array.Empty<string>(), true);
        if (teachers.Length == 0)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }
        else if (teachers.Length > 1)
        {
            throw new ErrorException(ErrorRegistry.MultipleTeachersFound());
        }

        return ApiError.PersonNotFound(command.Trn);
    }
}
