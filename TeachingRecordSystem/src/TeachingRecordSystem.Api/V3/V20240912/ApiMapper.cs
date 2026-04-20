using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using V20240912Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240912;

[Mapper]
public partial class ApiMapper
{
    public V20240912Dtos.QtlsResponse MapQtlsResponse(QtlsResult source) =>
        new() { QtsDate = source.QtsDate, Trn = source.Trn };
}
