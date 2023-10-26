namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateContactNameQuery(Guid ContactId, string? FirstName, string? MiddleName, string? LastName) : ICrmQuery<bool>;
