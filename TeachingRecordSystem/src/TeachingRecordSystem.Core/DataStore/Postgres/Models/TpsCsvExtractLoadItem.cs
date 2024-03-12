namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TpsCsvExtractLoadItem
{
    public const int FieldMaxLength = 200;
    public const string TpsCsvExtractIdIndexName = "ix_tps_csv_extract_load_items_tps_csv_extract_id";
    public const string TpsCsvExtractForeignKeyName = "fk_tps_csv_extract_load_items_tps_csv_extract_id";

    public required Guid TpsCsvExtractLoadItemId { get; set; }
    public required Guid TpsCsvExtractId { get; set; }
    public required string? Trn { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required string? DateOfBirth { get; set; }
    public required string? DateOfDeath { get; set; }
    public required string? MemberPostcode { get; set; }
    public required string? MemberEmailAddress { get; set; }
    public required string? LocalAuthorityCode { get; set; }
    public required string? EstablishmentNumber { get; set; }
    public required string? EstablishmentPostcode { get; set; }
    public required string? EstablishmentEmailAddress { get; set; }
    public required string? MemberId { get; set; }
    public required string? EmploymentStartDate { get; set; }
    public required string? EmploymentEndDate { get; set; }
    public required string? FullOrPartTimeIndicator { get; set; }
    public required string? WithdrawlIndicator { get; set; }
    public required string? ExtractDate { get; set; }
    public required string? Gender { get; set; }
    public required DateTime Created { get; set; }
    public required TpsCsvExtractItemLoadErrors? Errors { get; set; }
}
