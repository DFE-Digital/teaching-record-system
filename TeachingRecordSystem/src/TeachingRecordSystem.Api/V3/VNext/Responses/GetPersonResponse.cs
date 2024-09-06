using TeachingRecordSystem.Api.V3.Core.Operations;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240606.Responses.GetPersonResponse))]
public partial record GetPersonResponse;
