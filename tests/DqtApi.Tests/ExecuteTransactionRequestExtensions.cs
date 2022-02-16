using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;
using System.Linq;

namespace DqtApi.Tests
{
    public static class ExecuteTransactionRequestExtensions
    {
        public static TRequest AssertSingle<TRequest>(this ExecuteTransactionRequest request)
            where TRequest : OrganizationRequest
        {
            return (TRequest)Assert.Single(request.Requests, request => request is TRequest);
        }

        public static TRequest AssertSingle<TRequest>(this ExecuteTransactionRequest request, Predicate<TRequest> predicate)
            where TRequest : OrganizationRequest
        {
            return (TRequest)Assert.Single(request.Requests, request => request is TRequest req && predicate(req));
        }

        public static TEntity AssertSingleCreateRequest<TEntity>(this ExecuteTransactionRequest request)
            where TEntity : Entity
        {
            var createRequest = (CreateRequest)Assert.Single(
                request.Requests,
                request => request is CreateRequest createRequest && createRequest.Target is TEntity);

            return (TEntity)createRequest.Target;
        }

        public static void AssertDoesNotContainCreateRequest<TEntity>(this ExecuteTransactionRequest request)
            where TEntity : Entity
        {
            Assert.DoesNotContain(
                request.Requests,
                request => request is CreateRequest createRequest && createRequest.Target is TEntity);
        }

        public static TEntity AssertSingleUpdateRequest<TEntity>(this ExecuteTransactionRequest request)
            where TEntity : Entity
        {
            var updateRequest = (UpdateRequest)Assert.Single(
                request.Requests,
                request => request is UpdateRequest updateRequest && updateRequest.Target is TEntity);

            return (TEntity)updateRequest.Target;
        }

        public static void AssertDoesNotContainUpdateRequest<TEntity>(this ExecuteTransactionRequest request)
            where TEntity : Entity
        {
            Assert.DoesNotContain(
                request.Requests,
                request => request is UpdateRequest updateRequest && updateRequest.Target is TEntity);
        }
    }
}
