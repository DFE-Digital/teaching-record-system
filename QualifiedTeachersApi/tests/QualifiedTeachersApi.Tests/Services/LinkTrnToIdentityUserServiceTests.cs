using System;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Services;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using Xunit;

namespace QualifiedTeachersApi.Tests.Services
{
    public class LinkTrnToIdentityUserServiceTests : ApiTestBase
    {
        public LinkTrnToIdentityUserServiceTests(ApiFixture apiFixture)
            : base(apiFixture)
        {
        }

        [Fact]
        public async Task Given_teacher_not_found_in_crm_logger_error()
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
            LinkTrnToIdentityUserService service = new LinkTrnToIdentityUserService(dataverseAdapter.Object, logger.Object, ApiFixture.Services.GetRequiredService<IServiceProvider>(), apiClient.Object);

            // Act
            await service.AssociateTrnsNotLinkedToIdentities();

            // Assert
            logger.Verify(x => x.Log(
                              LogLevel.Error,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((o, t) => string.Equals($"{teacherId} teacher not found!", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                              It.IsAny<Exception>(),
                              (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                          Times.Once);
        }

        [Fact]
        public async Task Given_apiclient_returns_error_error_an_error_is_logged()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var identityUserId = Guid.NewGuid();
            var requestId = Guid.NewGuid().ToString();
            var trn = "1234567";
            var contact = new Contact() { dfeta_TRN = trn, Id = teacherId };

            var logger = new Mock<ILogger<LinkTrnToIdentityUserService>>();
            var apiClient = new Mock<IGetAnIdentityApiClient>();
            apiClient.Setup(x => x.SetTeacherTrn(It.IsAny<Guid>(), It.IsAny<string>())).Throws(new Exception());
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            dataverseAdapter.Setup(x => x.GetTeacher(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(contact);
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
            LinkTrnToIdentityUserService service = new LinkTrnToIdentityUserService(dataverseAdapter.Object, logger.Object, ApiFixture.Services.GetRequiredService<IServiceProvider>(), apiClient.Object);

            // Act
            await service.AssociateTrnsNotLinkedToIdentities();

            // Assert
            logger.Verify(x => x.Log(
                              LogLevel.Error,
                              It.IsAny<EventId>(),
                              It.Is<It.IsAnyType>((o, t) => string.Equals($"Error occurred while linking an identity {identityUserId} to {contact.dfeta_TRN}", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                              It.IsAny<Exception>(),
                              (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                          Times.Once);
        }

        [Fact]
        public async Task Given_multiple_records_require_linking_SetTeacherTrn_invocation_matches()
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
            var apiClient = new Mock<IGetAnIdentityApiClient>();
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            dataverseAdapter.Setup(x => x.GetTeacher(teacherId1, It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(contact1);
            dataverseAdapter.Setup(x => x.GetTeacher(teacherId2, It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(contact2);
            await WithDbContext(async dbContext =>
            {
                dbContext.AddRange(new TrnRequest()
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
            LinkTrnToIdentityUserService service = new LinkTrnToIdentityUserService(dataverseAdapter.Object, logger.Object, ApiFixture.Services.GetRequiredService<IServiceProvider>(), apiClient.Object);

            // Act
            await service.AssociateTrnsNotLinkedToIdentities();

            // Assert
            apiClient.Verify(x => x.SetTeacherTrn(identityUserId1, trn1));
            apiClient.Verify(x => x.SetTeacherTrn(identityUserId2, trn2));
        }
    }
}
