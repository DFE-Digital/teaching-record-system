using System.Net;
using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.IntegrationTransactions;

public class DetailTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_InvalidIntegrationTransactionRecordDetails_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/-1/detail");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidIntegrationTransactionWithNoRecords_ReturnsOk()
    {
        // Arrange
        var totalCount1 = 1;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
        });
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var message = doc.GetElementByTestId("no-integration-transaction-records-message");
        var interfaceType = doc.GetElementByTestId("integration-transaction-interface-type");
        Assert.NotNull(message);
        Assert.NotNull(interfaceType);
        Assert.Contains("No integration transaction records", message.TextContent);
        Assert.Equal($"{interfaceType1.GetDisplayName()} - File details", interfaceType.TextContent);
    }

    [Test]
    public async Task Get_ValidIntegrationTransactionWithRecords_ReturnsOk()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 1;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
            });
        });

        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var itr1 = integrationTransaction1.Records.First();
        doc.AssertResultsContainsIntegrationTransactionRecord(itr1.IntegrationTransactionRecordId);
    }

    [Test]
    [Arguments(IntegrationTransactionRecordStatus.Success, "govuk-tag--green", IntegrationTransactionImportStatus.Success)]
    [Arguments(IntegrationTransactionRecordStatus.Failure, "govuk-tag--red", IntegrationTransactionImportStatus.Failed)]
    public async Task Get_ValidIntegrationTransactionWithRecordsWithStatus_RendersRowExpectedCssClass(IntegrationTransactionRecordStatus recordStatus, string expectedCssClass, IntegrationTransactionImportStatus importStatus)
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 1;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = importStatus;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(recordStatus);
            });
        });
        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        doc.AssertResultsContainsIntegrationTransactionRecord(itr1.IntegrationTransactionRecordId);
        var rowItrId = doc.GetElementByTestId($"integration-transaction-record:{itr1.IntegrationTransactionRecordId}:id");
        var rowContact = doc.GetElementByTestId($"integration-transaction-record:{itr1.IntegrationTransactionRecordId}:personid");
        var rowDuplicate = doc.GetElementByTestId($"integration-transaction-record:{itr1.IntegrationTransactionRecordId}:duplicate");
        var rowStatus = doc.GetElementByTestId($"integration-transaction-record:{itr1.IntegrationTransactionRecordId}:status");
        Assert.NotNull(rowItrId);
        Assert.NotNull(rowContact);
        Assert.NotNull(rowDuplicate);
        Assert.NotNull(rowStatus);
        var statusElement = rowStatus.Children.FirstOrDefault(child => child.ClassList.Contains(expectedCssClass));
        Assert.NotNull(statusElement);
    }

    [Test]
    public async Task Get_RowForUnknownPerson_RendersCorrectPersonText()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 0;
        var successCount1 = 0;
        var failureCount1 = 1;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Failure);
            });
        });
        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var unknownPerson = doc.GetElementByTestId($"integration-transaction-record:{itr1.IntegrationTransactionRecordId}:personid");
        Assert.NotNull(unknownPerson);
        Assert.Contains("Unknown", unknownPerson.TextContent);
        var rowStatus = doc.GetElementByTestId($"integration-transaction-record:{itr1.IntegrationTransactionRecordId}:status");
        Assert.NotNull(rowStatus);
        var statusElement = rowStatus.Children.FirstOrDefault(child => child.ClassList.Contains("govuk-tag--red"));
        Assert.NotNull(statusElement);
        Assert.Contains("Failure", statusElement.TextContent);
    }

    [Test]
    public async Task Get_ExportFailedRecords_ButtonIsVisible()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 0;
        var successCount1 = 0;
        var failureCount1 = 1;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Failure);
            });
        });
        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var exportFailuresButton = doc.GetElementByTestId("export-integration-record-failures");
        Assert.NotNull(exportFailuresButton);
    }

    [Test]
    public async Task Get_ExportFailedRecords_ButtonIsNotVisible()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 0;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>

            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Success);
            });
        });

        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var exportFailuresButton = doc.GetElementByTestId("export-integration-record-failures");
        Assert.Null(exportFailuresButton);
    }

    [Test]
    public async Task Get_DownloadFailures_DownloadsFailedRecords()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 0;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Failed;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Failure);
            });
        });
        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/detail?handler=DownloadFailures");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal($"{integrationTransaction1.IntegrationTransactionId}.{interfaceType1}.failures.csv", response.Content.Headers.ContentDisposition!.FileName);
    }

    [Test]
    public async Task Get_DownloadFailuresForNotExistentIntegrationTransaction_ReturnsNotFound()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 0;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Failed;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Success);
            });
        });
        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{-1}/detail?handler=DownloadFailures");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task Get_ValidIntegrationTransaction_RendersCorrectSummaryCard()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var totalCount1 = 1;
        var successCount1 = 1;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Failed;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;
        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount1);
            p.WithSuccesCount(successCount1);
            p.WithFailureCount(failureCount1);
            p.WithDuplicateCount(duplicateCount1);
            p.WithFileName(fileName1);
            p.WithImportStatus(importStatus1);
            p.WithInterfaceType(interfaceType1);
            p.WithCreatedOn(createdOn1);
            p.WithRow(x =>
            {
                x.WithPersonId(person1.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Success);
            });
        });
        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{integrationTransaction1.IntegrationTransactionId}/detail");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var doc = await response.GetDocumentAsync();
        var integrationSummary = doc.GetElementByTestId("integration-record");
        Assert.NotNull(integrationSummary);
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("Date and time"), createdOn1.ToString(UiDefaults.DateTimeDisplayFormat));
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("File name"), fileName1);
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("Interface ID"), id.ToString());
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("Total count"), totalCount1.ToString());
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("Successes"), successCount1.ToString());
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("Failures"), failureCount1.ToString());
        Assert.Equal(integrationSummary.GetSummaryListValueForKey("Duplicates"), duplicateCount1.ToString());
    }
}

file static class Extensions
{
    public static void AssertResultsContainsIntegrationTransactionRecord(this IHtmlDocument document, long integrationTransactionRecordId) =>
        Assert.NotNull(document.GetElementByTestId($"integration-transaction-record:{integrationTransactionRecordId}"));
}
