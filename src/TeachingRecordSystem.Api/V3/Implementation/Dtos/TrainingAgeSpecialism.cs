using Optional;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record TrainingAgeSpecialism
{
    public required TrainingAgeSpecialismType? Type { get; init; }
    public required Option<int> From { get; init; }
    public required Option<int> To { get; init; }
}

public static class TrainingAgeSpecialismExtensions
{
    public static TrainingAgeSpecialism? FromRoute(PostgresModels.RouteToProfessionalStatus route) =>
        route.TrainingAgeSpecialismType is not null || route.TrainingAgeSpecialismRangeFrom is not null || route.TrainingAgeSpecialismRangeTo is not null ?
            new TrainingAgeSpecialism
            {
                Type = route.TrainingAgeSpecialismType,
                From = route.TrainingAgeSpecialismRangeFrom.ToOption(),
                To = route.TrainingAgeSpecialismRangeTo.ToOption()
            } :
            null;
}
