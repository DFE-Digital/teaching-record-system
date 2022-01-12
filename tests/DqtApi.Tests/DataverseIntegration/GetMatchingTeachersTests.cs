using System.Linq;
using DqtApi.DataStore.Crm;
using Xunit;
using static DqtApi.Tests.DataverseIntegration.GetMatchingTeachersFixture.MatchFixture;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetMatchingTeachersTests : IClassFixture<GetMatchingTeachersFixture>
    {
        private readonly GetMatchingTeachersFixture _fixture;
        private readonly IDataverseAdapter _dataverseAdapter;

        public GetMatchingTeachersTests(GetMatchingTeachersFixture fixture)
        {
            _fixture = fixture;
            _dataverseAdapter = new DataverseAdapter(_fixture.Service);
        }

        [Fact]
        public async void Given_matching_TRN_and_matching_birth_date_return_unique_teacher()
        {
            var request = _fixture.GetRequest(One, true);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Single(matchingTeachers);

            _fixture.AssertMatchesFixture(matchingTeachers.Single(), 0);
        }

        [Fact]
        public async void Given_non_matching_TRN_return_no_teachers()
        {
            var request = _fixture.GetRequest(None, true);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Empty(matchingTeachers);
        }

        [Fact]
        public async void Given_non_matching_birth_date_return_no_teachers()
        {
            var request = _fixture.GetRequest(One, false);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Empty(matchingTeachers);
        }

        [Fact]
        public async void Given_non_matching_TRN_and_non_matching_birth_date_return_no_teachers()
        {
            var request = _fixture.GetRequest(None, false);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Empty(matchingTeachers);
        }

        [Fact]
        public async void Given_matching_national_insurance_number_and_matching_birth_date_and_matching_TRN_return_teacher()
        {
            var request = _fixture.GetRequest(One, true, One);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Single(matchingTeachers);

            _fixture.AssertMatchesFixture(matchingTeachers.Single(), 0);
        }

        [Fact]
        public async void Given_matching_national_insurance_number_and_matching_birth_date_and_different_matching_TRN_return_teacher()
        {
            var request = _fixture.GetRequest(One, true, Two);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Collection(matchingTeachers,
                firstTeacher => _fixture.AssertMatchesFixture(firstTeacher, 0),
                secondTeacher => _fixture.AssertMatchesFixture(secondTeacher, 1)
            );
        }

        [Fact]
        public async void Given_matching_national_insurance_number_and_matching_birth_date_but_non_matching_TRN_return_teacher()
        {
            var request = _fixture.GetRequest(None, true, One);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Single(matchingTeachers);

            _fixture.AssertMatchesFixture(matchingTeachers.Single(), 0);
        }

        [Fact]
        public async void Given_matching_TRN_and_matching_birth_date_but_non_matching_national_insurance_number_return_teacher()
        {
            var request = _fixture.GetRequest(One, true, None);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Single(matchingTeachers);

            _fixture.AssertMatchesFixture(matchingTeachers.Single(), 0);
        }

        [Fact]
        public async void Given_matching_national_insurance_number_but_non_matching_TRN_and_non_matching_birth_date_return_empty()
        {
            var request = _fixture.GetRequest(None, false, One);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Empty(matchingTeachers);
        }

        [Fact]
        public async void Given_matching_TRN_but_non_matching_national_insurance_number_and_non_matching_birth_date_return_empty()
        {
            var request = _fixture.GetRequest(One, false, None);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Empty(matchingTeachers);
        }

        [Fact]
        public async void Given_matching_TRN_and_matching_national_insurance_number_but_non_matching_birth_date_return_empty()
        {
            var request = _fixture.GetRequest(One, false, One);

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            Assert.Empty(matchingTeachers);
        }
    }
}
