namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

public record DqtUser
{
    public required Guid? UserId { get; set; }
    public required string? Name { get; set; }
}
