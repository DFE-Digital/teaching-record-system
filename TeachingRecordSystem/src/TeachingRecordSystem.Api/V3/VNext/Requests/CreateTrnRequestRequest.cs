namespace TeachingRecordSystem.Api.V3.VNext.Requests;

[GenerateVersionedDto(typeof(V20240606.Requests.CreateTrnRequestRequest))]
public partial record CreateTrnRequestRequest
{
    public string? VerifiedOneLoginUserSubject { get; init; }
}
