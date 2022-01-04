using System;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetTeacherTests : IClassFixture<CrmClientFixture>
    {
        private readonly CrmClientFixture _crmClientFixture;
        private readonly DataverseAdaptor _dataverseAdaptor;
        private readonly ServiceClient _serviceClient;

        public GetTeacherTests(CrmClientFixture crmClientFixture)
        {
            _crmClientFixture = crmClientFixture;
            _dataverseAdaptor = crmClientFixture.CreateDataverseAdaptor();
            _serviceClient = crmClientFixture.ServiceClient;
        }

        [Fact]
        public async Task Given_teacher_that_does_not_exist_returns_null()
        {
            // Arrange
            var teacherId = Guid.NewGuid();

            // Act
            var result = await _dataverseAdaptor.GetTeacherAsync(teacherId, resolveMerges: false, Contact.Fields.StateCode);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Given_teacher_that_exists_returns_teacher()
        {
            // Arrange
            var teacherId = await _serviceClient.CreateAsync(new Contact());
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, teacherId);

            // Act
            var result = await _dataverseAdaptor.GetTeacherAsync(teacherId, resolveMerges: false, Contact.Fields.StateCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teacherId, result.Id);
        }

        [Fact]
        public async Task Given_merged_teacher_and_resolveMerges_true_return_master()
        {
            // Arrange
            var masterTeacherId = await _serviceClient.CreateAsync(new Contact());
            var teacherId = await _serviceClient.CreateAsync(new Contact());

            await _serviceClient.ExecuteAsync(new MergeRequest()
            {
                Target = new EntityReference(Contact.EntityLogicalName, masterTeacherId),
                SubordinateId = teacherId,
                PerformParentingChecks = false,
                UpdateContent = new Entity(Contact.EntityLogicalName)
            });

            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, masterTeacherId);
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, teacherId);

            // Act
            var result = await _dataverseAdaptor.GetTeacherAsync(teacherId, resolveMerges: true, Contact.Fields.StateCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(masterTeacherId, result.Id);
        }

        [Fact]
        public async Task Given_merged_teacher_and_resolveMerges_false_return_merged_record()
        {
            // Arrange
            var masterTeacherId = await _serviceClient.CreateAsync(new Contact());
            var teacherId = await _serviceClient.CreateAsync(new Contact());

            await _serviceClient.ExecuteAsync(new MergeRequest()
            {
                Target = new EntityReference(Contact.EntityLogicalName, masterTeacherId),
                SubordinateId = teacherId,
                PerformParentingChecks = false,
                UpdateContent = new Entity(Contact.EntityLogicalName)
            });

            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, masterTeacherId);
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, teacherId);

            // Act
            var result = await _dataverseAdaptor.GetTeacherAsync(teacherId, resolveMerges: false, Contact.Fields.StateCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teacherId, result.Id);
            Assert.Equal(ContactState.Inactive, result.StateCode);
        }
    }
}
