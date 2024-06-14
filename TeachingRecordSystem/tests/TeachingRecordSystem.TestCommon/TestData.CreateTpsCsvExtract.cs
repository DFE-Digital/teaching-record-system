using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task CreateTpsCsvExtract(Action<TpsCsvExtractBuilder>? configure = null)
    {
        var builder = new TpsCsvExtractBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class TpsCsvExtractBuilder
    {
        private readonly string[] validGenderValues = ["Male", "Female"];
        private readonly string[] validFullOrPartTimeIndicatorValues = ["FT", "PTI", "PTR"];
        private readonly List<TpsCsvExtractItem> _items = new();
        private Guid? _tpsCsvExtractId;
        private string? _filename;

        public TpsCsvExtractBuilder WithTpsCsvExtractId(Guid tpsCsvExtractId)
        {
            if (_tpsCsvExtractId.HasValue && _tpsCsvExtractId.Value != tpsCsvExtractId)
            {
                throw new InvalidOperationException("TpsCsvExtractId already set");
            }

            _tpsCsvExtractId = tpsCsvExtractId;
            return this;
        }

        public TpsCsvExtractBuilder WithFilename(string filename)
        {
            if (_filename != null && _filename != filename)
            {
                throw new InvalidOperationException("Filename already set");
            }

            _filename = filename;
            return this;
        }

        public TpsCsvExtractBuilder WithItem(
            string trn,
            string localAuthorityCode,
            string? establishmentNumber,
            string establishmentPostcode,
            DateOnly startDate,
            DateOnly endDate,
            DateOnly extractDate,
            string? fullOrPartTimeIndicator = null,
            string? nationalInsuranceNumber = null,
            DateOnly? dateOfBirth = null,
            string? memberPostcode = null)
        {
            nationalInsuranceNumber ??= Faker.Identification.UkNationalInsuranceNumber();
            dateOfBirth ??= DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            fullOrPartTimeIndicator ??= validFullOrPartTimeIndicatorValues[Faker.RandomNumber.Next(0, 2)];

            _items.Add(new TpsCsvExtractItem(trn, nationalInsuranceNumber, dateOfBirth.Value, localAuthorityCode, establishmentPostcode, establishmentNumber, startDate, endDate, fullOrPartTimeIndicator, extractDate, memberPostcode));
            return this;
        }

        internal async Task Execute(TestData testData)
        {
            if (_tpsCsvExtractId is null)
            {
                throw new InvalidOperationException("TpsCsvExtractId has not been set");
            }

            _tpsCsvExtractId ??= Guid.NewGuid();
            _filename ??= "test.csv";
            var createdOn = testData.Clock.UtcNow;

            var tpsCsvExtract = new TpsCsvExtract
            {
                TpsCsvExtractId = _tpsCsvExtractId.Value,
                Filename = _filename,
                CreatedOn = createdOn
            };

            await testData.WithDbContext(async dbContext =>
            {
                dbContext.TpsCsvExtracts.Add(tpsCsvExtract);
                int memberId = 10000;
                foreach (var item in _items)
                {
                    var loadItem = new TpsCsvExtractLoadItem
                    {
                        TpsCsvExtractLoadItemId = Guid.NewGuid(),
                        TpsCsvExtractId = tpsCsvExtract.TpsCsvExtractId,
                        Trn = item.Trn,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        DateOfBirth = item.DateOfBirth.ToString("dd/MM/yyyy"),
                        DateOfDeath = null,
                        MemberPostcode = item.MemberPostcode,
                        MemberEmailAddress = null,
                        LocalAuthorityCode = item.LocalAuthorityCode,
                        EstablishmentNumber = item.EstablishmentNumber,
                        EstablishmentPostcode = item.EstablishmentPostcode,
                        EstablishmentEmailAddress = null,
                        MemberId = memberId++.ToString(),
                        EmploymentStartDate = item.StartDate.ToString("dd/MM/yyyy"),
                        EmploymentEndDate = item.EndDate.ToString("dd/MM/yyyy"),
                        FullOrPartTimeIndicator = item.FullOrPartTimeIndicator,
                        WithdrawlIndicator = null,
                        ExtractDate = item.ExtractDate.ToString("dd/MM/yyyy"),
                        Gender = validGenderValues[Faker.RandomNumber.Next(0, 1)],
                        Errors = TpsCsvExtractItemLoadErrors.None,
                        Created = DateTime.UtcNow
                    };

                    dbContext.TpsCsvExtractLoadItems.Add(loadItem);

                    var validItem = new Core.DataStore.Postgres.Models.TpsCsvExtractItem
                    {
                        TpsCsvExtractItemId = Guid.NewGuid(),
                        TpsCsvExtractId = tpsCsvExtract.TpsCsvExtractId,
                        TpsCsvExtractLoadItemId = loadItem.TpsCsvExtractLoadItemId,
                        Trn = item.Trn,
                        NationalInsuranceNumber = item.NationalInsuranceNumber,
                        DateOfBirth = item.DateOfBirth,
                        DateOfDeath = null,
                        Gender = loadItem.Gender,
                        MemberPostcode = loadItem.MemberPostcode,
                        MemberEmailAddress = loadItem.MemberEmailAddress,
                        LocalAuthorityCode = item.LocalAuthorityCode,
                        EstablishmentNumber = item.EstablishmentNumber,
                        EstablishmentPostcode = item.EstablishmentPostcode,
                        EstablishmentEmailAddress = loadItem.EstablishmentEmailAddress,
                        MemberId = int.Parse(loadItem.MemberId),
                        EmploymentStartDate = item.StartDate,
                        EmploymentEndDate = item.EndDate,
                        EmploymentType = EmploymentTypeHelper.FromFullOrPartTimeIndicator(loadItem.FullOrPartTimeIndicator),
                        WithdrawlIndicator = loadItem.WithdrawlIndicator,
                        ExtractDate = item.ExtractDate,
                        Created = createdOn,
                        Result = null,
                        Key = $"{item.Trn}.{item.LocalAuthorityCode}.{item.EstablishmentNumber}.{item.StartDate:yyyyMMdd}"
                    };

                    dbContext.TpsCsvExtractItems.Add(validItem);
                }

                await dbContext.SaveChangesAsync();
            });
        }
    }

    public record TpsCsvExtractItem(string Trn, string NationalInsuranceNumber, DateOnly DateOfBirth, string LocalAuthorityCode, string EstablishmentPostcode, string? EstablishmentNumber, DateOnly StartDate, DateOnly EndDate, string FullOrPartTimeIndicator, DateOnly ExtractDate, string? MemberPostcode);
}
