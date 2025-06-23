namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record DegreeType
{
    public required Guid DegreeTypeId { get; init; }
    public required string Name { get; init; }

    public static DegreeType? FromModel(Core.DataStore.Postgres.Models.DegreeType? model) =>
        model is null ?
            null :
            new()
            {
                DegreeTypeId = model.DegreeTypeId,
                Name = model.Name
            };
}
