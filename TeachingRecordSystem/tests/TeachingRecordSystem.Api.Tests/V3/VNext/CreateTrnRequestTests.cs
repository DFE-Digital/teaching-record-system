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
    public async Task Post_CreatesOutboxMessageInCrm()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var email = TestData.GenerateUniqueEmail();
        var identityVerified = true;
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = CreateJsonContent(new
            {
                requestId = requestId,
                person = new
                {
                    firstName = firstName,
                    middleName = middleName,
                    lastName = lastName,
                    dateOfBirth = dateOfBirth,
                    emailAddresses = new[] { email }
                },
                identityVerified = identityVerified,
                oneLoginUserSubject = oneLoginUserSubject
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response, expectedStatusCode: StatusCodes.Status200OK);

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
                Assert.Equal(Clock.UtcNow, message.CreatedOn);
                Assert.Equal(email, message.EmailAddress);
                Assert.Equal(identityVerified, message.IdentityVerified);
                Assert.Equal(oneLoginUserSubject, message.OneLoginUserSubject);
                Assert.Equal(new[] { firstName, middleName, lastName }, message.Name);
                Assert.Equal(dateOfBirth, message.DateOfBirth);
            });
    }
}
