namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;

public class IntegrationTransactionsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(IntegrationTransactionSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/IntegrationTransactions/Index", routeValues: new { sortBy, sortDirection, pageNumber });

    public string Detail(long integrationTransactionId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/IntegrationTransactions/Detail", routeValues: new { integrationTransactionId });

    public string Detail(IntegrationTransactionRecordSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = 1, long? IntegrationTransactionId = null) =>
        linkGenerator.GetRequiredPathByPage($"/SupportTasks/IntegrationTransactions/Detail", routeValues: new { sortBy, sortDirection, pageNumber, IntegrationTransactionId });

    public string Row(long integrationTransactionId, long integrationTransactionRecordId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/IntegrationTransactions/Row", routeValues: new { integrationTransactionId, integrationTransactionRecordId });
}
