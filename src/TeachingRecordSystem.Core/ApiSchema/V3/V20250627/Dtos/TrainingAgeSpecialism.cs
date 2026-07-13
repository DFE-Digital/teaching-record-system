using Optional;
using TrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record TrainingAgeSpecialism
{
    public required TrainingAgeSpecialismType Type { get; init; }
    public required Option<int> From { get; init; }
    public required Option<int> To { get; init; }

    public static TrainingAgeSpecialism Create(Models.TrainingAgeSpecialismType source, int? from, int? to) => new()
    {
        Type = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismTypeExtensions
            .ConvertFromTrainingAgeSpecialismType(source),
        From = from is int f ? Option.Some(f) : Option.None<int>(),
        To = to is int t ? Option.Some(t) : Option.None<int>()
    };
}
