using DqtApi.DAL;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetTeacherTests : IClassFixture<GetTeacherFixture>
    {
        private readonly GetTeacherFixture _fixture;
        private readonly IDataverseAdaptor _adaptor;

        public GetTeacherTests(GetTeacherFixture fixture)
        {
            _fixture = fixture;
            _adaptor = new DataverseAdaptor(_fixture.Service);
        }

        [Fact]
        public void Given_matching_TRN_and_matching_birth_date_return_teacher()
        {
            var request = _fixture.GetRequest(true, true);

            var teacher = _adaptor.GetTeacher(request);

            _fixture.AssertMatchesFixtureId(teacher.Id);
        }

        [Fact]
        public void Given_non_matching_TRN_return_null()
        {
            var request = _fixture.GetRequest(false, true);

            var teacher = _adaptor.GetTeacher(request);

            Assert.Null(teacher);
        }

        [Fact]
        public void Given_non_matching_birth_date_return_null()
        {
            var request = _fixture.GetRequest(true, false);

            var teacher = _adaptor.GetTeacher(request);

            Assert.Null(teacher);
        }

        [Fact]
        public void Given_matching_national_insurance_number_and_matching_birth_date_but_non_matching_TRN_return_teacher()
        {
            var request = _fixture.GetRequest(false, true, true);

            var teacher = _adaptor.GetTeacher(request);

            _fixture.AssertMatchesFixtureId(teacher.Id);
        }

        [Fact]
        public void Given_matching_TRN_and_matching_birth_date_but_non_matching_national_insurance_number_return_teacher()
        {
            var request = _fixture.GetRequest(true, true, false);

            var teacher = _adaptor.GetTeacher(request);

            _fixture.AssertMatchesFixtureId(teacher.Id);
        }

        [Fact]
        public void Given_matching_national_insurance_number_but_non_matching_TRN_and_non_matching_birth_date_return_null()
        {
            var request = _fixture.GetRequest(false, false, true);

            var teacher = _adaptor.GetTeacher(request);

            Assert.Null(teacher);
        }

        [Fact]
        public void Given_matching_TRN_but_non_matching_national_insurance_number_and_non_matching_birth_date_return_null()
        {
            var request = _fixture.GetRequest(true, false, false);

            var teacher = _adaptor.GetTeacher(request);

            Assert.Null(teacher);
        }
    }
}
