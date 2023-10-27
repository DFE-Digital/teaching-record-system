namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateContactDateOfBirthQuery(Guid ContactId, DateOnly? DateOfBirth) : ICrmQuery<bool>;

