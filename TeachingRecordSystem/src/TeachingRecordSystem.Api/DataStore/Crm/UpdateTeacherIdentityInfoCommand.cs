#nullable disable

namespace TeachingRecordSystem.Api.DataStore.Crm;

public class UpdateTeacherIdentityInfoCommand
{
    public Guid TeacherId { get; set; }
    public Guid IdentityUserId { get; set; }
    public string EmailAddress { get; set; }
    public string MobilePhone { get; set; }
    public DateTime UpdateTimeUtc { get; set; }
}
