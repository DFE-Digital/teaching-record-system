using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace QualifiedTeachersApi.Tests
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

        public static TEntity AssertSingleUpsertRequest<TEntity>(this ExecuteTransactionRequest request)
            where TEntity : Entity
        {
            var upsertRequest = (UpsertRequest)Assert.Single(
            request.Requests,
            request => request is UpsertRequest upsertRequest && upsertRequest.Target is TEntity);

            return (TEntity)upsertRequest.Target;
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

        public static void AssertDoesNotContainUpsertRequest<TEntity>(this ExecuteTransactionRequest request)
            where TEntity : Entity
        {
            Assert.DoesNotContain(
                request.Requests,
                request => request is UpsertRequest upsertRequest && upsertRequest.Target is TEntity);
        }

        public static void AssertContainsRequest<TRequest>(this ExecuteTransactionRequest request, Predicate<TRequest> filter)
        {
            var requests = request.Requests.OfType<TRequest>();
            Assert.Contains(requests, filter);
        }

        public static void AssertContainsCreateRequest<TEntity>(this ExecuteTransactionRequest request, Predicate<TEntity> filter)
        {
            AssertContainsRequest<CreateRequest>(request, request => request.Target is TEntity entity && filter(entity));
        }
    }
}
