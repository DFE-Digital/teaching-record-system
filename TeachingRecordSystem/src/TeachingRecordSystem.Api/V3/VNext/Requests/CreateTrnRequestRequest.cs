
using TeachingRecordSystem.Api.V3.VNext.ApiModels;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

[GenerateVersionedDto(typeof(V20240606.Requests.CreateTrnRequestRequest), excludeMembers: "Person")]
public partial record CreateTrnRequestRequest
{
    public bool IdentityVerified { get; init; }
    public string? OneLoginUserSubject { get; init; }
    public required CreateTrnRequestRequestPerson Person { get; init; }
}

[GenerateVersionedDto(typeof(V20240606.Requests.CreateTrnRequestRequestPerson))]
public partial record CreateTrnRequestRequestPerson
{
    public CreateTrnRequestAddress? Address { get; set; }
    public Gender? GenderCode { get; set; }
}

public class CreateTrnRequestAddress
{
    public required string? AddressLine1 { get; set; }
    public required string? AddressLine2 { get; set; }
    public required string? AddressLine3 { get; set; }
    public required string? City { get; set; }
    public required string? PostalCode { get; set; }
    public required string? Country { get; set; }
}

