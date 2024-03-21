namespace TeachingRecordSystem.Core.Dqt.Queries;

public record SetQTLSDateQuery(Guid contactId, DateOnly? qtlsDate) : ICrmQuery<bool>;
