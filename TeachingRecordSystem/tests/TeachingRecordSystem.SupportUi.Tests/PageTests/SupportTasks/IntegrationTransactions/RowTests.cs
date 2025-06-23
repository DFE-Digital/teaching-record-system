using System.Net;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.IntegrationTransactions;

public class RowTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(IntegrationTransactionRecordStatus.Success)]
    [InlineData(IntegrationTransactionRecordStatus.Failure)]
    public async Task Get_Row_RowStatusIsCorrect(IntegrationTransactionRecordStatus rowStatus)
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
                x.WithStatus(rowStatus);
            });
        });

        var itr1 = integrationTransaction1.Records.First();
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/row?integrationtransactionrecordid={itr1.IntegrationTransactionRecordId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var transactionId = doc.GetElementByTestId("integration-transaction-record-id");
        var recordStatus = doc.GetElementByTestId("integration-transaction-record-status");
        Assert.NotNull(transactionId);
        Assert.NotNull(recordStatus);
        Assert.Contains(rowStatus.GetDisplayName()!, recordStatus.TextContent);
        Assert.Contains(itr1.IntegrationTransactionRecordId.ToString(), transactionId.TextContent);
    }

    [Fact]
    public async Task Get_NonExistentRow_ReturnsNotFound()
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
        });
        var id = integrationTransaction1.IntegrationTransactionId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{id}/row?integrationtransactionrecordid={-1}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_NonExistentIntegrationTransaction_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/integration-transactions/{-1}/row?integrationtransactionrecordid={-1}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

