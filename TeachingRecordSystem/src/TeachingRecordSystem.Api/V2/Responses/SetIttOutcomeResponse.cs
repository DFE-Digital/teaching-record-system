namespace TeachingRecordSystem.Api.V2.Responses;

public class SetIttOutcomeResponse
{
    public required string Trn { get; set; }

    public required DateOnly? QtsDate { get; set; }
}
