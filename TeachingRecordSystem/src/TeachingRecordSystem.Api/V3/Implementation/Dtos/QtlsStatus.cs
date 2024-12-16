using System.ComponentModel;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public enum QtlsStatus
{
    [Description("None")]
    None = 0,

    [Description("Expired")]
    Expired = 1,

    [Description("Active")]
    Active = 2,
}
