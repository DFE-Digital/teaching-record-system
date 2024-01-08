using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

public record RaisedByUser
{
    public required User? User { get; set; }
    public required DqtUser? DqtUser { get; set; }
}
