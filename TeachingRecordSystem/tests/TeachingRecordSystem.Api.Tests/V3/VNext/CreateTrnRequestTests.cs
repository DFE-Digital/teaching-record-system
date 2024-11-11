using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);
        XrmFakedContext.DeleteAllEntities<Contact>();
    }

    [Fact]
    public async Task Post_CompletedRequestWithVerifiedOneLoginUserId_CreatesOutboxMessageInCrm()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = CreateJsonContent(new
            {
                requestId = requestId,
                person = new
                {
                    firstName = TestData.GenerateFirstName(),
                    middleName = TestData.GenerateMiddleName(),
                    lastName = TestData.GenerateLastName(),
                    dateOfBirth = TestData.GenerateDateOfBirth(),
                },
                verifiedOneLoginUserSubject = oneLoginUserSubject
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal("Completed", jsonResponse.RootElement.GetProperty("status").GetString());

        var (crmQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        Assert.Collection(
            crmQuery.OutboxMessages,
            outboxMessage =>
            {
                Assert.Equal(nameof(TrnRequestMetadataMessage), outboxMessage.dfeta_MessageName);

                var messageSerializer = HostFixture.Services.GetRequiredService<MessageSerializer>();
                var message = Assert.IsType<TrnRequestMetadataMessage>(messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName));
                Assert.Equal(ApplicationUserId, message.ApplicationUserId);
                Assert.Equal(requestId, message.RequestId);
                Assert.Equal(oneLoginUserSubject, message.VerifiedOneLoginUserSubject);
            });
    }

    [Fact]
    public async Task Post_PendingRequestWithVerifiedOneLoginUserId_CreatesOutboxMessageInCrm()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var existingPerson = await TestData.CreatePerson(p => p.WithTrn());
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = CreateJsonContent(new
            {
                requestId = requestId,
                person = new
                {
                    firstName = existingPerson.FirstName,
                    middleName = existingPerson.MiddleName,
                    lastName = existingPerson.LastName,
                    dateOfBirth = existingPerson.DateOfBirth
                },
                verifiedOneLoginUserSubject = oneLoginUserSubject
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal("Pending", jsonResponse.RootElement.GetProperty("status").GetString());

        var (crmQuery, _) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        Assert.Collection(
            crmQuery.OutboxMessages,
            outboxMessage =>
            {
                Assert.Equal(nameof(TrnRequestMetadataMessage), outboxMessage.dfeta_MessageName);

                var messageSerializer = HostFixture.Services.GetRequiredService<MessageSerializer>();
                var message = Assert.IsType<TrnRequestMetadataMessage>(messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName));
                Assert.Equal(ApplicationUserId, message.ApplicationUserId);
                Assert.Equal(requestId, message.RequestId);
                Assert.Equal(oneLoginUserSubject, message.VerifiedOneLoginUserSubject);
            });
    }
}
