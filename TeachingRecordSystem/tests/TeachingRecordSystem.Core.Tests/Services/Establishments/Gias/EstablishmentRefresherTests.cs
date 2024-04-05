using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Establishments.Gias;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using Establishment = TeachingRecordSystem.Core.Models.Establishment;

namespace TeachingRecordSystem.Core.Tests.Services.Establishments.Gias;

public class EstablishmentRefresherTests
{
    public EstablishmentRefresherTests(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        DbFixture = dbFixture;
        Clock = new();

        var dbContextFactory = dbFixture.GetDbContextFactory();

        Helper = new TrsDataSyncHelper(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
    }

    [Fact]
    public Task RefreshEstablishments_WhenCalledforNewUrn_AddsNewEstablishments() =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var establishmentMasterDataService = Mock.Of<IEstablishmentMasterDataService>();
            var establishment1 = new Establishment
            {
                Urn = TestData.GenerateEstablishmentUrn(),
                LaCode = "123",
                LaName = "Test LA",
                EstablishmentNumber = "1234",
                EstablishmentName = "Test School",
                EstablishmentTypeCode = "01",
                EstablishmentTypeName = "Primary",
                EstablishmentTypeGroupCode = 1,
                EstablishmentTypeGroupName = "Academy",
                EstablishmentStatusCode = 1,
                EstablishmentStatusName = "Open",
                Street = "Test Street",
                Locality = "Test Locality",
                Address3 = "Test Address3",
                Town = "Test Town",
                County = "Test County",
                Postcode = "TE1 1ST"
            };
            var establishment2 = new Establishment
            {
                Urn = TestData.GenerateEstablishmentUrn(),
                LaCode = "123",
                LaName = "Test LA",
                EstablishmentNumber = "1235",
                EstablishmentName = "Test School 2",
                EstablishmentTypeCode = "02",
                EstablishmentTypeName = "Secondary",
                EstablishmentTypeGroupCode = 2,
                EstablishmentTypeGroupName = "Academy",
                EstablishmentStatusCode = 2,
                EstablishmentStatusName = "Closed",
                Street = "Test Street 2",
                Locality = "Test Locality 2",
                Address3 = "Test Address3 2",
                Town = "Test Town 2",
                County = "Test County 2",
                Postcode = "TE1 2ST"
            };

            var establishments = new List<Establishment> { establishment1, establishment2 };
            Mock.Get(establishmentMasterDataService)
                .Setup(s => s.GetEstablishments())
                .Returns(establishments.ToAsyncEnumerable());

            var establishmentRefresher = new EstablishmentRefresher(
                dbContext,
                establishmentMasterDataService);

            // Act
            await establishmentRefresher.RefreshEstablishments(CancellationToken.None);

            // Assert
            var establishmentsActual = await dbContext.Establishments.Where(e => e.Urn == establishment1.Urn || e.Urn == establishment2.Urn).OrderBy(e => e.Urn).ToListAsync();
            Assert.Collection(establishmentsActual,
                e =>
                {
                    Assert.Equal(establishment1.Urn, e.Urn);
                    Assert.Equal(establishment1.LaCode, e.LaCode);
                    Assert.Equal(establishment1.LaName, e.LaName);
                    Assert.Equal(establishment1.EstablishmentNumber, e.EstablishmentNumber);
                    Assert.Equal(establishment1.EstablishmentName, e.EstablishmentName);
                    Assert.Equal(establishment1.EstablishmentTypeCode, e.EstablishmentTypeCode);
                    Assert.Equal(establishment1.EstablishmentTypeName, e.EstablishmentTypeName);
                    Assert.Equal(establishment1.EstablishmentTypeGroupCode, e.EstablishmentTypeGroupCode);
                    Assert.Equal(establishment1.EstablishmentTypeGroupName, e.EstablishmentTypeGroupName);
                    Assert.Equal(establishment1.EstablishmentStatusCode, e.EstablishmentStatusCode);
                    Assert.Equal(establishment1.EstablishmentStatusName, e.EstablishmentStatusName);
                    Assert.Equal(establishment1.Street, e.Street);
                    Assert.Equal(establishment1.Locality, e.Locality);
                    Assert.Equal(establishment1.Address3, e.Address3);
                    Assert.Equal(establishment1.Town, e.Town);
                    Assert.Equal(establishment1.County, e.County);
                    Assert.Equal(establishment1.Postcode, e.Postcode);
                },
                e =>
                {
                    Assert.Equal(establishment2.Urn, e.Urn);
                    Assert.Equal(establishment2.LaCode, e.LaCode);
                    Assert.Equal(establishment2.LaName, e.LaName);
                    Assert.Equal(establishment2.EstablishmentNumber, e.EstablishmentNumber);
                    Assert.Equal(establishment2.EstablishmentName, e.EstablishmentName);
                    Assert.Equal(establishment2.EstablishmentTypeCode, e.EstablishmentTypeCode);
                    Assert.Equal(establishment2.EstablishmentTypeName, e.EstablishmentTypeName);
                    Assert.Equal(establishment2.EstablishmentTypeGroupCode, e.EstablishmentTypeGroupCode);
                    Assert.Equal(establishment2.EstablishmentTypeGroupName, e.EstablishmentTypeGroupName);
                    Assert.Equal(establishment2.EstablishmentStatusCode, e.EstablishmentStatusCode);
                    Assert.Equal(establishment2.EstablishmentStatusName, e.EstablishmentStatusName);
                    Assert.Equal(establishment2.Street, e.Street);
                    Assert.Equal(establishment2.Locality, e.Locality);
                    Assert.Equal(establishment2.Address3, e.Address3);
                    Assert.Equal(establishment2.Town, e.Town);
                    Assert.Equal(establishment2.County, e.County);
                    Assert.Equal(establishment2.Postcode, e.Postcode);
                });
        });

    [Fact]
    public Task RefreshEstablishments_WhenCalledForExistingUrn_UpdatesEstablishment() =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var establishmentMasterDataService = Mock.Of<IEstablishmentMasterDataService>();

            var urn = TestData.GenerateEstablishmentUrn();
            var dbEstablishment = new Core.DataStore.Postgres.Models.Establishment()
            {
                EstablishmentId = Guid.NewGuid(),
                EstablishmentSourceId = 1,
                Urn = urn,
                LaCode = "123",
                LaName = "Test LA",
                EstablishmentNumber = "1234",
                EstablishmentName = "Test School",
                EstablishmentTypeCode = "01",
                EstablishmentTypeName = "Primary",
                EstablishmentTypeGroupCode = 1,
                EstablishmentTypeGroupName = "Academy",
                EstablishmentStatusCode = 1,
                EstablishmentStatusName = "Open",
                Street = "Test Street",
                Locality = "Test Locality",
                Address3 = "Test Address3",
                Town = "Test Town",
                County = "Test County",
                Postcode = "TE1 1ST"
            };


            var updatedEstablishment = new Establishment
            {
                Urn = urn,
                LaCode = "124",
                LaName = "Test2 LA",
                EstablishmentNumber = "1235",
                EstablishmentName = "Test School 2",
                EstablishmentTypeCode = "02",
                EstablishmentTypeName = "Secondary",
                EstablishmentTypeGroupCode = 2,
                EstablishmentTypeGroupName = "School",
                EstablishmentStatusCode = 2,
                EstablishmentStatusName = "Closed",
                Street = "Test2 Street",
                Locality = "Test2 Locality",
                Address3 = "Test2 Address3",
                Town = "Test2 Town",
                County = "Test2 County",
                Postcode = "TE1 2ND"
            };

            var establishments = new List<Establishment> { updatedEstablishment };
            Mock.Get(establishmentMasterDataService)
                .Setup(s => s.GetEstablishments())
                .Returns(establishments.ToAsyncEnumerable());

            var establishmentRefresher = new EstablishmentRefresher(
                dbContext,
                establishmentMasterDataService);

            // Act
            await establishmentRefresher.RefreshEstablishments(CancellationToken.None);

            // Assert
            var urnEstablishments = await dbContext.Establishments.Where(e => e.Urn == dbEstablishment.Urn).ToListAsync();
            Assert.Single(urnEstablishments);
            var establishmentActual = urnEstablishments.Single();
            Assert.Equal(updatedEstablishment.Urn, establishmentActual.Urn);
            Assert.Equal(updatedEstablishment.LaCode, establishmentActual.LaCode);
            Assert.Equal(updatedEstablishment.LaName, establishmentActual.LaName);
            Assert.Equal(updatedEstablishment.EstablishmentNumber, establishmentActual.EstablishmentNumber);
            Assert.Equal(updatedEstablishment.EstablishmentName, establishmentActual.EstablishmentName);
            Assert.Equal(updatedEstablishment.EstablishmentTypeCode, establishmentActual.EstablishmentTypeCode);
            Assert.Equal(updatedEstablishment.EstablishmentTypeName, establishmentActual.EstablishmentTypeName);
            Assert.Equal(updatedEstablishment.EstablishmentTypeGroupCode, establishmentActual.EstablishmentTypeGroupCode);
            Assert.Equal(updatedEstablishment.EstablishmentTypeGroupName, establishmentActual.EstablishmentTypeGroupName);
            Assert.Equal(updatedEstablishment.EstablishmentStatusCode, establishmentActual.EstablishmentStatusCode);
            Assert.Equal(updatedEstablishment.EstablishmentStatusName, establishmentActual.EstablishmentStatusName);
            Assert.Equal(updatedEstablishment.Street, establishmentActual.Street);
            Assert.Equal(updatedEstablishment.Locality, establishmentActual.Locality);
            Assert.Equal(updatedEstablishment.Address3, establishmentActual.Address3);
            Assert.Equal(updatedEstablishment.Town, establishmentActual.Town);
            Assert.Equal(updatedEstablishment.County, establishmentActual.County);
            Assert.Equal(updatedEstablishment.Postcode, establishmentActual.Postcode);
        });

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }
}
