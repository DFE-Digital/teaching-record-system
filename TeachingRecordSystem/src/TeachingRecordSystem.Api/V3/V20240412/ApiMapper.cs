using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240412.Responses;

namespace TeachingRecordSystem.Api.V3.V20240412;

[Mapper]
public partial class ApiMapper
{
    public CreateNameChangeResponse MapCreateNameChangeResponse(CreateNameChangeRequestResult source) =>
        new() { CaseNumber = source.CaseNumber };

    public CreateDateOfBirthChangeResponse MapCreateDateOfBirthChangeResponse(CreateDateOfBirthChangeRequestResult source) =>
        new() { CaseNumber = source.CaseNumber };
}
