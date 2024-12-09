
using TeachingRecordSystem.Api.V3.Implementation.Dtos;

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
    public CreateTrnRequestAddress? Address { get; init; }
    public Gender? GenderCode { get; init; }
}

public class CreateTrnRequestAddress
{
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    public string? Postcode { get; init; }
    public string? Country { get; init; }
}

