using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace TeachingRecordSystem.Core.Services.Establishments.Gias;

public class CsvDownloadEstablishmentMasterDataService : IEstablishmentMasterDataService
{
    private readonly HttpClient _httpClient;
    private readonly IClock _clock;

    public CsvDownloadEstablishmentMasterDataService(
        HttpClient httpClient,
        IClock clock)
    {
        _httpClient = httpClient;
        _clock = clock;
    }

    public async IAsyncEnumerable<Establishment> GetEstablishments()
    {
        var filename = GetLatestEstablishmentsCsvFilename();
        using var response = await _httpClient.GetAsync(filename, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        // The CSV file is encoded in Windows 1252 encoding which is almost identical to Latin1 and allows Welsh and other special characters to be read correctly
        using var reader = new StreamReader(stream, Encoding.Latin1);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

        await foreach (var item in csv.GetRecordsAsync<EstablishmentCsvRow>())
        {
            yield return new Establishment
            {
                Urn = item.Urn,
                LaCode = item.LaCode,
                LaName = item.LaName,
                EstablishmentNumber = item.EstablishmentNumber,
                EstablishmentName = item.EstablishmentName,
                EstablishmentTypeCode = item.EstablishmentTypeCode,
                EstablishmentTypeName = item.EstablishmentTypeName,
                EstablishmentTypeGroupCode = int.Parse(item.EstablishmentGroupTypeCode),
                EstablishmentTypeGroupName = item.EstablishmentGroupTypeName,
                EstablishmentStatusCode = int.Parse(item.EstablishmentStatusCode),
                EstablishmentStatusName = item.EstablishmentStatusName,
                Street = item.Street,
                Locality = item.Locality,
                Address3 = item.Address3,
                Town = item.Town,
                County = item.County,
                Postcode = item.Postcode
            };
        }
    }

    private string GetLatestEstablishmentsCsvFilename()
    {
        var filename = $"edubasealldata{_clock.UtcNow:yyyyMMdd}.csv";
        return filename;
    }
}
