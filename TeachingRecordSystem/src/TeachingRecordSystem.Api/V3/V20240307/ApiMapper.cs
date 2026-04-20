using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using V20240307Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240307;

[Mapper]
public partial class ApiMapper
{
    public V20240307Dtos.TrnRequestInfo MapTrnRequestInfo(TrnRequestInfo source) =>
        new()
        {
            RequestId = source.RequestId,
            Person = MapTrnRequestPerson(source.Person),
            Status = (V20240307Dtos.TrnRequestStatus)(int)source.Status,
            Trn = source.Trn
        };

    private V20240307Dtos.TrnRequestPerson MapTrnRequestPerson(TrnRequestInfoPerson source) =>
        new()
        {
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            DateOfBirth = source.DateOfBirth,
            Email = source.EmailAddress,
            NationalInsuranceNumber = source.NationalInsuranceNumber
        };
}
