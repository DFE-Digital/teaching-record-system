using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum IntegrationTransactionInterfaceType
{
    [Description("EWC Wales")]
    [Display(Name = "EWC Wales")]
    EwcWales = 1,

    [Description("Capita Export New")]
    [Display(Name = "Capita Export New")]
    CapitaExportNew = 2,

    [Description("Capita Export Amend")]
    [Display(Name = "Capita Export Amend")]
    CapitaExportAmend = 3
}
