using NSwag.Examples;

namespace TeachingRecordSystem.Api.V2.Responses;

public class SetIttOutcomeResponse
{
    public required string Trn { get; set; }

    public required DateOnly? QtsDate { get; set; }
}

public class SetQtsResponseExample : IExampleProvider<SetIttOutcomeResponse>
{
    public SetIttOutcomeResponse GetExample() => new()
    {
        Trn = "1234567",
        QtsDate = new DateOnly(2021, 12, 23)
    };
}
