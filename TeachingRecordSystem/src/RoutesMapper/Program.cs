using System.Globalization;
using System.ServiceModel;
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
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using System.Text.Json;
using TeachingRecordSystem.Core.Services.Files;

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
services.AddFileService();
services.AddLogging();

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
var referenceDataCache = sp.GetRequiredService<ReferenceDataCache>();

var mapped = MapAsync();
await WriteResultsAsync(mapped);

async IAsyncEnumerable<TrsDataSyncHelper.IttQtsMapResult[]> MapAsync()
{
    const int pageSize = 1000;

    var columns = new ColumnSet(Contact.Fields.dfeta_qtlsdate);
    ////var contactFilter = new FilterExpression();
    ////contactFilter.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.Equal, Guid.Parse("9439f249-278f-e811-9100-000d3a269589"));    

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
        },
        ////Criteria = contactFilter
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

        var contacts = entities
            .Select(e => e.ToEntity<Contact>())
            .DistinctBy(c => c.Id);

        var qts = entities
            .Select(e => e.Extract<dfeta_qtsregistration>())
            .Where(qts => qts is not null)
            .DistinctBy(qts => qts.Id);

        var itt = entities
            .Select(e => e.Extract<dfeta_initialteachertraining>())
            .Where(itt => itt is not null)
            .DistinctBy(itt => itt.Id);

        yield return await helper.MapIttAndQtsRegistrationsAsync(contacts, qts, itt, createMigratedEvent: false);

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

async Task WriteResultsAsync(IAsyncEnumerable<TrsDataSyncHelper.IttQtsMapResult[]> results)
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
    csvWriter.WriteField("Route ID");
    csvWriter.WriteField("Route Name");
    csvWriter.WriteField("Teacher Status Value");
    csvWriter.WriteField("Teacher Status Name");
    csvWriter.WriteField("Early Years Status Value");
    csvWriter.WriteField("Early Years Status Name");
    csvWriter.WriteField("Programme Type");
    csvWriter.WriteField("ITT Result");
    csvWriter.WriteField("ITT Qual Value");
    csvWriter.WriteField("ITT Qual Name");
    csvWriter.WriteField("Teacher Status Derived Route ID");
    csvWriter.WriteField("Teacher Status Derived Route Name");
    csvWriter.WriteField("Programme Type Derived Route ID");
    csvWriter.WriteField("Programme Type Derived Route Name");
    csvWriter.WriteField("ITT Qual Derived Route ID");
    csvWriter.WriteField("ITT Qual Derived Route Name");
    csvWriter.WriteField("Multiple compatible ITT records");
    csvWriter.NextRecord();

    await foreach (var block in results)
    {
        foreach (var r in block)
        {
            RouteToProfessionalStatus? route = null;
            if (r.ProfessionalStatus is not null)
            {
                route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(r.ProfessionalStatus.RouteToProfessionalStatusId);
            }

            RouteToProfessionalStatus? statusDerivedRoute = null;
            if (r.StatusDerivedRouteId is not null)
            {
                statusDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(r.StatusDerivedRouteId.Value);
            }

            RouteToProfessionalStatus? programmeTypeDerivedRoute = null;
            if (r.ProgrammeTypeDerivedRouteId is not null)
            {
                programmeTypeDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(r.ProgrammeTypeDerivedRouteId.Value);
            }

            RouteToProfessionalStatus? ittQualificationDerivedRoute = null;
            if (r.IttQualificationDerivedRouteId is not null)
            {
                ittQualificationDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(r.IttQualificationDerivedRouteId.Value);
            }

            csvWriter.WriteField(r.ContactId);
            csvWriter.WriteField(r.Success);
            csvWriter.WriteField(r.FailedReason);
            csvWriter.WriteField(r.IttId);
            csvWriter.WriteField(r.QtsRegistrationId);
            csvWriter.WriteField(route?.RouteToProfessionalStatusId);
            csvWriter.WriteField(route?.Name);
            csvWriter.WriteField(r.TeacherStatus?.dfeta_Value);
            csvWriter.WriteField(r.TeacherStatus?.dfeta_name);
            csvWriter.WriteField(r.EarlyYearsStatus?.dfeta_Value);
            csvWriter.WriteField(r.EarlyYearsStatus?.dfeta_name);
            csvWriter.WriteField(r.ProgrammeType);
            csvWriter.WriteField(r.IttResult);
            csvWriter.WriteField(r.IttQualification?.dfeta_Value);
            csvWriter.WriteField(r.IttQualification?.dfeta_name);
            csvWriter.WriteField(r.StatusDerivedRouteId);
            csvWriter.WriteField(statusDerivedRoute?.Name);
            csvWriter.WriteField(r.ProgrammeTypeDerivedRouteId);
            csvWriter.WriteField(programmeTypeDerivedRoute?.Name);
            csvWriter.WriteField(r.IttQualificationDerivedRouteId);
            csvWriter.WriteField(ittQualificationDerivedRoute?.Name);
            csvWriter.WriteField(JsonSerializer.Serialize(r.MultiplePotentialCompatibleIttRecords));
            csvWriter.NextRecord();
        }
    }
}
