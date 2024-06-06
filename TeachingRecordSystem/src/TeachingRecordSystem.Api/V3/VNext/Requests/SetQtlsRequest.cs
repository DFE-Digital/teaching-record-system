using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetQtlsRequest
{
    public required DateOnly? QtsDate { get; init; }

    [FromRoute]
    public string? Trn { get; set; }
}
