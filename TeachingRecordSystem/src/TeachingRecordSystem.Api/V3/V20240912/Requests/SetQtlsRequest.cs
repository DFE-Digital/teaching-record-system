namespace TeachingRecordSystem.Api.V3.V20240912.Requests;

public record SetQtlsRequest
{
    public required DateOnly? QtsDate { get; init; }
}
