using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240920.Responses.GetPersonResponse), excludeMembers: ["Induction"])]
public partial record GetPersonResponse
{
    public required Option<GetPersonResponseInduction> Induction { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }
}

[AutoMap(typeof(GetPersonResultInduction))]
public partial record GetPersonResponseInduction : InductionInfo
{
    public required string? CertificateUrl { get; init; }
}
