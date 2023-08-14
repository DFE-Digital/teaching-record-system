namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public record User
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
}
