namespace TeachingRecordSystem.Core.Events.Models;

public record MandatoryQualification
{
    public required Guid QualificationId { get; init; }
    public required MandatoryQualificationProvider? Provider { get; init; }
    public required MandatoryQualificationSpecialism? Specialism { get; init; }
    public required MandatoryQualificationStatus? Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }

    public static MandatoryQualification FromModel(DataStore.Postgres.Models.MandatoryQualification model, string? providerNameHint = null) => new()
    {
        QualificationId = model.QualificationId,
        Provider = model.ProviderId is Guid providerId ?
            new MandatoryQualificationProvider()
            {
                MandatoryQualificationProviderId = providerId,
                Name = providerNameHint ?? (model.Provider ?? throw new InvalidOperationException($"Missing {nameof(Provider)}.")).Name
            } :
            null,
        Specialism = model.Specialism,
        Status = model.Status,
        StartDate = model.StartDate,
        EndDate = model.EndDate
    };
}

public record MandatoryQualificationProvider
{
    public required Guid? MandatoryQualificationProviderId { get; init; }
    public required string? Name { get; init; }
    public Guid? DqtMqEstablishmentId { get; init; }
    public string? DqtMqEstablishmentName { get; init; }
}
