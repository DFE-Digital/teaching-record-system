using System;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetTeachersByTrnAndDobTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;

        public GetTeachersByTrnAndDobTests(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        [Fact]
        public async Task Given_existing_teacher_matches_on_dob_and_trn_return_teacher()
        {
            // Arrange
            var dob = new DateOnly(1980, 01, 01);
            var command = new CreateTeacherCommand()
            {
                FirstName = "Someone",
                LastName = "Ryder",
                BirthDate = dob.ToDateTime(),
                GenderCode = Contact_GenderCode.Female,
                InitialTeacherTraining = new()
                {
                    ProviderUkprn = "10044534",  // ARK Teacher Training
                    ProgrammeStartDate = new(2020, 4, 1),
                    ProgrammeEndDate = new(2020, 10, 10),
                    ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                    Subject1 = "Computer Science",
                    Subject2 = "Mathematics",
                    AgeRangeFrom = dfeta_AgeRange._05,
                    AgeRangeTo = dfeta_AgeRange._11
                },
                Qualification = new()
                {
                    ProviderUkprn = "10044534",
                    CountryCode = "XK",
                    Subject = "Computing",
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new(2021, 5, 3)
                }
            };

            // Act
            var (teacherResult, _) = await _dataverseAdapter.CreateTeacherImpl(command);
            var result = await _dataverseAdapter.GetTeachersByTrnAndDoB(teacherResult.Trn, dob, columnNames: Contact.Fields.BirthDate);


            // Assert
            Assert.NotNull(result);
            Assert.Equal(dob, DateOnly.FromDateTime(result[0].BirthDate.Value)) ;
        }

        [Fact]
        public async Task Given_existing_teacher_matches_only_trn_return_empty_collection()
        {
            // Arrange
            var dob = new DateOnly(1988, 01, 01);
            var command = new CreateTeacherCommand()
            {
                FirstName = "Someone",
                LastName = "Ryder",
                BirthDate = dob.ToDateTime(),
                GenderCode = Contact_GenderCode.Female,
                InitialTeacherTraining = new()
                {
                    ProviderUkprn = "10044534",  // ARK Teacher Training
                    ProgrammeStartDate = new(2020, 4, 1),
                    ProgrammeEndDate = new(2020, 10, 10),
                    ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                    Subject1 = "Computer Science",
                    Subject2 = "Mathematics",
                    AgeRangeFrom = dfeta_AgeRange._05,
                    AgeRangeTo = dfeta_AgeRange._11
                },
                Qualification = new()
                {
                    ProviderUkprn = "10044534",
                    CountryCode = "XK",
                    Subject = "Computing",
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new(2021, 5, 3)
                }
            };

            // Act
            var (teacherResult, _) = await _dataverseAdapter.CreateTeacherImpl(command);
            var result = await _dataverseAdapter.GetTeachersByTrnAndDoB(teacherResult.Trn, new DateOnly(2022,1,1), columnNames: Contact.Fields.BirthDate);

            // Assert
            Assert.Empty(result);
        }
    }
}
