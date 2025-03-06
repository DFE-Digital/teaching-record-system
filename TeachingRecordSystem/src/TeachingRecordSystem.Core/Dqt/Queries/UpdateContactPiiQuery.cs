namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateContactPiiQuery(Guid ContactId, string? FirstName, string? MiddleName, string? LastName, DateOnly DateOfBirth, string? NationalInsuranceNumber, Contact_GenderCode? Gender, string? EmailAddress) : ICrmQuery<bool>;
