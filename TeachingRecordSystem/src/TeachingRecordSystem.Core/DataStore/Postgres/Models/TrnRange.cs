namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRange
{
    public int FromTrn { get; set; }
    public int ToTrn { get; set; }
    public int NextTrn { get; set; }
    public bool IsExhausted { get; set; }
}
