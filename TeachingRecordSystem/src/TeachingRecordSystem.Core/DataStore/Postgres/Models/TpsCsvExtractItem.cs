namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TpsCsvExtractItem
{
    public const string TrnIndexName = "ix_tps_csv_extract_items_trn";
    public const string LaCodeEstablishmentNumberIndexName = "ix_tps_csv_extract_items_la_code_establishment_number";
    public const string TpsCsvExtractIdIndexName = "ix_tps_csv_extract_items_tps_csv_extract_id";
    public const string TpsCsvExtractForeignKeyName = "fk_tps_csv_extract_items_tps_csv_extract_id";
    public const string TpsCsvExtractLoadItemIdIndexName = "ix_tps_csv_extract_items_tps_csv_extract_load_item_id";
    public const string TpsCsvExtractLoadItemIdForeignKeyName = "fk_tps_csv_extract_items_tps_csv_extract_load_item_id";
    public const string KeyIndexName = "ix_tps_csv_extract_items_key";

    public required Guid TpsCsvExtractItemId { get; set; }
    public required Guid TpsCsvExtractId { get; set; }
    public required Guid TpsCsvExtractLoadItemId { get; set; }
    public required string Trn { get; set; }
    public required string NationalInsuranceNumber { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public required DateOnly? DateOfDeath { get; set; }
    public required string Gender { get; set; }
    public required string? MemberPostcode { get; set; }
    public required string? MemberEmailAddress { get; set; }
    public required string LocalAuthorityCode { get; set; }
    public required string? EstablishmentNumber { get; set; }
    public required string? EstablishmentPostcode { get; set; }
    public required string? EstablishmentEmailAddress { get; set; }
    public required int? MemberId { get; set; }
    public required DateOnly EmploymentStartDate { get; set; }
    public required DateOnly? EmploymentEndDate { get; set; }
    public required EmploymentType EmploymentType { get; set; }
    public required string? WithdrawlIndicator { get; set; }
    public required DateOnly ExtractDate { get; set; }
    public required DateTime Created { get; set; }
    public required TpsCsvExtractItemResult? Result { get; set; }
    public required string Key { get; set; }
}
