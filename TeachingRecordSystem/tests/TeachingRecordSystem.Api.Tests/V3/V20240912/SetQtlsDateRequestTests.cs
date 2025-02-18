using System.Net;
using TeachingRecordSystem.Api.V3.V20240912.Requests;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3.V20240912;

public class SetQtlsDateRequestTests : TestBase
{
    public SetQtlsDateRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.AssignQtls]);
    }

    [Theory, RoleNamesData(except: ApiRoles.AssignQtls)]
    public async Task Put_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var requestBody = CreateJsonContent(new { qtsDate = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Put_QtlsDateInFuture_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var futureDate = Clock.Today.AddDays(1);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = CreateJsonContent(new { qtsDate = futureDate })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: nameof(SetQtlsRequest.QtsDate),
            expectedError: "Date cannot be in the future.");
    }

    [Fact]
    public async Task Put_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTrn = "0000000";

        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{nonExistentTrn}/qtls")
        {
            Content = CreateJsonContent(new { qtsDate = new DateOnly(1990, 01, 01) })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ValidQtsDateWithNoExistingQtsDate_UpdatesQtlsDateAndReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var qtlsDate = new DateOnly(2020, 01, 01);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = CreateJsonContent(new { qtsDate = qtlsDate })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                person.Trn,
                qtsDate = qtlsDate
            },
            expectedStatusCode: 200);

        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        Assert.Equal(qtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_NullQtlsDateWithExistingQtlsDate_UpdatesQtlsDateAndReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQtlsDate(new DateOnly(2020, 01, 01)));

        DateOnly? qtlsDate = null;

        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = CreateJsonContent(new { qtsDate = qtlsDate })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                person.Trn,
                qtsDate = qtlsDate
            },
            expectedStatusCode: 200);

        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        Assert.Null(contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_SetQtlsWithActiveAlert_SetsQtlsAndCreatesReviewTask()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null)));

        var qtlsDate = new DateOnly(2020, 01, 01);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = CreateJsonContent(new { qtsDate = qtlsDate })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(
            x => x.RegardingObjectId.Id == person.PersonId && x.Category == "QTLS date set/removed for record with an active alert");

        Assert.NotNull(task);
        Assert.Equal("QTLS date set/removed for record with an active alert", task.Category);
        Assert.Equal($"QTLSDate {qtlsDate} set for a record with active alert", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
