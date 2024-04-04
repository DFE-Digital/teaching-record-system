namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    private static int _lastEstablishmentUrn = 100000;

    public int GenerateEstablishmentUrn() => Interlocked.Increment(ref _lastEstablishmentUrn);

    public async Task<Core.DataStore.Postgres.Models.Establishment> CreateEstablishment(
        string localAuthorityCode,
        string? localAuthorityName = null,
        string? establishmentNumber = null,
        string? establishmentName = null,
        string? postcode = null,
        int? urn = null,
        bool isHigherEducationInstitution = false,
        int establishmentStatusCode = 1)
    {
        var establishmentTypeCode = isHigherEducationInstitution ? "29" : "01";
        var establishmentTypeName = isHigherEducationInstitution ? "Higher education institutions" : "Community school";
        var establishmentTypeGroupCode = isHigherEducationInstitution ? 2 : 4;
        var establishmentTypeGroupName = isHigherEducationInstitution ? "Universities" : "Local authority maintained schools";
        localAuthorityName ??= Faker.Address.City();
        establishmentName ??= Faker.Company.Name();
        postcode ??= Faker.Address.UkPostCode();
        urn ??= GenerateEstablishmentUrn();
        var establishmentStatusName = "Open";
        switch (establishmentStatusCode)
        {
            case 1:
                establishmentStatusName = "Open";
                break;
            case 2:
                establishmentStatusName = "Closed";
                break;
            case 3:
                establishmentStatusName = "Open, but proposed to close";
                break;
            case 4:
                establishmentStatusName = "Proposed to open";
                break;
            default:
                establishmentStatusCode = 1;
                break;
        }

        var establishment = await WithDbContext(async dbContext =>
        {
            var establishment = new Core.DataStore.Postgres.Models.Establishment
            {
                EstablishmentId = Guid.NewGuid(),
                EstablishmentSourceId = 1,
                Urn = urn.Value,
                LaCode = localAuthorityCode,
                LaName = localAuthorityName,
                EstablishmentNumber = establishmentNumber,
                EstablishmentName = establishmentName,
                EstablishmentTypeCode = establishmentTypeCode,
                EstablishmentTypeName = establishmentTypeName,
                EstablishmentTypeGroupCode = establishmentTypeGroupCode,
                EstablishmentTypeGroupName = establishmentTypeGroupName,
                EstablishmentStatusCode = establishmentStatusCode,
                EstablishmentStatusName = establishmentStatusName,
                Street = null,
                Locality = null,
                Address3 = null,
                Town = null,
                County = null,
                Postcode = postcode,
            };

            dbContext.Establishments.Add(establishment);
            await dbContext.SaveChangesAsync();

            return establishment;
        });

        return establishment;
    }
}
