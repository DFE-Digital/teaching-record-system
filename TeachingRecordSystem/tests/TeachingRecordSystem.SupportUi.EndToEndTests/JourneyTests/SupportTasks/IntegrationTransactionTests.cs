namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class IntegrationTransactionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task NoIntegrationTransactionRecordRows()
    {
        var totalCount1 = 0;
        var successCount1 = 0;
        var warningCount1 = 0;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = DateTime.UtcNow;
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
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToIntegrationTransactionsPageAsync();
        await page.ClickAsync($"{LinkHrefContains($"{integrationTransaction1.IntegrationTransactionId.ToString()}/detail")}");
        await page.AssertOnIntegrationTransactionDetailPageAsync(integrationTransaction1.IntegrationTransactionId);
        var locator = page.Locator("td:has-text(\"No integration transaction records\")");
        var isVisible = await locator.IsVisibleAsync();
        Assert.True(isVisible);
    }

    [Fact]
    public async Task SuccessfullyImportedIntegrationTransactionRecords()
    {
        var totalCount1 = 1;
        var successCount1 = 1;
        var warningCount1 = 0;
        var failureCount1 = 0;
        var duplicateCount1 = 0;
        var fileName1 = "FileName.csv";
        var person = await TestData.CreatePersonAsync();
        var importStatus1 = IntegrationTransactionImportStatus.Success;
        var interfaceType1 = IntegrationTransactionInterfaceType.EwcWales;
        var createdOn1 = DateTime.UtcNow;
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
            p.WithRow(x =>
            {
                x.WithPersonId(person.PersonId);
                x.WithRowData("some,random,csv,data");
                x.WithFailureMessage("Some failure message");
                x.WithStatus(IntegrationTransactionRecordStatus.Success);
            });
        });

        var integrationTransactions = await WithDbContextAsync(dbContext =>
         dbContext.IntegrationTransactions
             .Include(itr => itr.IntegrationTransactionRecords)
             .Where(it => it.IntegrationTransactionId == integrationTransaction1.IntegrationTransactionId)
             .ToArrayAsync());
        var itr = integrationTransactions!.First().IntegrationTransactionRecords!.First();
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToIntegrationTransactionsPageAsync();
        await page.ClickAsync($"{LinkHrefContains($"{integrationTransaction1.IntegrationTransactionId.ToString()}/detail")}");
        await page.AssertOnIntegrationTransactionDetailPageAsync(integrationTransaction1.IntegrationTransactionId);
        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.LastName}")}");
        await page.AssertOnIntegrationTransactionDetailRowPageAsync(integrationTransaction1.IntegrationTransactionId, itr.IntegrationTransactionRecordId);
    }
}
