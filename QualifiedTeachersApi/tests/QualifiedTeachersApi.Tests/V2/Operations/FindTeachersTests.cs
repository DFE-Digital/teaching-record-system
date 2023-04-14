#nullable disable
using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.TestCommon;
using QualifiedTeachersApi.V2.Responses;
using Xunit;

namespace QualifiedTeachersApi.Tests.V2.Operations;

public class FindTeachersTests : ApiTestBase
{
    public FindTeachersTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Given_no_results_returns_ok()
    {
        // Arrange
        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync(Array.Empty<Contact>());

        var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                results = Array.Empty<object>()
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Given_search_returns_a_result_returns_expected_response()
    {
        // Arrange
        var contact1 = new Contact()
        {
            FirstName = "test",
            LastName = "testing",
            Id = Guid.NewGuid(),
            dfeta_NINumber = "1111",
            BirthDate = new DateTime(1988, 2, 1),
            dfeta_TRN = "someReference"
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync(new[] { contact1 });

        var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                results = new[]
                {
                    new
                    {
                         trn = contact1.dfeta_TRN,
                         emailAddresses = Array.Empty<string>(),
                         firstName = contact1.FirstName,
                         lastName = contact1.LastName,
                         dateOfBirth = DateOnly.FromDateTime(contact1.BirthDate.Value).ToString("yyyy-MM-dd"),
                         nationalInsuranceNumber = contact1.dfeta_NINumber,
                         uid = contact1.Id.ToString(),
                         hasActiveSanctions = false
                    }
                }
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Given_find_with_no_search_parameters_return_empty_collection()
    {
        // Arrange
        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync((Contact[])null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?dateOfBirth&emailAddress&firstName&ittProviderName&lastName&nationalInsuranceNumber");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                results = Enumerable.Empty<FindTeacherResult>()
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData("someProvider", "")]
    [InlineData(null, "1005811506")]
    public async Task Given_search_with_valid_provider_returns_results(string providerName, string providerUkprn)
    {
        // Arrange
        var account = new Account() { Name = "someProvider" };

        var contact1 = new Contact()
        {
            FirstName = "test",
            LastName = "testing",
            Id = Guid.NewGuid(),
            dfeta_NINumber = "1111",
            BirthDate = new DateTime(1988, 1, 1),
            dfeta_TRN = "someReference"
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetIttProviderOrganizationsByName(It.IsAny<string>(), It.IsAny<string[]>(), /*activeOnly: */false))
            .ReturnsAsync(new[] { account });

        ApiFixture.DataverseAdapter
             .Setup(mock => mock.GetIttProviderOrganizationsByUkprn(It.IsAny<string>(), It.IsAny<string[]>(), /*activeOnly: */false))
             .ReturnsAsync(new[] { account });

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync(new[] { contact1 });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}&IttProviderUkPrn={providerUkprn}&IttProviderName={providerName}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                results = new[]
                {
                    new
                    {
                         trn = contact1.dfeta_TRN,
                         emailAddresses = Array.Empty<string>(),
                         firstName = contact1.FirstName,
                         lastName = contact1.LastName,
                         dateOfBirth = DateOnly.FromDateTime(contact1.BirthDate.Value).ToString("yyyy-MM-dd"),
                         nationalInsuranceNumber = contact1.dfeta_NINumber,
                         uid = contact1.Id.ToString(),
                         hasActiveSanctions = false
                    }
                }
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Given_both_ukprn_and_provider_name_are_specified_returns_error()
    {
        // Arrange
        var contact1 = new Contact()
        {
            FirstName = "test",
            LastName = "testing",
            Id = Guid.NewGuid(),
            dfeta_NINumber = "1111",
            BirthDate = new DateTime(1988, 1, 1),
            dfeta_TRN = "someReference"
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync(new[] { contact1 });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}&IttProviderUkPrn=12345678910&IttProviderName=provider");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_search_returns_a_result_with_no_active_sanctions_returns_expected_response()
    {
        // Arrange
        var contact1 = new Contact() { FirstName = "test", LastName = "testing", Id = Guid.NewGuid(), dfeta_NINumber = "1111", BirthDate = new DateTime(1988, 2, 1), dfeta_TRN = "someReference", dfeta_ActiveSanctions = null };
        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync(new[] { contact1 });

        var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                results = new[]
                {
                    new
                    {
                         trn = contact1.dfeta_TRN,
                         emailAddresses = Array.Empty<string>(),
                         firstName = contact1.FirstName,
                         lastName = contact1.LastName,
                         dateOfBirth = DateOnly.FromDateTime(contact1.BirthDate.Value).ToString("yyyy-MM-dd"),
                         nationalInsuranceNumber = contact1.dfeta_NINumber,
                         uid = contact1.Id.ToString(),
                         hasActiveSanctions = false
                    }
                }
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_search_returns_a_result_with_activesanctions_set_returns_expected_response(bool? activeSanctions)
    {
        // Arrange
        var contact1 = new Contact()
        {
            FirstName = "test",
            LastName = "testing",
            Id = Guid.NewGuid(),
            dfeta_NINumber = "1111",
            BirthDate = new DateTime(1988, 2, 1),
            dfeta_TRN = "someReference",
            dfeta_ActiveSanctions = activeSanctions
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersQuery>()))
            .ReturnsAsync(new[] { contact1 });

        var request = new HttpRequestMessage(HttpMethod.Get, $"v2/teachers/find?FirstName={contact1.FirstName}&LastName={contact1.LastName}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                results = new[]
                {
                    new
                    {
                         trn = contact1.dfeta_TRN,
                         emailAddresses = Array.Empty<string>(),
                         firstName = contact1.FirstName,
                         lastName = contact1.LastName,
                         dateOfBirth = DateOnly.FromDateTime(contact1.BirthDate.Value).ToString("yyyy-MM-dd"),
                         nationalInsuranceNumber = contact1.dfeta_NINumber,
                         uid = contact1.Id.ToString(),
                         hasActiveSanctions = activeSanctions
                    }
                }
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }
}
