using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.FakeMessageExecutors;
using TeachingRecordSystem.TestCommon.Infrastructure.FakeXrmEasy.Plugins;

namespace TeachingRecordSystem.TestCommon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFakeXrm(this IServiceCollection services)
    {
        var operationLock = new object();

        var fakedXrmContext = MiddlewareBuilder
            .New()
            .AddCrud()
            .AddFakeMessageExecutors(Assembly.GetAssembly(typeof(ExecuteTransactionExecutor)))
            .AddFakeMessageExecutor<CloseIncidentRequest>(new WorkaroundCloseIncidentRequestExecutor())
            .AddPipelineSimulation()
            .Use(next => (IXrmFakedContext context, OrganizationRequest request) =>
            {
                lock (operationLock)
                {
                    return next(context, request);
                }
            })
            .UsePipelineSimulation()
            .UseCrud()
            .UseMessages()
            .SetLicense(FakeXrmEasyLicense.NonCommercial)
            .Build();

        fakedXrmContext.EnableProxyTypes(typeof(Contact).Assembly);
        fakedXrmContext.InitializeMetadata(typeof(Contact).Assembly);

        AssignTicketNumberToIncidentPlugin.Register(fakedXrmContext);
        PersonNameChangedPlugin.Register(fakedXrmContext);
        CalculateActiveSanctionsPlugin.Register(fakedXrmContext);
        QtsRegistrationUpdatedPlugin.Register(fakedXrmContext);
        UpdateInductionStatusPlugin.Register(fakedXrmContext);
        UpdateQtlsDateSetPlugin.Register(fakedXrmContext);


        // SeedCrmReferenceData must be registered before AddDefaultServiceClient is called
        // to ensure this task runs before the cache pre-warming task
        services.AddStartupTask<SeedCrmReferenceData>();

        services.AddSingleton<IXrmFakedContext>(fakedXrmContext);
        var organizationService = fakedXrmContext.GetAsyncOrganizationService2();
        services.AddDefaultServiceClient(ServiceLifetime.Singleton, _ => organizationService);

        fakedXrmContext.CallerProperties.CallerId = CreateTestUser();

        services.AddSingleton<SeedCrmReferenceData>();

        return services;

        EntityReference CreateTestUser()
        {
            // User must be assigned to a BusinessUnit, otherwise the WhoAmIRequest will not work

            var businessUnit = new BusinessUnit()
            {
                Id = Guid.NewGuid()
            };
            organizationService.Create(businessUnit);

            var systemUser = new SystemUser()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                BusinessUnitId = businessUnit.Id.ToEntityReference(BusinessUnit.EntityLogicalName)
            };
            organizationService.Create(systemUser);

            return systemUser.ToEntityReference();
        }
    }
}
