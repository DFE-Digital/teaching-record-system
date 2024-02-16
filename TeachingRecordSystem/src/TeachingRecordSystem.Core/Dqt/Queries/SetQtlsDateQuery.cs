namespace TeachingRecordSystem.Core.Dqt.Queries;

public record SetQtlsDateQuery(Guid ContactId, DateOnly? QtlsDate, bool HasActiveSanctions, DateTime? TaskScheduleEnd) : ICrmQuery<bool>;
