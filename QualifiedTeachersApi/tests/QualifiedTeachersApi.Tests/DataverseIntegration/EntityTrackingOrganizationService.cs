#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public interface ITrackedEntityOrganizationService : IOrganizationServiceAsync2, IAsyncDisposable
{
    Task DeleteCreatedEntities();
}

public static class EntityTrackingOrganizationService
{
    public static ITrackedEntityOrganizationService CreateProxy(ServiceClient serviceClient)
    {
        var generator = new ProxyGenerator();

        return generator.CreateInterfaceProxyWithoutTarget<ITrackedEntityOrganizationService>(
            new RecordTrackedEntitiesInterceptor(serviceClient));
    }

    private class RecordTrackedEntitiesInterceptor : IInterceptor
    {
        private static readonly MethodInfo _disposeAsyncMethod = typeof(IAsyncDisposable).GetMethod(nameof(IAsyncDisposable.DisposeAsync));
        private static readonly MethodInfo _deleteCreatedEntitiesMethod = typeof(ITrackedEntityOrganizationService).GetMethod(nameof(ITrackedEntityOrganizationService.DeleteCreatedEntities));

        private static readonly MethodInfo _createMethod = typeof(IOrganizationService).GetMethod(nameof(IOrganizationService.Create));
        private static readonly MethodInfo _createAsyncMethod = typeof(IOrganizationServiceAsync).GetMethod(nameof(IOrganizationServiceAsync.CreateAsync), new[] { typeof(Entity) });
        private static readonly MethodInfo _createAsync2Method = typeof(IOrganizationServiceAsync2).GetMethod(nameof(IOrganizationServiceAsync2.CreateAsync), new[] { typeof(Entity), typeof(CancellationToken) });

        private readonly List<EntityReference> _createdEntities = new();
        private readonly ServiceClient _serviceClient;
        private bool _disposed = false;

        public RecordTrackedEntitiesInterceptor(ServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == _disposeAsyncMethod)
            {
#pragma warning disable CA2012 // Use ValueTasks correctly
                invocation.ReturnValue = DisposeAsync();
#pragma warning restore CA2012 // Use ValueTasks correctly
            }
            else if (invocation.Method == _deleteCreatedEntitiesMethod)
            {
                invocation.ReturnValue = DeleteCreatedEntities();
            }
            else
            {
                var result = invocation.Method.Invoke(_serviceClient, invocation.Arguments);

                var resultType = invocation.Method.ReturnType;
                var isAsyncResult = resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>);

                if (!isAsyncResult)
                {
                    ExtractCreatedEntitiesFromInvocation(invocation, result);
                }
                else
                {
                    result = GetType().GetMethod(nameof(CreateExtractCreatedEntitiesContinuation), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(resultType.GetGenericArguments()[0])
                        .Invoke(this, new object[] { result, invocation });
                }

                invocation.ReturnValue = result;
            }

            Task DeleteCreatedEntities()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(typeof(ITrackedEntityOrganizationService).Name);
                }

                return DeleteCreatedEntitiesCore();
            }

            async ValueTask DisposeAsync()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                await DeleteCreatedEntitiesCore();
            }
        }

        private Task<T> CreateExtractCreatedEntitiesContinuation<T>(Task result, IInvocation invocation) =>
            ((Task<T>)result).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    var asyncResult = t.Result;
                    ExtractCreatedEntitiesFromInvocation(invocation, asyncResult);
                    return Task.FromResult(asyncResult);
                }
                else
                {
                    return t;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();

        private void ExtractCreatedEntitiesFromInvocation(IInvocation invocation, object underlyingResult)
        {
            if (underlyingResult is OrganizationResponse organizationResponse)
            {
                var request = (OrganizationRequest)invocation.Arguments[0];
                ExtractCreatedEntitiesFromMessages(request, organizationResponse);
            }
            else if (invocation.Method == _createMethod || invocation.Method == _createAsyncMethod || invocation.Method == _createAsync2Method)
            {
                var entity = (Entity)invocation.Arguments[0];
                var entityId = (Guid)underlyingResult;
                var entityReference = new EntityReference(entity.LogicalName, entityId);

                _createdEntities.Add(entityReference);
            }
        }

        private void ExtractCreatedEntitiesFromMessages(OrganizationRequest request, OrganizationResponse response)
        {
            switch (response)
            {
                case CreateEntityResponse createEntityResponse:
                    {
                        var entityReference = new EntityReference(((CreateEntityRequest)request).Entity.LogicalName, createEntityResponse.EntityId);
                        _createdEntities.Add(entityReference);
                        break;
                    }

                case CreateResponse createResponse:
                    {
                        var entityReference = new EntityReference(((CreateRequest)request).Target.LogicalName, createResponse.id);
                        _createdEntities.Add(entityReference);
                        break;
                    }

                case ExecuteTransactionResponse executeTransactionResponse:
                    {
                        var executeTransactionRequest = (ExecuteTransactionRequest)request;

                        if (executeTransactionRequest.ReturnResponses != true)
                        {
                            throw new NotSupportedException(
                                $"Cannot track created entities for an {nameof(ExecuteTransactionRequest)} unless {nameof(ExecuteTransactionRequest.ReturnResponses)} is {true}.");
                        }

                        for (var i = 0; i < executeTransactionRequest.Requests.Count; i++)
                        {
                            var innerRequest = executeTransactionRequest.Requests[i];
                            var innerResponse = executeTransactionResponse.Responses[i];

                            ExtractCreatedEntitiesFromMessages(innerRequest, innerResponse);
                        }

                        break;
                    }

                case ExecuteMultipleResponse executeMultipleResponse:
                    {
                        var executeMultipleRequest = (ExecuteMultipleRequest)request;

                        if (executeMultipleRequest.Settings.ReturnResponses != true)
                        {
                            throw new NotSupportedException(
                                $"Cannot track created entities for an {nameof(ExecuteMultipleRequest)} unless {nameof(ExecuteMultipleRequest.Settings.ReturnResponses)} is {true}.");
                        }

                        for (var i = 0; i < executeMultipleRequest.Requests.Count; i++)
                        {
                            if (executeMultipleResponse.Responses[i].Fault != null)
                            {
                                continue;
                            }

                            var innerRequest = executeMultipleRequest.Requests[i];
                            var innerResponse = executeMultipleResponse.Responses[i].Response;

                            ExtractCreatedEntitiesFromMessages(innerRequest, innerResponse);
                        }

                        break;
                    }
            }
        }

        private Task DeleteCreatedEntitiesCore()
        {
            EntityReference[] toBeCleared;
            lock (_createdEntities)
            {
                toBeCleared = _createdEntities.ToArray();
                _createdEntities.Clear();
            }

            var multiRequest = new ExecuteMultipleRequest()
            {
                Requests = new(),
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true
                }
            };

            foreach (var entity in toBeCleared)
            {
                multiRequest.Requests.Add(new DeleteRequest()
                {
                    Target = entity
                });
            }

            return _serviceClient.ExecuteAsync(multiRequest);
        }
    }
}
