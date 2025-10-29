using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.IntegrationTransactions;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_IntegrationTransactionsReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_IntegrationTransactionsRendersNoRecords()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var noRecordsCell = doc.GetElementByTestId("no-integration-transactions-message");
        Assert.NotNull(noRecordsCell);
        Assert.Contains("No integration transactions", noRecordsCell.TextContent);
    }

    [Test]
    public async Task Get_IntegrationSingleIntegrationTransaction_RendorMultipleRows()
    {
        // Arrange
        var totalCount1 = 1;
        var successCount1 = 1;
        var warningCount1 = 0;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = Clock.UtcNow;


        var totalCount2 = 10;
        var successCount2 = 4;
        var warningCount2 = 1;
        var failureCount2 = 3;
        var duplicateCount2 = 2;
        var fileName2 = "MoreThanOneRow.csv";
        var importStatus2 = IntegrationTransactionImportStatus.Success;
        var interfaceType2 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn2 = Clock.UtcNow;


        var integrationTransaction1 = await TestData.CreateIntegrationTransactionAsync(p =>
            {
                p.WithTotalCount(totalCount1);
                p.WithSuccessCount(successCount1);
                p.WithWarningCount(warningCount1);
                p.WithFailureCount(failureCount1);
                p.WithDuplicateCount(duplicateCount1);
                p.WithFileName(fileName1);
                p.WithImportStatus(importStatus1);
                p.WithInterfaceType(interfaceType1);
                p.WithCreatedOn(createdOn1);
            });

        var integrationTransaction2 = await TestData.CreateIntegrationTransactionAsync(p =>
            {
                p.WithTotalCount(totalCount2);
                p.WithSuccessCount(successCount2);
                p.WithWarningCount(warningCount2);
                p.WithFailureCount(failureCount2);
                p.WithDuplicateCount(duplicateCount2);
                p.WithFileName(fileName2);
                p.WithImportStatus(importStatus2);
                p.WithInterfaceType(interfaceType2);
                p.WithCreatedOn(createdOn2);
            });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        doc.AssertResultsContainsIntegrationTransaction(integrationTransaction1.IntegrationTransactionId.ToString());
        doc.AssertResultsContainsIntegrationTransaction(integrationTransaction2.IntegrationTransactionId.ToString());
    }

    [Test]
    public async Task Get_IntegrationSingleIntegrationTransaction_RendersRow()
    {
        // Arrange
        var totalCount = 1;
        var successCount = 1;
        var warningCount = 0;
        var failureCount = 0;
        var duplicateCount = 0;
        var fileName = "FileName.csv";
        var importStatus = IntegrationTransactionImportStatus.Success;
        var interfaceType = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn = Clock.UtcNow;
        var integrationTransaction = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount);
            p.WithSuccessCount(successCount);
            p.WithWarningCount(warningCount);
            p.WithFailureCount(failureCount);
            p.WithDuplicateCount(duplicateCount);
            p.WithFileName(fileName);
            p.WithImportStatus(importStatus);
            p.WithInterfaceType(interfaceType);
            p.WithCreatedOn(createdOn);
        });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        doc.AssertResultsContainsIntegrationTransaction(integrationTransaction.IntegrationTransactionId.ToString());

        var idCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:id");
        var interfaceCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:interface");
        var createdonCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:createdon");
        var importstatusCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:importstatus");
        var totalCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:total");
        var successesCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:successes");
        var warningsCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:warnings");
        var failuresCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:failures");
        var duplicatesCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:duplicates");
        Assert.NotNull(idCell);
        Assert.NotNull(interfaceCell);
        Assert.NotNull(createdonCell);
        Assert.NotNull(importstatusCell);
        Assert.NotNull(totalCell);
        Assert.NotNull(successesCell);
        Assert.NotNull(warningsCell);
        Assert.NotNull(failuresCell);
        Assert.NotNull(duplicatesCell);
        Assert.Equal(integrationTransaction.IntegrationTransactionId.ToString(), idCell.TextContent.Trim());
        Assert.Equal(interfaceType.GetDisplayName(), interfaceCell.TextContent.Trim());
        Assert.Contains(createdOn.ToString(UiDefaults.DateOnlyDisplayFormat), createdonCell.TextContent.Trim());
        Assert.Contains(createdOn.ToString("hh:mm tt"), createdonCell.TextContent.Trim());
        Assert.Equal(importStatus.ToString(), importstatusCell.TextContent.Trim());
        Assert.Equal(totalCount.ToString(), totalCell.TextContent.Trim());
        Assert.Equal(successCount.ToString(), successesCell.TextContent.Trim());
        Assert.Equal(warningCount.ToString(), warningsCell.TextContent.Trim());
        Assert.Equal(failureCount.ToString(), failuresCell.TextContent.Trim());
        Assert.Equal(duplicateCount.ToString(), duplicatesCell.TextContent.Trim());
    }

    [Test]
    [Arguments(IntegrationTransactionImportStatus.InProgress, "govuk-tag--light-blue")]
    [Arguments(IntegrationTransactionImportStatus.Success, "govuk-tag--green")]
    [Arguments(IntegrationTransactionImportStatus.Failed, "govuk-tag--red")]
    public async Task Get_IntegrationSingleIntegrationTransaction_RendersStatusWithCssClass(IntegrationTransactionImportStatus status, string cssClass)
    {
        // Arrange
        var totalCount = 1;
        var successCount = 1;
        var warningCount = 1;
        var failureCount = 0;
        var duplicateCount = 0;
        var fileName = "FileName.csv";
        var importStatus = status;
        var interfaceType = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn = Clock.UtcNow;
        var integrationTransaction = await TestData.CreateIntegrationTransactionAsync(p =>
        {
            p.WithTotalCount(totalCount);
            p.WithSuccessCount(successCount);
            p.WithWarningCount(warningCount);
            p.WithFailureCount(failureCount);
            p.WithDuplicateCount(duplicateCount);
            p.WithFileName(fileName);
            p.WithImportStatus(importStatus);
            p.WithInterfaceType(interfaceType);
            p.WithCreatedOn(createdOn);
        });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        doc.AssertResultsContainsIntegrationTransaction(integrationTransaction.IntegrationTransactionId.ToString());
        var importstatusCell = doc.GetElementByTestId($"integration-transaction:{integrationTransaction.IntegrationTransactionId.ToString()}:importstatus");
        Assert.NotNull(importstatusCell);
        var statusElement = importstatusCell.Children.FirstOrDefault(child => child.ClassList.Contains(cssClass));
        Assert.NotNull(statusElement);
    }
}

file static class Extensions
{
    public static void AssertResultsContainsIntegrationTransaction(this IHtmlDocument document, string integrationTransactionId) =>
        Assert.NotNull(document.GetElementByTestId($"integration-transaction:{integrationTransactionId}"));
}
