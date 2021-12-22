using System;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetTeacherTests : IClassFixture<CrmClientFixture>
    {
        private readonly DataverseAdaptor _dataverseAdaptor;
        private readonly ServiceClient _serviceClient;

        public GetTeacherTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdaptor = crmClientFixture.CreateDataverseAdaptor();
            _serviceClient = crmClientFixture.ServiceClient;
        }

        [Fact]
        public async Task Given_teacher_that_does_not_exist_returns_null()
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            // Act
            var result = await _dataverseAdaptor.GetTeacherAsync(teacherId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Given_teacher_that_exists_returns_teacher()
        {
            // Arrange
            var teacherId = await _serviceClient.CreateAsync(new Contact());

            // Act
            var result = await _dataverseAdaptor.GetTeacherAsync(teacherId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teacherId, result.Id);
        }
    }
}
