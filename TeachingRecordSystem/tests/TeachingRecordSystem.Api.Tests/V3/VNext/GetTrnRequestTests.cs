namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.CreateTrn]);
    }

    [Fact]
    public async Task Get_CompletedRequestWithOneLoginUserMetadata_AddsOneLoginUserToDb()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithTrnRequestId(TrnRequestHelper.GetCrmTrnRequestId(ApplicationUserId, requestId)));

        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        await WithDbContext(dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(new Core.DataStore.Postgres.Models.TrnRequestMetadata()
            {
                ApplicationUserId = ApplicationUserId,
                RequestId = requestId,
                VerifiedOneLoginUserSubject = oneLoginUserSubject
            });

            return dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestId={requestId}");

        // Assert
        await AssertEx.JsonResponse(response, expectedStatusCode: 200);

        await WithDbContext(async dbContext =>
        {
            var oneLoginUser = await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == oneLoginUserSubject);
            Assert.NotNull(oneLoginUser);
            Assert.Equal(person.PersonId, oneLoginUser.PersonId);
            Assert.Equal(Clock.UtcNow, oneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.External, oneLoginUser.VerificationRoute);
            Assert.Equal(ApplicationUserId, oneLoginUser.VerifiedByApplicationUserId);
        });
    }

    [Fact]
    public async Task Get_PendingRequestWithOneLoginUserMetadata_DoesNotAddOneLoginUserToDb()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePerson(p => p
            .WithoutTrn()
            .WithTrnRequestId(TrnRequestHelper.GetCrmTrnRequestId(ApplicationUserId, requestId)));

        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        await WithDbContext(dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(new Core.DataStore.Postgres.Models.TrnRequestMetadata()
            {
                ApplicationUserId = ApplicationUserId,
                RequestId = requestId,
                VerifiedOneLoginUserSubject = oneLoginUserSubject
            });

            return dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestId={requestId}");

        // Assert
        await AssertEx.JsonResponse(response, expectedStatusCode: 200);

        await WithDbContext(async dbContext =>
        {
            var oneLoginUser = await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == oneLoginUserSubject);
            Assert.Null(oneLoginUser);
        });
    }
}
