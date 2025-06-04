using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models
{
    public class IntegrationTransactionRecord
    {
        [Key]
        public required Int64 IntegrationTransactionRecordId { get; set; }
        public required string? RowData { get; set; }
        public required string? FailureMessage { get; set; }
        public required Person Person { get; set; }
        public required Guid PersonId { get; set; }
        public required bool? Duplicate { get; set; }
        public required IntegrationTransactionRecordStatus Status { get; set; }
        public required DateTime CreatedDate { get; set; }
        public required IntegrationTransaction IntegrationTransaction { get; set; }
        public required Int64 IntegrationTransactionId { get; set; }
    }
}
