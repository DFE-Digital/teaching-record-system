using TeachingRecordSystem.Api.V3.Core.Operations;

namespace TeachingRecordSystem.Api.V3.V20240416.Responses;

[AutoMap(typeof(GetPersonResult))]
[GenerateVersionedDto(typeof(V20240101.Responses.GetTeacherResponse))]
public partial record GetTeacherResponse;
