using System.Globalization;
using System.ServiceModel;
using System.Text.Json;
using CsvHelper;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

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
    csvWriter.WriteField("Age Specialism Type");
    csvWriter.WriteField("Age Specialism Range From");
    csvWriter.WriteField("Age Specialism Range To");
    csvWriter.WriteField("Status");
    csvWriter.WriteField("Holds From");
    csvWriter.WriteField("Degree Type");
    csvWriter.WriteField("Training Provider Name");
    csvWriter.WriteField("Training Country");
    csvWriter.WriteField("Training Subject Codes");
    csvWriter.WriteField("Teacher Status Value");
    csvWriter.WriteField("Teacher Status Name");
    csvWriter.WriteField("QTS Date");
    csvWriter.WriteField("Early Years Status Value");
    csvWriter.WriteField("Early Years Status Name");
    csvWriter.WriteField("EYTS Date");
    csvWriter.WriteField("Partial Recognition Date");
    csvWriter.WriteField("QTLS Date");
    csvWriter.WriteField("Programme Type");
    csvWriter.WriteField("DQT Age Range From");
    csvWriter.WriteField("DQT Age Range To");
    csvWriter.WriteField("ITT Result");
    csvWriter.WriteField("ITT Qual Value");
    csvWriter.WriteField("ITT Qual Name");
    csvWriter.WriteField("ITT Provider Name");
    csvWriter.WriteField("ITT Country");
    csvWriter.WriteField("ITT Subject 1");
    csvWriter.WriteField("ITT Subject 2");
    csvWriter.WriteField("ITT Subject 3");
    csvWriter.WriteField("Teacher Status Derived Route ID");
    csvWriter.WriteField("Teacher Status Derived Route Name");
    csvWriter.WriteField("Programme Type Derived Route ID");
    csvWriter.WriteField("Programme Type Derived Route Name");
    csvWriter.WriteField("ITT Qual Derived Route ID");
    csvWriter.WriteField("ITT Qual Derived Route Name");
    csvWriter.WriteField("Multiple compatible ITT records");
    csvWriter.WriteField("Contact ITT Row Count");
    csvWriter.WriteField("Contact QTS Row Count");
    csvWriter.NextRecord();

    await foreach (var block in results)
    {
        foreach (var r in block)
        {
            RouteToProfessionalStatusType? route = null;
            if (r.ProfessionalStatus is not null)
            {
                route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(r.ProfessionalStatus.RouteToProfessionalStatusTypeId);
            }

            RouteToProfessionalStatusType? statusDerivedRoute = null;
            if (r.StatusDerivedRouteId is not null)
            {
                statusDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(r.StatusDerivedRouteId.Value);
            }

            RouteToProfessionalStatusType? programmeTypeDerivedRoute = null;
            if (r.ProgrammeTypeDerivedRouteId is not null)
            {
                programmeTypeDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(r.ProgrammeTypeDerivedRouteId.Value);
            }

            RouteToProfessionalStatusType? ittQualificationDerivedRoute = null;
            if (r.IttQualificationDerivedRouteId is not null)
            {
                ittQualificationDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(r.IttQualificationDerivedRouteId.Value);
            }

            var degreeType = r.ProfessionalStatus?.DegreeTypeId is not null
                ? await referenceDataCache.GetDegreeTypeByIdAsync(r.ProfessionalStatus.DegreeTypeId!.Value)
                : null;

            var trainingProvider = r.ProfessionalStatus?.TrainingProviderId is not null
                ? await referenceDataCache.GetTrainingProviderByIdAsync(r.ProfessionalStatus.TrainingProviderId.Value)
                : null;

            var trainingCountry = r.ProfessionalStatus?.TrainingCountryId is not null
                ? await referenceDataCache.GetTrainingCountryByIdAsync(r.ProfessionalStatus.TrainingCountryId)
                : null;

            var trainingSubjects = r.ProfessionalStatus?.TrainingSubjectIds is not null
                ? await Task.WhenAll(r.ProfessionalStatus.TrainingSubjectIds.Select(id => referenceDataCache.GetTrainingSubjectsByIdAsync(id)))
                : [];

            var ittProvider = r.IttProviderId is not null
                ? await referenceDataCache.GetIttProviderByIdAsync(r.IttProviderId.Value)
                : null;

            var ittCountry = r.IttCountryId is not null
                ? await referenceDataCache.GetCountryByIdAsync(r.IttCountryId.Value)
                : null;

            var ittSubject1 = r.IttSubjectId1 is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(r.IttSubjectId1.Value)
                : null;

            var ittSubject2 = r.IttSubjectId2 is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(r.IttSubjectId2.Value)
                : null;

            var ittSubject3 = r.IttSubjectId3 is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(r.IttSubjectId3.Value)
                : null;

            csvWriter.WriteField(r.ContactId);
            csvWriter.WriteField(r.Success);
            csvWriter.WriteField(r.FailedReason);
            csvWriter.WriteField(r.IttId);
            csvWriter.WriteField(r.QtsRegistrationId);
            csvWriter.WriteField(route?.RouteToProfessionalStatusTypeId);
            csvWriter.WriteField(route?.Name);
            csvWriter.WriteField(r.ProfessionalStatus?.TrainingAgeSpecialismType);
            csvWriter.WriteField(r.ProfessionalStatus?.TrainingAgeSpecialismRangeFrom);
            csvWriter.WriteField(r.ProfessionalStatus?.TrainingAgeSpecialismRangeTo);
            csvWriter.WriteField(r.ProfessionalStatus?.Status.ToString());
            csvWriter.WriteField(r.ProfessionalStatus?.HoldsFrom?.ToString("dd/MM/yyyy"));
            csvWriter.WriteField(degreeType?.Name);
            csvWriter.WriteField(trainingProvider?.Name);
            csvWriter.WriteField(trainingCountry?.CountryId);
            csvWriter.WriteField(JsonSerializer.Serialize(trainingSubjects.Select(s => s.Reference).ToArray()));
            csvWriter.WriteField(r.TeacherStatus?.dfeta_Value);
            csvWriter.WriteField(r.TeacherStatus?.dfeta_name);
            csvWriter.WriteField(r.QtsDate?.ToString("dd/MM/yyyy"));
            csvWriter.WriteField(r.EarlyYearsStatus?.dfeta_Value);
            csvWriter.WriteField(r.EarlyYearsStatus?.dfeta_name);
            csvWriter.WriteField(r.EytsDate?.ToString("dd/MM/yyyy"));
            csvWriter.WriteField(r.PartialRecognitionDate?.ToString("dd/MM/yyyy"));
            csvWriter.WriteField(r.QtlsDate?.ToString("dd/MM/yyyy"));
            csvWriter.WriteField(r.ProgrammeType);
            csvWriter.WriteField(r.ProfessionalStatus?.DqtAgeRangeFrom);
            csvWriter.WriteField(r.ProfessionalStatus?.DqtAgeRangeTo);
            csvWriter.WriteField(r.IttResult);
            csvWriter.WriteField(r.IttQualification?.dfeta_Value);
            csvWriter.WriteField(r.IttQualification?.dfeta_name);
            csvWriter.WriteField(ittProvider?.Name);
            csvWriter.WriteField(ittCountry?.dfeta_Value);
            csvWriter.WriteField(ittSubject1?.dfeta_Value);
            csvWriter.WriteField(ittSubject2?.dfeta_Value);
            csvWriter.WriteField(ittSubject3?.dfeta_Value);
            csvWriter.WriteField(r.StatusDerivedRouteId);
            csvWriter.WriteField(statusDerivedRoute?.Name);
            csvWriter.WriteField(r.ProgrammeTypeDerivedRouteId);
            csvWriter.WriteField(programmeTypeDerivedRoute?.Name);
            csvWriter.WriteField(r.IttQualificationDerivedRouteId);
            csvWriter.WriteField(ittQualificationDerivedRoute?.Name);
            csvWriter.WriteField(JsonSerializer.Serialize(r.MultiplePotentialCompatibleIttRecords));
            csvWriter.WriteField(r.ContactIttRowCount);
            csvWriter.WriteField(r.ContactQtsRowCount);
            csvWriter.NextRecord();
        }

        await csvWriter.FlushAsync();
    }
}
