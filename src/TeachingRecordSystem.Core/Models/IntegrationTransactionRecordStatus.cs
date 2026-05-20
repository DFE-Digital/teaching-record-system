using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum IntegrationTransactionRecordStatus
{
    [Description("Success")]
    [Display(Name = "Success")]
    Success = 0,

    [Description("Failure")]
    [Display(Name = "Failure")]
    Failure = 1,

    [Description("Warning")]
    [Display(Name = "Warning")]
    Warning = 2
}
