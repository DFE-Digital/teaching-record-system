namespace TeachingRecordSystem.Core.Events.Models;

public record PersonEmployment
{
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }

    public static PersonEmployment FromModel(DataStore.Postgres.Models.PersonEmployment model) => new()
    {
        PersonEmploymentId = model.PersonEmploymentId,
        PersonId = model.PersonId,
        EstablishmentId = model.EstablishmentId,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        EmploymentType = model.EmploymentType
    };
}
