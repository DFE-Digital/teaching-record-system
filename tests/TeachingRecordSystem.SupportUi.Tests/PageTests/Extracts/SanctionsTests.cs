using System.Globalization;
using CsvHelper;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Extracts;

public class SanctionsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const string RequestPath = "/extracts";

    [Theory]
    [InlineData(UserRoles.AccessManager)]
    [InlineData(UserRoles.AlertsManagerTra)]
    [InlineData(UserRoles.AlertsManagerTraDbs)]
    [InlineData(UserRoles.RecordManager)]
    [InlineData(UserRoles.Viewer)]
    public async Task Get_ExtractsWithoutAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: role);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ExtractsWithAdministratorRole_ReturnsOk()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_SpentSanctions_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=SpentSanctions";

        await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(Clock.Today.AddDays(-10))
                .WithEndDate(Clock.Today.AddDays(-3))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var csvContent = await response.Content.ReadAsStringAsync();

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "Date of birth",
                "Full name",
                "Sanction name",
                "Alert start date",
                "Alert end date"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_ExtractsWithNewSanction_ReturnsOk()
    {
        // Arrange
        var path = $"{RequestPath}?handler=NewSanctions";
        var startDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(startDate)
                .WithCreatedUtc(Clock.UtcNow.AddDays(-10))));
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var fileName =
            response.Content.Headers.ContentDisposition?.FileNameStar ??
            response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        Assert.Equal(
            $"new-sanctions-{Clock.Today:yyyyMMdd}.csv",
            fileName);

        var csvContent = await response.Content.ReadAsStringAsync();
        var row = await GetCsvRowAsync(response, person.Trn!);

        Assert.Equal(
            $"{person.FirstName} {person.MiddleName} {person.LastName}",
            row["Full name"]?.ToString());

        Assert.Equal(
            startDate.ToString("MM/dd/yyyy"),
            row["Alert start date"]?.ToString());

        Assert.True(
            string.IsNullOrWhiteSpace(
                row["Alert end date"]?.ToString()));
    }

    [Fact]
    public async Task Get_ExtractsWithSpentSanction_ReturnsOk()
    {
        // Arrange
        var path = $"{RequestPath}?handler=SpentSanctions";
        var startDate = Clock.Today.AddDays(-10);
        var endDate = Clock.Today.AddDays(-3);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(startDate)
                .WithEndDate(endDate)
                .WithCreatedUtc(Clock.UtcNow.AddDays(-10))));
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var fileName =
            response.Content.Headers.ContentDisposition?.FileNameStar ??
            response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        Assert.Equal(
            $"spent-sanctions-{Clock.Today:yyyyMMdd}.csv",
            fileName);

        var csvContent = await response.Content.ReadAsStringAsync();
        var row = await GetCsvRowAsync(response, person.Trn!);

        Assert.Equal(
            $"{person.FirstName} {person.MiddleName} {person.LastName}",
            row["Full name"]?.ToString());

        Assert.Equal(
            startDate.ToString("MM/dd/yyyy"),
            row["Alert start date"]?.ToString());

        Assert.Equal(
            endDate.ToString("MM/dd/yyyy"),
            row["Alert end date"]?.ToString());
    }

    private static async Task<IDictionary<string, object>> GetCsvRowAsync(
        HttpResponseMessage response,
        string trn)
    {
        var csvContent = await response.Content.ReadAsStringAsync();

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rows = csv.GetRecords<dynamic>().ToList();

        return rows
            .Cast<IDictionary<string, object>>()
            .Single(r => r["TRN"]?.ToString() == trn);
    }
}
