#nullable disable
using System.ComponentModel;

namespace QualifiedTeachersApi.V2.Responses;

public class UnlockTeacherResponse
{
    [Description("Whether the account has been unlocked")]
    public bool HasBeenUnlocked { get; set; }
}
