#nullable disable
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace QualifiedTeachersApi.V2.Responses;

public class SetIttOutcomeResponse
{
    [SwaggerSchema(Nullable = false)]
    public string Trn { get; set; }

    public DateOnly? QtsDate { get; set; }
}

public class SetQtsResponseExample : IExamplesProvider<SetIttOutcomeResponse>
{
    public SetIttOutcomeResponse GetExamples() => new()
    {
        Trn = "1234567",
        QtsDate = new DateOnly(2021, 12, 23)
    };
}
