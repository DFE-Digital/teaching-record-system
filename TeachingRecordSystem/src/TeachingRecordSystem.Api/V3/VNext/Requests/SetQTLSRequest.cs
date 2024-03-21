using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetQTLSRequest
{
    public required DateOnly? AwardedDate { get; init; }

    [FromRoute]
    public string? Trn { get; set; }

}
