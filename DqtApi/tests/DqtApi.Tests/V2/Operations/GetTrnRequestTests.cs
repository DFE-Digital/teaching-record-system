﻿using System;
using System.Net;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql.Models;
using DqtApi.TestCommon;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
    public class GetTrnRequestTests : ApiTestBase
    {
        public GetTrnRequestTests(ApiFixture apiFixture) : base(apiFixture)
        {
        }

        [Fact]
        public async Task Given_trn_request_with_specified_id_does_not_exist_returns_notfound()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();

            // Act
            var response = await HttpClient.GetAsync($"v2/trn-requests/{requestId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Given_trn_request_with_specified_id_does_not_exist_for_current_client_returns_notfound()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();

            var anotherClientId = "ANOTHER-CLIENT";
            Assert.NotEqual(ClientId, anotherClientId);

            await WithDbContext(async dbContext =>
            {
                dbContext.Add(new TrnRequest()
                {
                    ClientId = anotherClientId,
                    RequestId = requestId
                });

                await dbContext.SaveChangesAsync();
            });

            // Act
            var response = await HttpClient.GetAsync($"v2/trn-requests/{requestId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Given_valid_pending_trn_request_returns_ok_with_pending_status()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();

            await WithDbContext(async dbContext =>
            {
                dbContext.Add(new TrnRequest()
                {
                    ClientId = ClientId,
                    RequestId = requestId,
                    TeacherId = null
                });

                await dbContext.SaveChangesAsync();
            });

            // Act
            var response = await HttpClient.GetAsync($"v2/trn-requests/{requestId}");

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    requestId = requestId,
                    status = "Pending",
                    trn = (string)null
                },
                expectedStatusCode: StatusCodes.Status200OK);
        }

        [Fact]
        public async Task Given_valid_completed_trn_request_returns_ok_with_completed_status_and_trn()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var teacherId = Guid.NewGuid();
            var trn = "1234567";

            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ true, It.IsAny<string[]>()))
                .ReturnsAsync(new Contact()
                {
                    Id = teacherId,
                    dfeta_TRN = trn
                });

            await WithDbContext(async dbContext =>
            {
                dbContext.Add(new TrnRequest()
                {
                    ClientId = ClientId,
                    RequestId = requestId,
                    TeacherId = teacherId
                });

                await dbContext.SaveChangesAsync();
            });

            // Act
            var response = await HttpClient.GetAsync($"v2/trn-requests/{requestId}");

            // Assert
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    requestId = requestId,
                    status = "Completed",
                    trn = trn
                },
                expectedStatusCode: StatusCodes.Status200OK);
        }
    }
}
