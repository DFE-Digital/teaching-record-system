using System.Net.Http;
using System.Threading.Tasks;
using DqtApi.Properties;
using DqtApi.TestCommon;
using Xunit;

namespace DqtApi.Tests.V1.Operations
{
    public class GetTeacherTests : ApiTestBase
    {
        public GetTeacherTests(ApiFixture apiFixture)
            : base(apiFixture)
        {
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("12345678")]
        [InlineData("xxx")]
        public async Task InvalidTrn_ReturnsError(string trn)
        {
            var birthDate = "1990-04-01";
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

            var response = await HttpClient.SendAsync(request);

            await AssertEx.ResponseIsValidationErrorForProperty(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
        }

        [Theory]
        [InlineData("xxx")]
        public async Task InvalidBirthDate_ReturnsError(string birthDate)
        {
            var trn = "1234567";
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

            var response = await HttpClient.SendAsync(request);

            await AssertEx.ResponseIsValidationErrorForProperty(response, "birthdate", expectedError: StringResources.ErrorMessages_InvalidBirthDate);
        }

        //[Fact(Skip = "not implemented")]
        //public async Task NinoSpecified_GeneratesQueryWithNino()
        //{
        //    throw new System.NotImplementedException();
        //}

        //[Fact(Skip = "not implemented")]
        //public async Task NinoNotSpecified_GeneratesQueryWithoutNino()
        //{
        //    throw new System.NotImplementedException();
        //}

        //[Fact(Skip = "not implemented")]
        //public async Task NoContactFound_ReturnsNotFound()
        //{
        //    throw new System.NotImplementedException();
        //}

        //[Fact(Skip = "not implemented")]
        //public async Task MultipleContactsFound_ReturnsFirstMatch()
        //{
        //    throw new System.NotImplementedException();
        //}

        // TODO response validation
    }
}
