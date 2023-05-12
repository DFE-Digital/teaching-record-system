using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using Xunit;

namespace QualifiedTeachersApi.Tests.Services;

public class LinkTrnToIdentityUserServiceTests : ApiTestBase
{
    public LinkTrnToIdentityUserServiceTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task TeacherNotFoundInCrm_LogsError()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var identityUserId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();

        var logger = new Mock<ILogger<LinkTrnToIdentityUserService>>();
        var apiClient = new Mock<IGetAnIdentityApiClient>();
        var dataverseAdapter = new Mock<IDataverseAdapter>();

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId,
                LinkedToIdentity = false,
                IdentityUserId = identityUserId
            });

            await dbContext.SaveChangesAsync();
        });

        var service = new LinkTrnToIdentityUserService(ApiFixture.Services, logger.Object);

        // Act
        await service.AssociateTrnsNotLinkedToIdentities();

        // Assert
        logger.Verify(x => x.Log(
                          LogLevel.Error,
                          It.IsAny<EventId>(),
                          It.Is<It.IsAnyType>((o, t) => string.Equals($"{teacherId} teacher not found!", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                          It.IsAny<Exception>(),
                          (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                      Times.Once);
    }

    [Fact]
    public async Task IdentityApiCallThrows_LogsError()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var identityUserId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();
        var trn = "1234567";
        var contact = new Contact() { dfeta_TRN = trn, Id = teacherId };

        var logger = new Mock<ILogger<LinkTrnToIdentityUserService>>();
        ApiFixture.IdentityApiClient.Setup(x => x.SetTeacherTrn(It.IsAny<Guid>(), It.IsAny<string>())).Throws(new Exception());
        ApiFixture.DataverseAdapter.Setup(x => x.GetTeacher(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(contact);

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId,
                LinkedToIdentity = false,
                IdentityUserId = identityUserId
            });

            await dbContext.SaveChangesAsync();
        });

        var service = new LinkTrnToIdentityUserService(ApiFixture.Services, logger.Object);

        // Act
        await service.AssociateTrnsNotLinkedToIdentities();

        // Assert
        logger.Verify(x => x.Log(
                          LogLevel.Error,
                          It.IsAny<EventId>(),
                          It.Is<It.IsAnyType>((o, t) => string.Equals($"Error occurred while linking an identity {identityUserId} to {contact.dfeta_TRN}", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                          It.IsAny<Exception>(),
                          (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                      Times.Once);
    }

    [Fact]
    public async Task CallsIdentityApiForEachUnlinkedRecord()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        var teacherId1 = Guid.NewGuid();
        var identityUserId1 = Guid.NewGuid();
        var requestId1 = Guid.NewGuid().ToString();
        var trn1 = "1234567";
        var contact1 = new Contact() { dfeta_TRN = trn1, Id = teacherId1 };

        var teacherId2 = Guid.NewGuid();
        var identityUserId2 = Guid.NewGuid();
        var requestId2 = Guid.NewGuid().ToString();
        var trn2 = "1234567";
        var contact2 = new Contact() { dfeta_TRN = trn2, Id = teacherId2 };

        var logger = new Mock<ILogger<LinkTrnToIdentityUserService>>();
        ApiFixture.DataverseAdapter.Setup(x => x.GetTeacher(teacherId1, It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(contact1);
        ApiFixture.DataverseAdapter.Setup(x => x.GetTeacher(teacherId2, It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(contact2);

        await WithDbContext(async dbContext =>
        {
            dbContext.AddRange(
                new TrnRequest()
                {
                    ClientId = ClientId,
                    RequestId = requestId1,
                    TeacherId = teacherId1,
                    LinkedToIdentity = false,
                    IdentityUserId = identityUserId1
                },
                new TrnRequest()
                {
                    ClientId = ClientId,
                    RequestId = requestId2,
                    TeacherId = teacherId2,
                    LinkedToIdentity = false,
                    IdentityUserId = identityUserId2
                });

            await dbContext.SaveChangesAsync();
        });

        var service = new LinkTrnToIdentityUserService(ApiFixture.Services, logger.Object);

        // Act
        await service.AssociateTrnsNotLinkedToIdentities();

        // Assert
        ApiFixture.IdentityApiClient.Verify(x => x.SetTeacherTrn(identityUserId1, trn1));
        ApiFixture.IdentityApiClient.Verify(x => x.SetTeacherTrn(identityUserId2, trn2));
    }
}
