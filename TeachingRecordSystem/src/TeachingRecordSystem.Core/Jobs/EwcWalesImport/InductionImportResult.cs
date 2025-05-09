namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public record InductionImportResult(int TotalCount, int SuccessCount, int DuplicateCount, int FailureCount, string FailureMessage, long IntegrationTransactionId);

