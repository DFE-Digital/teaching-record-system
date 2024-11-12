#nullable disable
using System.Net;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

public class UnlockTeacherTests : TestBase
{
    public UnlockTeacherTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.UnlockPerson });
    }

    [Theory, RoleNamesData(except: new[] { ApiRoles.UnlockPerson })]
    public async Task UnlockTeacher_ClientDoesNotHaveSecurityRoles_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var createPersonResult = await TestData.CreatePerson();
        var contact = (await TestData.OrganizationService.RetrieveAsync(Contact.EntityLogicalName, createPersonResult.ContactId, new ColumnSet(allColumns: true)))
            .ToEntity<Contact>();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(contact.Id, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(contact);

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(contact.Id))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{createPersonResult.ContactId}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync((Contact)null);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Given_a_teacher_that_does_exist_and_is_locked_returns_ok()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson(p => p.WithLoginFailedCounter(3));
        var contact = (await TestData.OrganizationService.RetrieveAsync(Contact.EntityLogicalName, createPersonResult.ContactId, new ColumnSet(allColumns: true)))
            .ToEntity<Contact>();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(contact.Id, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(contact);

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(contact.Id))
            .ReturnsAsync(true)
            .Verifiable();

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(contact.Id))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{contact.Id}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                hasBeenUnlocked = true
            });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Given_a_teacher_that_does_exist_but_is_not_locked_returns_ok(int? loginFailedCounter)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson(p => p.WithLoginFailedCounter(loginFailedCounter));
        var contact = (await TestData.OrganizationService.RetrieveAsync(Contact.EntityLogicalName, createPersonResult.ContactId, new ColumnSet(allColumns: true)))
            .ToEntity<Contact>();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(contact.Id, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(contact);

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(contact.Id))
            .ReturnsAsync(true)
            .Verifiable();

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(contact.Id))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{contact.Id}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                hasBeenUnlocked = false
            });
    }

    [Fact]
    public async Task Given_a_teacher_that_has_activesanctions_returns_error()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson(b => b.WithAlert(a => a.WithEndDate(null)));
        var contact = (await TestData.OrganizationService.RetrieveAsync(Contact.EntityLogicalName, createPersonResult.ContactId, new ColumnSet(allColumns: true)))
            .ToEntity<Contact>();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(contact.Id, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(contact);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{contact.Id}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10014, expectedStatusCode: StatusCodes.Status400BadRequest);
    }
}
