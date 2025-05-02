using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events.Models;

public record ProfessionalStatusPersonAttributes
{
    public required DateOnly? QtsDate { get; init; }
    public required DateOnly? EytsDate { get; init; }
    public required bool HasEyps { get; init; }
    public required DateOnly? PqtsDate { get; init; }

    public static ProfessionalStatusPersonAttributes FromModel(Person person) => new()
    {
        QtsDate = person.QtsDate,
        EytsDate = person.EytsDate,
        HasEyps = person.HasEyps,
        PqtsDate = person.PqtsDate
    };
}
