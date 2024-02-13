namespace TeachingRecordSystem.Core.Services.Establishments;

public interface IEstablishmentMasterDataService
{
    IAsyncEnumerable<Establishment> GetEstablishments();
}
