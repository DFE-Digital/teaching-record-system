namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public enum TpsCsvExtractItemResult
{
    ValidNoChange = 0,
    ValidDataAdded = 1,
    ValidDataUpdated = 2,
    InvalidTrn = 3,
    InvalidEstablishment = 4
}
