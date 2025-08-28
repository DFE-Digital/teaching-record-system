using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Jobs;
public class CapitaImportUserOption
{
    [Required]
    public required Guid UserId { get; set; }
}
