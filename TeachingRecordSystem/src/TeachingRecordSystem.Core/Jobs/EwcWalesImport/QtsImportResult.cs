namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public record QtsImportResult(int TotalCount, int SuccessCount, int DuplicateCount, int FailureCount, string FailureMessage, Guid IntegrationTransactionId);
