using System;
using System.Collections.Generic;
using Xunit;

namespace DqtApi.Tests.UnitTests
{
    public class GetTeacherRequest
    {
        private readonly string _trn = "1111111";
        private readonly string _nationalInsuranceNumber = "AA123456A";

        private Contact _matchingTrn;
        private Contact _matchingNationalInsuranceNumber;
        private IEnumerable<Contact> _matches;

        public GetTeacherRequest()
        {
            _matchingTrn = new() { dfeta_TRN = _trn, Id = Guid.NewGuid() };
            _matchingNationalInsuranceNumber = new() { dfeta_NINumber = _nationalInsuranceNumber, Id = Guid.NewGuid() };
            _matches = new[] { _matchingTrn, _matchingNationalInsuranceNumber };
        }

        [Fact]
        public void Given_a_match_by_trn_return_contact()
        {
            var request = new Models.GetTeacherRequest { TRN = _trn, NationalInsuranceNumber = "BB234567B" };
            var match = request.SelectMatch(_matches);
            Assert.Equal(_matchingTrn.Id, match.Id);
        }

        [Fact]
        public void Given_no_match_by_trn_but_a_match_by_national_insurance_number_return_contact()
        {
            var request = new Models.GetTeacherRequest { TRN = "2222222", NationalInsuranceNumber = _nationalInsuranceNumber };
            var match = request.SelectMatch(_matches);
            Assert.Equal(_matchingNationalInsuranceNumber.Id, match.Id);
        }

        [Fact]
        public void Given_no_match_by_trn_or_national_insurance_number_return_null()
        {
            var request = new Models.GetTeacherRequest { TRN = "2222222", NationalInsuranceNumber = "BB234567B" };
            var match = request.SelectMatch(_matches);
            Assert.Null(match);
        }
    }
}
