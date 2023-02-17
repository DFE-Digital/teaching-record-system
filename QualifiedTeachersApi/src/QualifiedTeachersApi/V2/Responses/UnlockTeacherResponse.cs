﻿using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Responses;

public class UnlockTeacherResponse
{
    [SwaggerSchema(description: "Whether the account has been unlocked")]
    public bool HasBeenUnlocked { get; set; }
}
