using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.DqtNoteAttachments;

if (args is not [var filePath])
{
    Console.Error.WriteLine("Need a filepath");
    Environment.Exit(1);
    return;
}

var config = new ConfigurationBuilder()
    .AddUserSecrets("RoutesMapper")
    .Build();

var crmConnectionString = config["CrmConnectionString"]!;
var dbConnectionString = config["DbConnectionString"]!;

var services = new ServiceCollection();
services.AddTrsBaseServices();
services.AddDatabase(dbConnectionString);
services.AddSingleton<ReferenceDataCache>();
services.AddSingleton<IAuditRepository, BlobStorageAuditRepository>();
services.AddLogging();
services.AddSingleton<IDqtNoteAttachmentStorage, BlobStorageDqtNoteAttachmentStorage>();

services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient("UseDevelopmentStorage=true");
});

services.AddDefaultServiceClient(ServiceLifetime.Singleton, _ => new ServiceClient(crmConnectionString));

services.AddNamedServiceClient(
    TrsDataSyncService.CrmClientName,
    ServiceLifetime.Singleton,
    _ => new ServiceClient(crmConnectionString));

services.AddCrmEntityChangesService(name: TrsDataSyncService.CrmClientName);

services.AddSingleton<TrsDataSyncHelper>();

var sp = services.BuildServiceProvider();

var resiliencePipeline = new ResiliencePipelineBuilder()
    .AddRetry(new Polly.Retry.RetryStrategyOptions()
    {
        BackoffType = DelayBackoffType.Linear,
        Delay = TimeSpan.FromSeconds(30),
        MaxRetryAttempts = 10
    })
    .Build();

var helper = sp.GetRequiredService<TrsDataSyncHelper>();
var serviceClient = sp.GetRequiredKeyedService<IOrganizationServiceAsync2>(TrsDataSyncService.CrmClientName);

var mapped = Map();
await WriteResults(mapped);

async IAsyncEnumerable<TrsDataSyncHelper.IttQtsMapResult[]> Map()
{
    const int pageSize = 1000;

    var columns = new ColumnSet(Contact.Fields.dfeta_qtlsdate);

    var query = new QueryExpression(Contact.EntityLogicalName)
    {
        ColumnSet = columns,
        Orders =
        {
            new OrderExpression(Contact.PrimaryIdAttribute, OrderType.Ascending)
        },
        PageInfo = new PagingInfo()
        {
            Count = pageSize,
            PageNumber = 1
        }
    };

    var ittLink = query.AddLink(
        dfeta_initialteachertraining.EntityLogicalName,
        Contact.PrimaryIdAttribute,
        dfeta_initialteachertraining.Fields.dfeta_PersonId,
        JoinOperator.LeftOuter);
    ittLink.Columns = new ColumnSet(true);
    ittLink.EntityAlias = dfeta_initialteachertraining.EntityLogicalName;
    //ittLink.LinkCriteria.AddCondition("dfeta_qtsregistration", ConditionOperator.NotNull);

    var qtsLink = query.AddLink(
        dfeta_qtsregistration.EntityLogicalName,
        Contact.PrimaryIdAttribute,
        dfeta_qtsregistration.Fields.dfeta_PersonId,
        JoinOperator.LeftOuter);
    qtsLink.Columns = new ColumnSet(true);
    qtsLink.EntityAlias = dfeta_qtsregistration.EntityLogicalName;

    List<Entity> resultsForLastContact = [];

    while (true)
    {
        async Task<EntityCollection> QueryAsync()
        {
             try
             {
                 return await serviceClient.RetrieveMultipleAsync(query);
             }
             catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
             {
                 await Task.Delay(retryAfter);
                 return await QueryAsync();
             }
        }

        var result = await resiliencePipeline.ExecuteAsync(async _ => await QueryAsync());

        // We need to process all QTS and ITT for a given contact at once but given we're paging we may not have every row
        // since rows could span a page boundary.
        // If there's more data available, stash the rows for the final contact and process them next time around.

        var entities = resultsForLastContact.Concat(result.Entities.ToList()).ToList();
        var lastContactId = entities.Last().Id;

        if (result.MoreRecords)
        {
            resultsForLastContact = entities.Where(e => e.Id == lastContactId).ToList();
            entities = entities.Where(e => e.Id != lastContactId).ToList();
        }

        var qts = entities
            .Select(e => e.Extract<dfeta_qtsregistration>())
            .Where(qts => qts is not null)
            .DistinctBy(qts => qts.Id);

        var itt = entities
            .Select(e => e.Extract<dfeta_initialteachertraining>())
            .Where(itt => itt is not null)
            .DistinctBy(itt => itt.Id);

        yield return await helper.MapIttAndQtsRegistrationsAsync(qts, itt, createMigratedEvent: false);

        if (result.MoreRecords)
        {
            //break;
            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = result.PagingCookie;
        }
        else
        {
            break;
        }
    }
}

async Task WriteResults(IAsyncEnumerable<TrsDataSyncHelper.IttQtsMapResult[]> results)
{
    if (File.Exists(filePath))
    {
        File.Delete(filePath);
    }

    using var fs = File.OpenWrite(filePath);
    using var streamWriter = new StreamWriter(fs);
    using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
    csvWriter.WriteField("Contact ID");
    csvWriter.WriteField("Success");
    csvWriter.WriteField("Failed reason");
    csvWriter.WriteField("ITT ID");
    csvWriter.WriteField("QTS ID");
    csvWriter.WriteField("Route");
    csvWriter.NextRecord();

    await foreach (var block in results)
    {
        foreach (var r in block)
        {
            csvWriter.WriteField(r.ContactId);
            csvWriter.WriteField(r.Success);
            csvWriter.WriteField(r.FailedReason);
            csvWriter.WriteField(r.IttId);
            csvWriter.WriteField(r.QtsRegistrationId);
            csvWriter.WriteField(r.ProfessionalStatus?.RouteToProfessionalStatusId);
            csvWriter.NextRecord();
        }
    }
}
