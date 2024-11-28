using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

[AutoMap(typeof(CreateNameChangeRequestResult))]
[GenerateVersionedDto(typeof(V20240412.Responses.CreateNameChangeResponse))]
public partial record CreateNameChangeResponse;
