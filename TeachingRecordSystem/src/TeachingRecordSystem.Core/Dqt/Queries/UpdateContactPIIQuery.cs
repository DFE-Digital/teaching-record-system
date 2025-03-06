namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateContactPIIQuery(Guid ContactId, string? FirstName, string? MiddleName, string? LastName, DateOnly DateOfBirth) : ICrmQuery<bool>;
