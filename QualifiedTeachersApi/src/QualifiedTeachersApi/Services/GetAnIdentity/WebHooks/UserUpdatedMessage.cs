﻿namespace QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

public record UserUpdatedMessage : INotificationMessage
{
    public const string MessageTypeName = "UserUpdated";

    public required User User { get; init; }
}
