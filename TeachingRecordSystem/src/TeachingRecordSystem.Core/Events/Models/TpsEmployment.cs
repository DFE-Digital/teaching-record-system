namespace TeachingRecordSystem.Core.Events.Models;

public record TpsEmployment
{
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DateOnly LastKnownTpsEmployedDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
    public required bool WithdrawalConfirmed { get; init; }
    public required DateOnly LastExtractDate { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string? PersonPostcode { get; init; }
    public required string Key { get; init; }

    public static TpsEmployment FromModel(DataStore.Postgres.Models.TpsEmployment model) => new()
    {
        PersonEmploymentId = model.TpsEmploymentId,
        PersonId = model.PersonId,
        EstablishmentId = model.EstablishmentId,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        LastKnownTpsEmployedDate = model.LastKnownTpsEmployedDate,
        EmploymentType = model.EmploymentType,
        WithdrawalConfirmed = model.WithdrawalConfirmed,
        LastExtractDate = model.LastExtractDate,
        NationalInsuranceNumber = model.NationalInsuranceNumber,
        PersonPostcode = model.PersonPostcode,
        Key = model.Key
    };
}
