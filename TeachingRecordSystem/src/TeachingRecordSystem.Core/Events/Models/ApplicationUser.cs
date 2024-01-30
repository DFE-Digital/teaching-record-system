namespace TeachingRecordSystem.Core.Events.Models;

public record ApplicationUser
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string[] ApiRoles { get; set; }
    public string? OneLoginClientId { get; set; }
    public string? OneLoginPrivateKeyPem { get; set; }

    public static ApplicationUser FromModel(DataStore.Postgres.Models.ApplicationUser user) => new()
    {
        UserId = user.UserId,
        Name = user.Name,
        ApiRoles = user.ApiRoles,
        OneLoginClientId = user.OneLoginClientId,
        OneLoginPrivateKeyPem = user.OneLoginPrivateKeyPem
    };
}
