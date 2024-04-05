namespace TeachingRecordSystem.Core.Services.Establishments.Gias;

public interface IEstablishmentMasterDataService
{
    IAsyncEnumerable<Establishment> GetEstablishments();
}
