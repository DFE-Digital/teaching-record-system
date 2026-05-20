using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.PublishApi;

public class PublishApiOptions
{
    [Required]
    public required string BaseAddress { get; init; }

    [Required]
    public required string RefreshTrainingProvidersJobSchedule { get; init; }
}
