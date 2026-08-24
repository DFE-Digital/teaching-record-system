using System.Globalization;
using CsvHelper;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Alerts;

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
                .WithStartDate(TimeProvider.Today.AddDays(-10))
                .WithEndDate(TimeProvider.Today.AddDays(-3))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
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
    public async Task Get_AuditAlerts_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=AuditAlerts";
        var updatedOn = new DateTime(2025, 8, 1, 12, 30, 0, DateTimeKind.Utc);
        var alertTypeId = await GetNonExcludedSanctionAlertTypeIdAsync();
        var alertType = await ReferenceDataCache.GetAlertTypeByIdAsync(alertTypeId);
        var startDate = TimeProvider.Today.AddDays(-10);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason"));

        TimeProvider.SetUtcNow(new DateTimeOffset(updatedOn));

        await AlertService.CreateAlertAsync(
            new CreateAlertOptions
            {
                PersonId = person.PersonId,
                AlertTypeId = alertType.AlertTypeId,
                Details = null,
                ExternalLink = null,
                StartDate = startDate,
                EndDate = null
            },
            new ProcessContext(ProcessType.AlertCreating, updatedOn, GetCurrentUserId()));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "Date of change",
                "EventType",
                "Teacher TRN",
                "alert",
                "alert_start",
                "alert_end",
                "alert_created",
                "alert_updated"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_ExtractsWithAuditAlerts_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=AuditAlerts";
        var startDate = TimeProvider.Today.AddDays(-10);
        var alertCreatedOn = new DateTime(2025, 8, 1, 12, 30, 0, DateTimeKind.Utc);
        var extractRunOn = alertCreatedOn.AddDays(7);
        var alertTypeId = await GetNonExcludedSanctionAlertTypeIdAsync();
        var alertType = await ReferenceDataCache.GetAlertTypeByIdAsync(alertTypeId);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason"));

        TimeProvider.SetUtcNow(new DateTimeOffset(alertCreatedOn));

        await AlertService.CreateAlertAsync(
            new CreateAlertOptions
            {
                PersonId = person.PersonId,
                AlertTypeId = alertType.AlertTypeId,
                Details = null,
                ExternalLink = null,
                StartDate = startDate,
                EndDate = null
            },
            new ProcessContext(ProcessType.AlertCreating, alertCreatedOn, GetCurrentUserId()));

        person = await WithDbContextAsync(db =>
            db.Persons.AsNoTracking().SingleAsync(p => p.PersonId == person.PersonId));

        TimeProvider.SetUtcNow(new DateTimeOffset(extractRunOn));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var rows = await GetCsvRowsAsync(response);
        var row = rows.Single(r => NormalizeTrn(r["Teacher TRN"]?.ToString()) == NormalizeTrn(person.Trn));

        Assert.Equal(alertCreatedOn.ToString("MM/dd/yyyy HH:mm:ss"), row["Date of change"]?.ToString());
        Assert.Equal(person.Trn, row["Teacher TRN"]?.ToString());
        Assert.Equal(alertType.Name, row["alert"]?.ToString());
        Assert.Equal(startDate.ToString("MM/dd/yyyy"), row["alert_start"]?.ToString());
        Assert.True(string.IsNullOrWhiteSpace(row["alert_end"]?.ToString()));
        Assert.Equal(alertCreatedOn.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_created"]?.ToString());
        Assert.Equal(alertCreatedOn.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_updated"]?.ToString());
    }

    [Fact]
    public async Task Get_DupSanctions_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DupSanctions";

        await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(TimeProvider.Today.AddDays(-10)))
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(TimeProvider.Today.AddDays(-7))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "Full name",
                "Date of birth",
                "Alert name",
                "Alert start date",
                "Alert end date",
                "Alert created on"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_DqS07DuplicationRecordsWithAlerts_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS07DuplicationRecordsWithAlerts";
        var alertType = await ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.ProhibitionBySoSMisconduct);
        var dateOfBirth = new DateOnly(1985, 4, 12);

        var alertPerson = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alex")
            .WithLastName("Taylor")
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber("AB123456C")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(TimeProvider.Today.AddDays(-10))));

        await TestData.CreatePersonAsync(p => p
            .WithFirstName(alertPerson.FirstName)
            .WithLastName(alertPerson.LastName)
            .WithDateOfBirth(alertPerson.DateOfBirth!.Value)
            .WithNationalInsuranceNumber(alertPerson.NationalInsuranceNumber!));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "First name",
                "Last name",
                "Date of birth",
                "National insurance number",
                "alert",
                "alert_start",
                "alert_end",
                "alert_addedtodqt",
                "TRNS",
                "firstnames",
                "surnames",
                "DOBS",
                "NINOS"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_DqS08IpoWithAlertDetails_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS08IpoWithAlertDetails";
        var alertType = await ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.InterimProhibitionBySoS);

        await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(TimeProvider.Today.AddDays(-10))
                .WithDetails("Details for IPO alert")));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "First name",
                "Middle name",
                "Last name",
                "Date of birth",
                "Full name",
                "alert",
                "alert_start",
                "alert_end",
                "alert_addedtodqt",
                "details"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_DqS10SoSNoProhibitionsActivePast2YearDate_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS10SoSNoProhibitionsActivePast2YearDate";
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).Single(t => t.DqtSanctionCode == "T6");

        await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithMiddleName("P")
            .WithLastName("Mason")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(TimeProvider.Today.AddYears(-3))
                .WithCreatedUtc(TimeProvider.UtcNow.AddYears(-3))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "First name",
                "Middle name",
                "Last name",
                "Full name",
                "alert",
                "alert_start",
                "alert_end",
                "alert_addedtodqt",
                "details"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_DqS11FailedInductionNoAlerts_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS11FailedInductionNoAlerts";

        await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason")
            .WithInductionStatus(i => i
                .WithStatus(InductionStatus.Failed)
                .WithStartDate(new DateOnly(2020, 1, 1))
                .WithCompletedDate(new DateOnly(2020, 1, 2))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "First name",
                "Last name",
                "induction_status",
                "induction_start_date",
                "induction_completed_date"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_DqS12DeletedAlertMonthly_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS12DeletedAlertMonthly";
        var alertType = await GetAlertTypeBySanctionCodeAsync("T1");
        var deletedOn = new DateTime(2025, 9, 17, 8, 30, 0, DateTimeKind.Utc);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithMiddleName("P")
            .WithLastName("Mason")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(TimeProvider.Today.AddDays(-30))
                .WithCreatedUtc(TimeProvider.UtcNow.AddDays(-30))));

        TimeProvider.SetUtcNow(new DateTimeOffset(deletedOn));

        await AlertService.DeleteAlertAsync(
            new DeleteAlertOptions
            {
                AlertId = person.Alerts!.Single().AlertId
            },
            new ProcessContext(ProcessType.AlertDeleting, deletedOn, GetCurrentUserId()));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "First name",
                "Middle name",
                "Last name",
                "Full name",
                "alert",
                "alert_start",
                "alert_end",
                "alert_deleted",
                "alert_updated"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_MonthlyTmuAlertReconciliation_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=MonthlyTmuAlertReconciliation";
        var alertType = await GetAlertTypeBySanctionCodeAsync("T2");

        await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(new DateOnly(2019, 1, 2))
                .WithCreatedUtc(new DateTime(2019, 1, 2, 10, 0, 0, DateTimeKind.Utc))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "Full name",
                "DOB",
                "Alert type",
                "Alert start",
                "Alert end",
                "Alert added to TRS"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_MonthlyTmuAlertReconciliationKpiProcess_ContainsExpectedHeaders()
    {
        // Arrange
        var path = $"{RequestPath}?handler=MonthlyTmuAlertReconciliationKpiProcess";
        var alertType = await GetAlertTypeBySanctionCodeAsync("T2");

        await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(new DateOnly(2020, 1, 2))
                .WithCreatedUtc(new DateTime(2020, 1, 2, 10, 0, 0, DateTimeKind.Utc))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var csv = await CreateCsvReaderAsync(response);
        await csv.ReadAsync();
        csv.ReadHeader();

        Assert.Equal(
            [
                "TRN",
                "Full name",
                "DOB",
                "Alert type",
                "Alert start",
                "Alert end",
                "Alert added to TRS",
                "Alert modified on TRS"
            ],
            csv.HeaderRecord!);
    }

    [Fact]
    public async Task Get_ExtractsWithNewSanction_ReturnsOk()
    {
        // Arrange
        var path = $"{RequestPath}?handler=NewSanctions";
        var startDate = TimeProvider.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(startDate)
                .WithCreatedUtc(TimeProvider.UtcNow.AddDays(-10))));
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
            $"new-sanctions-{TimeProvider.Today:yyyyMMdd}.csv",
            fileName);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

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
        var startDate = TimeProvider.Today.AddDays(-10);
        var endDate = TimeProvider.Today.AddDays(-3);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.ProhibitionBySoSMisconduct)
                .WithStartDate(startDate)
                .WithEndDate(endDate)
                .WithCreatedUtc(TimeProvider.UtcNow.AddDays(-10))));
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
            $"spent-sanctions-{TimeProvider.Today:yyyyMMdd}.csv",
            fileName);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

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

    [Fact]
    public async Task Get_ExtractsWithDupSanctions_ReturnsExpectedRows()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DupSanctions";
        var startDate1 = TimeProvider.Today.AddDays(-10);
        var startDate2 = TimeProvider.Today.AddDays(-7);
        var createdOn1 = new DateTime(2025, 8, 1, 9, 15, 0, DateTimeKind.Utc);
        var createdOn2 = new DateTime(2025, 8, 1, 10, 45, 0, DateTimeKind.Utc);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.ProhibitionBySoSMisconduct);

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(startDate1)
                .WithCreatedUtc(createdOn1))
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(startDate2)
                .WithCreatedUtc(createdOn2)));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var rows = await GetCsvRowsAsync(response);

        Assert.Contains(rows, row => row["TRN"]?.ToString() == person.Trn);
        var firstRow = rows.First(row => row["TRN"]?.ToString() == person.Trn);

        Assert.Equal(
            $"{person.FirstName} {person.LastName}",
            firstRow["Full name"]?.ToString());

        Assert.Equal(
            person.DateOfBirth?.ToString("MM/dd/yyyy"),
            firstRow["Date of birth"]?.ToString());

        Assert.Equal(
            alertType.Name,
            firstRow["Alert name"]?.ToString());

        Assert.Equal(
            startDate1.ToString("MM/dd/yyyy"),
            firstRow["Alert start date"]?.ToString());

        Assert.True(string.IsNullOrWhiteSpace(firstRow["Alert end date"]?.ToString()));

        Assert.Equal(
            createdOn1.ToString("MM/dd/yyyy HH:mm:ss"),
            firstRow["Alert created on"]?.ToString());
    }

    [Fact]
    public async Task Get_ExtractsWithDqS07DuplicationRecordsWithAlerts_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS07DuplicationRecordsWithAlerts";
        var alertType = await ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.ProhibitionBySoSMisconduct);
        var dateOfBirth = new DateOnly(1985, 4, 12);
        var alertStart = TimeProvider.Today.AddDays(-10);
        var alertAddedToDqt = new DateTime(2025, 8, 1, 12, 30, 0, DateTimeKind.Utc);

        var alertPerson = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber()
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(alertStart)
                .WithCreatedUtc(alertAddedToDqt)));

        var duplicatePerson = await TestData.CreatePersonAsync(p => p
            .WithFirstName(alertPerson.FirstName)
            .WithLastName(alertPerson.LastName)
            .WithDateOfBirth(alertPerson.DateOfBirth!.Value)
            .WithNationalInsuranceNumber(alertPerson.NationalInsuranceNumber!));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var rows = await GetCsvRowsAsync(response);
        var row = rows.First(r => r["TRN"]?.ToString() == alertPerson.Trn);

        Assert.Equal(alertPerson.Trn, row["TRN"]?.ToString());
        Assert.Equal(alertPerson.FirstName, row["First name"]?.ToString());
        Assert.Equal(alertPerson.LastName, row["Last name"]?.ToString());
        Assert.Equal(alertPerson.DateOfBirth?.ToString("MM/dd/yyyy"), row["Date of birth"]?.ToString());
        Assert.Equal(alertPerson.NationalInsuranceNumber, row["National insurance number"]?.ToString());
        Assert.Equal(alertType.Name, row["alert"]?.ToString());
        Assert.Equal(alertStart.ToString("MM/dd/yyyy"), row["alert_start"]?.ToString());
        Assert.True(string.IsNullOrWhiteSpace(row["alert_end"]?.ToString()));
        Assert.Equal(alertAddedToDqt.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_addedtodqt"]?.ToString());
        Assert.Equal(duplicatePerson.Trn, row["TRNS"]?.ToString());
        Assert.Equal(duplicatePerson.FirstName, row["firstnames"]?.ToString());
        Assert.Equal(duplicatePerson.LastName, row["surnames"]?.ToString());
        Assert.Equal(duplicatePerson.DateOfBirth?.ToString("MM/dd/yyyy"), row["DOBS"]?.ToString());
        Assert.Equal(duplicatePerson.NationalInsuranceNumber, row["NINOS"]?.ToString());
    }

    [Fact]
    public async Task Get_ExtractsWithDqS08IpoWithAlertDetails_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS08IpoWithAlertDetails";
        var alertType = await ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.InterimProhibitionBySoS);
        var alertStart = TimeProvider.Today.AddDays(-10);
        var alertAddedToDqt = new DateTime(2025, 8, 1, 12, 30, 0, DateTimeKind.Utc);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithMiddleName("P")
            .WithLastName("Mason")
            .WithDateOfBirth(new DateOnly(1981, 3, 2))
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(alertStart)
                .WithCreatedUtc(alertAddedToDqt)
                .WithDetails("Details for IPO alert")));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var rows = await GetCsvRowsAsync(response);
        var row = rows.First(r => r["TRN"]?.ToString() == person.Trn);

        Assert.Equal(person.Trn, row["TRN"]?.ToString());
        Assert.Equal(person.FirstName, row["First name"]?.ToString());
        Assert.Equal(person.MiddleName, row["Middle name"]?.ToString());
        Assert.Equal(person.LastName, row["Last name"]?.ToString());
        Assert.Equal(person.DateOfBirth?.ToString("MM/dd/yyyy"), row["Date of birth"]?.ToString());
        Assert.Equal($"{person.FirstName} {person.LastName}", row["Full name"]?.ToString());
        Assert.Equal(alertType.Name, row["alert"]?.ToString());
        Assert.Equal(alertStart.ToString("MM/dd/yyyy"), row["alert_start"]?.ToString());
        Assert.True(string.IsNullOrWhiteSpace(row["alert_end"]?.ToString()));
        Assert.Equal(alertAddedToDqt.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_addedtodqt"]?.ToString());
        Assert.Equal("Details for IPO alert", row["details"]?.ToString());
    }

    [Fact]
    public async Task Get_ExtractsWithDqS10SoSNoProhibitionsActivePast2YearDate_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS10SoSNoProhibitionsActivePast2YearDate";
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).Single(t => t.DqtSanctionCode == "T6");
        var alertStart = TimeProvider.Today.AddYears(-3);
        var alertAddedToDqt = TimeProvider.UtcNow.AddYears(-3);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithMiddleName("P")
            .WithLastName("Mason")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(alertStart)
                .WithCreatedUtc(alertAddedToDqt)));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

        Assert.Equal(person.Trn, row["TRN"]?.ToString());
        Assert.Equal(person.FirstName, row["First name"]?.ToString());
        Assert.Equal(person.MiddleName, row["Middle name"]?.ToString());
        Assert.Equal(person.LastName, row["Last name"]?.ToString());
        Assert.Equal($"{person.FirstName} {person.LastName}", row["Full name"]?.ToString());
        Assert.Equal(alertType.Name, row["alert"]?.ToString());
        Assert.Equal(alertStart.ToString("MM/dd/yyyy"), row["alert_start"]?.ToString());
        Assert.Equal(alertAddedToDqt.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_addedtodqt"]?.ToString());
        Assert.False(string.IsNullOrWhiteSpace(row["details"]?.ToString()));
    }

    [Fact]
    public async Task Get_ExtractsWithDqS11FailedInductionNoAlerts_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS11FailedInductionNoAlerts";

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason")
            .WithInductionStatus(i => i
                .WithStatus(InductionStatus.Failed)
                .WithStartDate(new DateOnly(2020, 1, 1))
                .WithCompletedDate(new DateOnly(2020, 1, 2))));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

        Assert.Equal(person.Trn, row["TRN"]?.ToString());
        Assert.Equal(person.FirstName, row["First name"]?.ToString());
        Assert.Equal(person.LastName, row["Last name"]?.ToString());
        Assert.Equal(person.InductionStatus.ToString(), row["induction_status"]?.ToString());
        Assert.Equal(person.InductionStartDate?.ToString("MM/dd/yyyy"), row["induction_start_date"]?.ToString());
        Assert.Equal(person.InductionCompletedDate?.ToString("MM/dd/yyyy"), row["induction_completed_date"]?.ToString());
    }

    [Fact]
    public async Task Get_ExtractsWithDqS12DeletedAlertMonthly_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=DqS12DeletedAlertMonthly";
        var alertType = await GetAlertTypeBySanctionCodeAsync("T1");
        var startDate = TimeProvider.Today.AddDays(-30);
        var deletedOn = new DateTime(2025, 9, 17, 8, 30, 0, DateTimeKind.Utc);
        var extractRunOn = deletedOn.AddDays(7);
        var createdOn = TimeProvider.UtcNow.AddDays(-30);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithMiddleName("P")
            .WithLastName("Mason")
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(startDate)
                .WithEndDate(null)
                .WithCreatedUtc(createdOn)));

        TimeProvider.SetUtcNow(new DateTimeOffset(deletedOn));

        await AlertService.DeleteAlertAsync(
            new DeleteAlertOptions
            {
                AlertId = person.Alerts!.Single().AlertId
            },
            new ProcessContext(ProcessType.AlertDeleting, deletedOn, GetCurrentUserId()));

        TimeProvider.SetUtcNow(new DateTimeOffset(extractRunOn));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

        Assert.Equal(person.Trn, row["TRN"]?.ToString());
        Assert.Equal(person.FirstName, row["First name"]?.ToString());
        Assert.Equal(person.MiddleName, row["Middle name"]?.ToString());
        Assert.Equal(person.LastName, row["Last name"]?.ToString());
        Assert.Equal($"{person.FirstName} {person.LastName}", row["Full name"]?.ToString());
        Assert.Equal(alertType.Name, row["alert"]?.ToString());
        Assert.Equal(startDate.ToString("MM/dd/yyyy"), row["alert_start"]?.ToString());
        Assert.True(string.IsNullOrWhiteSpace(row["alert_end"]?.ToString()));
        Assert.Equal(deletedOn.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_deleted"]?.ToString());
        Assert.Equal(deletedOn.ToString("MM/dd/yyyy HH:mm:ss"), row["alert_updated"]?.ToString());
    }

    [Fact]
    public async Task Get_ExtractsWithMonthlyTmuAlertReconciliation_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=MonthlyTmuAlertReconciliation";
        var alertType = await GetAlertTypeBySanctionCodeAsync("T2");
        var startDate = TimeProvider.Today.AddDays(-10);
        var createdOn = TimeProvider.UtcNow.AddDays(-7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason")
            .WithDateOfBirth(new DateOnly(1981, 3, 2))
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(startDate)
                .WithEndDate(null)
                .WithCreatedUtc(createdOn)));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

        Assert.Equal(person.Trn, row["TRN"]?.ToString());
        Assert.Equal($"{person.FirstName} {person.LastName}", row["Full name"]?.ToString());
        Assert.Equal(person.DateOfBirth?.ToString("MM/dd/yyyy"), row["DOB"]?.ToString());
        Assert.Equal(alertType.Name, row["Alert type"]?.ToString());
        Assert.Equal(startDate.ToString("MM/dd/yyyy"), row["Alert start"]?.ToString());
        Assert.True(string.IsNullOrWhiteSpace(row["Alert end"]?.ToString()));
        Assert.Equal(createdOn.ToString("MM/dd/yyyy HH:mm:ss"), row["Alert added to TRS"]?.ToString());
    }

    [Fact]
    public async Task Get_ExtractsWithMonthlyTmuAlertReconciliationKpiProcess_ReturnsExpectedRow()
    {
        // Arrange
        var path = $"{RequestPath}?handler=MonthlyTmuAlertReconciliationKpiProcess";
        var alertType = await GetAlertTypeBySanctionCodeAsync("T2");
        var startDate = TimeProvider.Today.AddDays(-10);
        var createdOn = TimeProvider.UtcNow.AddDays(-7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jordan")
            .WithLastName("Mason")
            .WithDateOfBirth(new DateOnly(1981, 3, 2))
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(startDate)
                .WithCreatedUtc(createdOn)));

        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var row = await GetCsvRowAsync(response, "TRN", person.Trn!);

        Assert.Equal(person.Trn, row["TRN"]?.ToString());
        Assert.Equal($"{person.FirstName} {person.LastName}", row["Full name"]?.ToString());
        Assert.Equal(person.DateOfBirth?.ToString("MM/dd/yyyy"), row["DOB"]?.ToString());
        Assert.Equal(alertType.Name, row["Alert type"]?.ToString());
        Assert.Equal(startDate.ToString("MM/dd/yyyy"), row["Alert start"]?.ToString());
        Assert.True(string.IsNullOrWhiteSpace(row["Alert end"]?.ToString()));
        Assert.Equal(createdOn.ToString("MM/dd/yyyy HH:mm:ss"), row["Alert added to TRS"]?.ToString());
        Assert.Equal(createdOn.ToString("MM/dd/yyyy HH:mm:ss"), row["Alert modified on TRS"]?.ToString());
    }

    private async Task<Guid> GetNonExcludedSanctionAlertTypeIdAsync()
    {
        var excludedNames = new[]
        {
            "Prohibition by the Secretary of State - misconduct",
            "Secretary of State decision - no prohibition",
            "Interim prohibition by the Secretary of State"
        };

        return (await ReferenceDataCache.GetAlertTypesAsync())
            .First(t =>
                t.DqtSanctionCode is not null &&
                t.DqtSanctionCode != "E3" &&
                !excludedNames.Contains(t.Name))
            .AlertTypeId;
    }

    private async Task<AlertType> GetAlertTypeBySanctionCodeAsync(string sanctionCode) =>
        (await ReferenceDataCache.GetAlertTypesAsync())
            .Single(t => t.DqtSanctionCode == sanctionCode);

    private static async Task<IReadOnlyList<IDictionary<string, object>>> GetCsvRowsAsync(
        HttpResponseMessage response)
    {
        var csvContent = await response.Content.ReadAsStringAsync();

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<dynamic>()
            .Cast<IDictionary<string, object>>()
            .ToList();
    }

    private static async Task<IDictionary<string, object>> GetCsvRowAsync(
        HttpResponseMessage response,
        string headerName,
        string value)
    {
        var rows = await GetCsvRowsAsync(response);

        return rows.Single(r => string.Equals(
            r[headerName]?.ToString()?.Trim(),
            value.Trim(),
            StringComparison.Ordinal));
    }

    private static string NormalizeTrn(string? trn) =>
        new string((trn ?? string.Empty).Where(char.IsDigit).ToArray());

    private static async Task<CsvReader> CreateCsvReaderAsync(HttpResponseMessage response)
    {
        var csvContent = await response.Content.ReadAsStringAsync();
        var reader = new StringReader(csvContent);
        return new CsvReader(reader, CultureInfo.InvariantCulture);
    }
}
