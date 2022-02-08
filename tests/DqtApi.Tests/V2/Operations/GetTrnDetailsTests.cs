using System;
using System.Collections.Generic;
using System.Net.Http;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.TestCommon;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DqtApi.Tests.V2.Operations
{
    public class GetTrnDetailsTests : ApiTestBase
    {
        public GetTrnDetailsTests(ApiFixture apiFixture) : base(apiFixture)
        {
        }

        [Fact]
        public async Task When_no_results_found_then_response_is_nocontent()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        }

        [Fact]
        public async Task Given_SearchParameters_SearchResults_Match()
        {
            // Arrange
            var contact1 = new Contact() { FirstName = "test", LastName = "testing", Id = Guid.NewGuid(), dfeta_NINumber = "1111", BirthDate = new DateTime(1988, 2, 1), dfeta_TRN = "someReference" };
            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}");
            ApiFixture.DataverseAdapter
                .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
                .ReturnsAsync(new List<Contact> { contact1 })
                .Verifiable();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            ApiFixture.DataverseAdapter.Verify();
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    results = new[]
                    {
                        new
                        {
                             trn = contact1.dfeta_TRN,
                             emailAddresses = default(List<string>),
                             firstName = contact1.FirstName,
                             lastName = contact1.LastName,
                             dateOfBirth = DateOnly.FromDateTime(contact1.BirthDate.Value).ToString("yyyy-MM-dd"),
                             nationalInsuranceNumber = contact1.dfeta_NINumber,
                             uid = contact1.Id.ToString()
                        }
                    }
                },
                expectedStatusCode: StatusCodes.Status200OK);
        }

        [Theory]
        [InlineData("someProvider", "")]
        [InlineData(null, "1005811506")]
        public async Task Given_NonExistent_ProviderNameOrUkprnProvider_ReturnError(string providerName, string providerUkprn)
        {
            // Arrange
            var contact1 = new Contact() { FirstName = "test", LastName = "testing", Id = Guid.NewGuid(), dfeta_NINumber = "1111", BirthDate = new DateTime(1988, 1, 1), dfeta_TRN = "someReference" };
            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}&IttProviderUkPrn={providerUkprn}&IttProviderName={providerName}");
            ApiFixture.DataverseAdapter
                .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
                .ReturnsAsync(new List<Contact> { contact1 })
                .Verifiable();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("someProvider", "")]
        [InlineData(null, "1005811506")]
        public async Task Given_SearchParametersContainValidProvider_SearchResultsAreReturned(string providerName, string providerUkprn)
        {
            // Arrange
            var account = new Account() { Name = "someProvider" };
            var contact1 = new Contact() { FirstName = "test", LastName = "testing", Id = Guid.NewGuid(), dfeta_NINumber = "1111", BirthDate = new DateTime(1988, 1, 1), dfeta_TRN = "someReference" };
            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}&IttProviderUkPrn={providerUkprn}&IttProviderName={providerName}");
            ApiFixture.DataverseAdapter
                .Setup(mock => mock.GetOrganizationByProviderName(It.IsAny<string>()))
                .ReturnsAsync(account);
            ApiFixture.DataverseAdapter
                 .Setup(mock => mock.GetOrganizationByUkprn(It.IsAny<string>()))
                 .ReturnsAsync(account);
            ApiFixture.DataverseAdapter
                .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
                .ReturnsAsync(new List<Contact> { contact1 })
                .Verifiable();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            ApiFixture.DataverseAdapter.Verify();
            await AssertEx.JsonResponseEquals(
                response,
                expected: new
                {
                    results = new[]
                    {
                        new
                        {
                             trn = contact1.dfeta_TRN,
                             emailAddresses = default(List<string>),
                             firstName = contact1.FirstName,
                             lastName = contact1.LastName,
                             dateOfBirth = DateOnly.FromDateTime(contact1.BirthDate.Value).ToString("yyyy-MM-dd"),
                             nationalInsuranceNumber = contact1.dfeta_NINumber,
                             uid = contact1.Id.ToString()
                        }
                    }
                },
                expectedStatusCode: StatusCodes.Status200OK);
        }

        [Fact]
        public async Task Given_BothUkPrnAndProviderNameAreProvided_ReturnError()
        {
            // Arrange
            var contact1 = new Contact() { FirstName = "test", LastName = "testing", Id = Guid.NewGuid(), dfeta_NINumber = "1111", BirthDate = new DateTime(1988, 1, 1), dfeta_TRN = "someReference" };
            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}&IttProviderUkPrn=12345678910&IttProviderName=provider");
            ApiFixture.DataverseAdapter
                .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
                .ReturnsAsync(new List<Contact> { contact1 })
                .Verifiable();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }
    }
}
