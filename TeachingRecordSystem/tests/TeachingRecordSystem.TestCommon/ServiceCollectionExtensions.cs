using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

namespace TeachingRecordSystem.TestCommon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakeXrm(this IServiceCollection services)
    {
        var fakedXrmContext = MiddlewareBuilder
            .New()
            .AddCrud()
            .AddFakeMessageExecutors(Assembly.GetAssembly(typeof(ExecuteTransactionExecutor)))
            .AddPipelineSimulation()
            .UsePipelineSimulation()
            .UseCrud()
            .UseMessages()
            .SetLicense(FakeXrmEasyLicense.NonCommercial)
            .Build();

        fakedXrmContext.EnableProxyTypes(typeof(Contact).Assembly);
        fakedXrmContext.InitializeMetadata(typeof(Contact).Assembly);

        AssignTicketNumberToIncidentPlugin.Register(fakedXrmContext);

        services.AddSingleton<IXrmFakedContext>(fakedXrmContext);
        services.AddSingleton<IOrganizationServiceAsync>(fakedXrmContext.GetAsyncOrganizationService());

        services.AddSingleton<SeedCrmReferenceData>();
        services.AddStartupTask<SeedCrmReferenceData>();

        return services;
    }
}
