using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240920.Responses.GetPersonResponse), excludeMembers: ["Induction"])]
public partial record GetPersonResponse
{
    public required Option<GetPersonResponseInduction> Induction { get; init; }
}

[AutoMap(typeof(GetPersonResultInduction))]
public partial record GetPersonResponseInduction : InductionInfo
{
    public required string? CertificateUrl { get; init; }
}
