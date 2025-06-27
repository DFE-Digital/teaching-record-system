namespace TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

public class AddWelshRMessage
{
    public required Guid PersonId { get; set; }
    public required DateOnly? AwardedDate { get; set; }
}

