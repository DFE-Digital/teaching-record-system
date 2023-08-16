#nullable disable


namespace TeachingRecordSystem.Core.Dqt.Models;

public class UpdateTeacherIdentityInfoCommand
{
    public Guid TeacherId { get; set; }
    public Guid IdentityUserId { get; set; }
    public string EmailAddress { get; set; }
    public string MobilePhone { get; set; }
    public DateTime UpdateTimeUtc { get; set; }
}
