namespace TeachingRecordSystem.Core.Dqt.Queries;

public record SetQtlsDateQuery(Guid contactId, DateOnly? qtlsDate) : ICrmQuery<bool>;
