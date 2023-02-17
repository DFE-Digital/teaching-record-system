using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using QualifiedTeachersApi.TestCommon;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace QualifiedTeachersApi.FunctionalTests.Endpoints.V1;

public class GetTeacherTests : IAssemblyFixture<ApiFixture>
{
    public readonly string _validTrn;
    public readonly string _validTrnMostlyNull;
    public readonly string _validTrnNestedNull;
    public readonly string _validTrnBadQual;
    public readonly string _validNino;
    public readonly string _validBirthdate;

    public GetTeacherTests(ApiFixture fixture)
    {
        HttpClient = fixture.HttpClient;

        _validTrn = fixture.TestData["ValidTrn"];
        _validTrnMostlyNull = fixture.TestData["ValidTrnMostlyNull"];
        _validTrnNestedNull = fixture.TestData["ValidTrnNestedNull"];
        _validTrnBadQual = fixture.TestData["ValidTrnBadQual"];
        _validNino = fixture.TestData["ValidNino"];
        _validBirthdate = fixture.TestData["ValidBirthdate"];
    }

    public HttpClient HttpClient { get; }

    [Fact]
    public async Task ValidTrnAndBirthdate_ReturnsOk()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrn}?birthdate={_validBirthdate}");

        var response = await HttpClient.SendAsync(request);

        var content = await AssertEx.JsonResponse(response);
        Assert.Equal(_validTrn, content.trn.ToString());
        Assert.NotNull(content.qualified_teacher_status.qts_date);
        Assert.NotNull(content.qualified_teacher_status.name);
        Assert.NotNull(content.induction.start_date);
        Assert.NotNull(content.induction.completion_date);
        Assert.NotNull(content.initial_teacher_training.programme_end_date);
        Assert.True(content.qualifications.Count > 0);
    }

    [Fact]
    public async Task ValidTrnWithMostlyNullData_ReturnsOk()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrnMostlyNull}?birthdate={_validBirthdate}");

        var response = await HttpClient.SendAsync(request);

        var content = await AssertEx.JsonResponse(response);
        Assert.Equal(_validTrnMostlyNull, content.trn.ToString());
        Assert.Empty(content.qualified_teacher_status);
        Assert.Empty(content.induction);
        Assert.Empty(content.initial_teacher_training);
        Assert.True(content.qualifications.Count == 0);
    }

    [Fact]
    public async Task ValidTrnWithNestedNullData_ReturnsOk()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrnNestedNull}?birthdate={_validBirthdate}");

        var response = await HttpClient.SendAsync(request);

        var content = await AssertEx.JsonResponse(response);

        Assert.Equal(_validTrnNestedNull, content.trn.ToString());

        Assert.NotNull(content.qualified_teacher_status);
        Assert.Empty(content.qualified_teacher_status.qts_date);

        Assert.NotNull(content.induction);
        Assert.Empty(content.induction.completion_date);

        Assert.NotNull(content.initial_teacher_training);
        Assert.Empty(content.initial_teacher_training.programme_end_date);

        Assert.True(content.qualifications.Count == 2);
        Assert.Empty(content.qualifications[0].date_awarded);
    }

    [Fact]
    public async Task TrnWithBadQualData_ReturnsOk()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrnBadQual}?birthdate={_validBirthdate}");

        var response = await HttpClient.SendAsync(request);

        var content = await AssertEx.JsonResponse(response);

        Assert.Equal(_validTrnBadQual, content.trn.ToString());
        Assert.True(content.qualifications.Count == 2);
        Assert.Contains((IEnumerable<dynamic>)content.qualifications, x => x.name == "NPQEL" && x.date_awarded != null);
    }

    [Fact]
    public async Task InvalidTrn_ReturnsBadRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/12345678?birthdate={_validBirthdate}");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MissingTrn_ReturnsNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TrnDoesntExist_ReturnsNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/9999999?birthdate={_validBirthdate}");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MismatchingBirthdate_ReturnsNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/9999999?birthdate=1990-01-01");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InvalidBirthdate_ReturnsBadRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrn}?birthdate=01-01-1990");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidBirthdateMonth_ReturnsBadRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrn}?birthdate=1990-13-01");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MissingBirthdate_ReturnsNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrn}");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "Quirk of the current APIM implementation")]
    public async Task AdditinalODataFieldsSpecified_ReturnsForbidden()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrn}?birthdate={_validBirthdate}&$select=dfeta_ninumber,dfeta_trn,firstname,lastname,birthdate");

        var response = await HttpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ValidTrnBirthdateAndMismatchingNino_ReturnsCorrectRecord()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{_validTrnMostlyNull}/?birthdate={_validBirthdate}&nino={_validNino}");

        var response = await HttpClient.SendAsync(request);

        var content = await AssertEx.JsonResponse(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(_validTrnMostlyNull, content.trn.ToString());
        Assert.NotEqual(_validNino, content.ni_number.ToString());
    }

    [Fact]
    public async Task ValidBirthdateAndNinoAndMismatchingTrn_ReturnsCorrectRecord()
    {
        var trn = "0000000";
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/teachers/{trn}/?birthdate={_validBirthdate}&nino={_validNino}");

        var response = await HttpClient.SendAsync(request);

        var content = await AssertEx.JsonResponse(response);
        Assert.NotEqual(trn, content.trn.ToString());
        Assert.Equal(_validNino, content.ni_number.ToString());
    }
}
