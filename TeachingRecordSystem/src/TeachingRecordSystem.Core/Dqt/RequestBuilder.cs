using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt;

/// <summary>
/// A helper type for composing CRM requests.
/// </summary>
/// <example>
/// <code>
/// var builder = MultiRequestBuilder.CreateTransaction(organizationService);
/// var request1 = builder.AddRequest(new CreateRequest());
/// var request2 = builder.AddRequest(new UpdateRequest());
/// await builder.Execute();
/// var response1 = request1.GetResponse();
/// var response2 = request2.GetResponse();
/// </code>
/// </example>
public class RequestBuilder
{
    private readonly IOrganizationServiceAsync _organizationService;
    private readonly RequestType _requestType;
    private readonly List<OrganizationRequest> _requests;
    private OrganizationResponse[]? _responses;
    private readonly TaskCompletionSource _completedTcs;

    private RequestBuilder(IOrganizationServiceAsync organizationService, RequestType requestType)
    {
        _organizationService = organizationService;
        _requestType = requestType;
        _requests = new();
        _completedTcs = new();
    }

    public static RequestBuilder CreateSingle(IOrganizationServiceAsync organizationService) =>
        new(organizationService, RequestType.Single);

    public static RequestBuilder CreateMultiple(IOrganizationServiceAsync organizationService) =>
        new(organizationService, RequestType.Multiple);

    public static RequestBuilder CreateTransaction(IOrganizationServiceAsync organizationService) =>
        new(organizationService, RequestType.Transaction);

    public void AddRequest(OrganizationRequest request) => AddRequest<OrganizationResponse>(request);

    public IInnerRequestHandle<TResponse> AddRequest<TResponse>(OrganizationRequest request)
        where TResponse : OrganizationResponse
    {
        ThrowIfCompleted();

        if (_requestType == RequestType.Single && _requests.Count != 0)
        {
            throw new InvalidOperationException($"Only a single request can be specified when {nameof(RequestType)} is {nameof(RequestType.Single)}.");
        }

        _requests.Add(request);

        if (_requestType == RequestType.Single)
        {
            _ = Execute();
        }

        return new InnerRequestHandle<TResponse>(this, request);
    }

    public void AddRequests(params OrganizationRequest[] requests)
    {
        foreach (var request in requests)
        {
            AddRequest<OrganizationResponse>(request);
        }
    }

    public async Task Execute()
    {
        ThrowIfCompleted();

        var executeTask = _requestType switch
        {
            RequestType.Single => ExecuteSingleRequest(),
            RequestType.Multiple => ExecuteMultipleRequest(),
            RequestType.Transaction => ExecuteTransactionRequest(),
            _ => throw new NotSupportedException($"Unknown {nameof(RequestType)}: '{_requestType}'.")
        };

        await executeTask;
        await _completedTcs.Task;

        async Task ExecuteSingleRequest()
        {
            if (_requests.Count == 0)
            {
                throw new InvalidOperationException("No request has been specified.");
            }

            try
            {
                var request = _requests.Single();
                var response = await _organizationService.ExecuteAsync(request);
                _responses = new[] { response };

                _completedTcs.SetResult();
            }
            catch (Exception ex)
            {
                _completedTcs.SetException(ex);
            }
        }

        async Task ExecuteMultipleRequest()
        {
            if (_requests.Count == 0)
            {
                _responses = Array.Empty<OrganizationResponse>();
                _completedTcs.SetResult();
                return;
            }

            var request = new ExecuteMultipleRequest()
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new()
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                }
            };

            request.Requests.AddRange(_requests);

            try
            {
                var response = (ExecuteMultipleResponse)await _organizationService.ExecuteAsync(request);
                _responses = response.Responses.Select(r => r.Response).ToArray();

                _completedTcs.SetResult();
            }
            catch (Exception ex)
            {
                _completedTcs.SetException(ex);
            }
        }

        async Task ExecuteTransactionRequest()
        {
            if (_requests.Count == 0)
            {
                _responses = Array.Empty<OrganizationResponse>();
                _completedTcs.SetResult();
                return;
            }

            var request = new ExecuteTransactionRequest()
            {
                Requests = new OrganizationRequestCollection(),
                ReturnResponses = true
            };

            request.Requests.AddRange(_requests);

            try
            {
                var response = (ExecuteTransactionResponse)await _organizationService.ExecuteAsync(request);
                _responses = response.Responses.ToArray();

                _completedTcs.SetResult();
            }
            catch (Exception ex)
            {
                _completedTcs.SetException(ex);
            }
        }
    }

    private void ThrowIfCompleted()
    {
        if (_completedTcs.Task.IsCompleted)
        {
            throw new InvalidOperationException("Request has already been executed.");
        }
    }

    public interface IInnerRequestHandle<TResponse>
        where TResponse : OrganizationResponse
    {
        TResponse GetResponse();
        Task<TResponse> GetResponseAsync();
    }

    private class InnerRequestHandle<TResponse> : IInnerRequestHandle<TResponse>
        where TResponse : OrganizationResponse
    {
        private readonly RequestBuilder _requestBuilder;
        private readonly OrganizationRequest _request;

        public InnerRequestHandle(RequestBuilder requestBuilder, OrganizationRequest request)
        {
            _requestBuilder = requestBuilder;
            _request = request;
        }

        public TResponse GetResponse()
        {
            if (!_requestBuilder._completedTcs.Task.IsCompleted)
            {
                throw new InvalidOperationException("Request has not been executed.");
            }

            if (_requestBuilder._completedTcs.Task.IsFaulted)
            {
                throw _requestBuilder._completedTcs.Task.Exception!;
            }

            var index = _requestBuilder._requests.IndexOf(_request);
            var response = _requestBuilder._responses![index];
            return (TResponse)response;
        }

        public Task<TResponse> GetResponseAsync() => _requestBuilder._completedTcs.Task.ContinueWith(_ => GetResponse());
    }

    private enum RequestType { Single, Transaction, Multiple }
}
