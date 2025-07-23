namespace TeachingRecordSystem.Core.Services.TrnRequests;

public class TrnRequestOptions
{
    public Guid[] AllowContactPiiUpdatesFromUserIds { get; set; } = [];
    public Guid[] FlagFurtherChecksRequiredFromUserIds { get; set; } = [];
}

