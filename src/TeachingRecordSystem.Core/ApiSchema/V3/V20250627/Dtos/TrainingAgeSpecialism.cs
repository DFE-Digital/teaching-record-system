using Optional;
using TrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record TrainingAgeSpecialism
{
    public required TrainingAgeSpecialismType Type { get; init; }
    public required Option<int> From { get; init; }
    public required Option<int> To { get; init; }
}
