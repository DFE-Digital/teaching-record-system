using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum IntegrationTransactionInterfaceType
{
    [Description("EWC Wales")]
    [Display(Name = "EWC Wales")]
    EwcWales = 1
}
