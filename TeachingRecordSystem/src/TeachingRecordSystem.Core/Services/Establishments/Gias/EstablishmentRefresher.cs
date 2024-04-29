using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Services.Establishments.Gias;

public class EstablishmentRefresher(
    TrsDbContext dbContext,
    IEstablishmentMasterDataService establishmentMasterDataService)
{
    public async Task RefreshEstablishments(CancellationToken cancellationToken)
    {
        int i = 0;
        await foreach (var establishment in establishmentMasterDataService.GetEstablishments())
        {
            var existingEstablishment = await dbContext.Establishments.SingleOrDefaultAsync(e => e.Urn == establishment.Urn);
            if (existingEstablishment == null)
            {
                dbContext.Establishments.Add(new()
                {
                    EstablishmentId = Guid.NewGuid(),
                    EstablishmentSourceId = 1,
                    Urn = establishment.Urn,
                    LaCode = establishment.LaCode,
                    LaName = establishment.LaName,
                    EstablishmentNumber = establishment.EstablishmentNumber,
                    EstablishmentName = establishment.EstablishmentName,
                    EstablishmentTypeCode = establishment.EstablishmentTypeCode,
                    EstablishmentTypeName = establishment.EstablishmentTypeName,
                    EstablishmentTypeGroupCode = establishment.EstablishmentTypeGroupCode,
                    EstablishmentTypeGroupName = establishment.EstablishmentTypeGroupName,
                    EstablishmentStatusCode = establishment.EstablishmentStatusCode,
                    EstablishmentStatusName = establishment.EstablishmentStatusName,
                    Street = establishment.Street,
                    Locality = establishment.Locality,
                    Address3 = establishment.Address3,
                    Town = establishment.Town,
                    County = establishment.County,
                    Postcode = establishment.Postcode
                });
            }
            else
            {
                existingEstablishment.EstablishmentSourceId = 1;
                existingEstablishment.LaCode = establishment.LaCode;
                existingEstablishment.LaName = establishment.LaName;
                existingEstablishment.EstablishmentName = establishment.EstablishmentName;
                existingEstablishment.EstablishmentTypeCode = establishment.EstablishmentTypeCode;
                existingEstablishment.EstablishmentTypeName = establishment.EstablishmentTypeName;
                existingEstablishment.EstablishmentTypeGroupCode = establishment.EstablishmentTypeGroupCode;
                existingEstablishment.EstablishmentTypeGroupName = establishment.EstablishmentTypeGroupName;
                existingEstablishment.EstablishmentStatusCode = establishment.EstablishmentStatusCode;
                existingEstablishment.EstablishmentStatusName = establishment.EstablishmentStatusName;
                existingEstablishment.Street = establishment.Street;
                existingEstablishment.Locality = establishment.Locality;
                existingEstablishment.Address3 = establishment.Address3;
                existingEstablishment.Town = establishment.Town;
                existingEstablishment.County = establishment.County;
                existingEstablishment.Postcode = establishment.Postcode;
            }

            if (++i % 2000 == 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
