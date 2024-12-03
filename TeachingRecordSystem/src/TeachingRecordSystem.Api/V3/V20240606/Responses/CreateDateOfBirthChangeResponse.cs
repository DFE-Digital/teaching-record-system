using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

[AutoMap(typeof(CreateDateOfBirthChangeRequestResult))]
[GenerateVersionedDto(typeof(V20240412.Responses.CreateDateOfBirthChangeResponse))]
public partial record CreateDateOfBirthChangeResponse;
