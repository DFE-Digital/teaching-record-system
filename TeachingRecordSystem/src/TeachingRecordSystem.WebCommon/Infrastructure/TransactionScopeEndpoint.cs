using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TeachingRecordSystem.WebCommon.Infrastructure;

public class TransactionScopeEndpointConventions : IPageApplicationModelConvention
{
    public void Apply(PageApplicationModel model)
    {
        if (model.HandlerTypeAttributes.OfType<TransactionScopeAttribute>().Any())
        {
            model.EndpointMetadata.Add(TransactionScopeEndpointMetadataMarker.Instance);
        }
    }
}

internal sealed class TransactionScopeEndpointMetadataMarker
{
    private TransactionScopeEndpointMetadataMarker() { }

    public static TransactionScopeEndpointMetadataMarker Instance { get; } = new();
}

public class TransactionScopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<TransactionScopeEndpointMetadataMarker>() is not null)
        {
            using var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
            await next(context);
            scope.Complete();
        }
        else
        {
            await next(context);
        }
    }
}
