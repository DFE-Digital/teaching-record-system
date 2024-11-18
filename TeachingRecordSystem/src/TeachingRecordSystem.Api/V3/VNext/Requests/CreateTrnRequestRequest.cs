namespace TeachingRecordSystem.Api.V3.VNext.Requests;

[GenerateVersionedDto(typeof(V20240606.Requests.CreateTrnRequestRequest))]
public partial record CreateTrnRequestRequest
{
    public bool IdentityVerified { get; init; }
    public string? OneLoginUserSubject { get; init; }
}
