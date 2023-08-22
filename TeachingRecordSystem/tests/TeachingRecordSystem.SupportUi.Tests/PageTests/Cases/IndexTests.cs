using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Cases;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UserWithNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var request = new HttpRequestMessage(HttpMethod.Get, "/cases");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var case1TicketNumber = "CAS-01006-F4Q327";
        var case1CreatedOn = new DateTime(2023, 06, 19, 09, 15, 00);
        var case1Contact1Id = Guid.NewGuid();
        var case1FirstName = Faker.Name.First();
        var case1LastName = Faker.Name.Last();
        var case1SubjectId = Guid.NewGuid();
        var case1SubjectTitle = "Case 1 Subject";
        var case2TicketNumber = "CAS-20042-R6T3S2";
        var case2CreatedOn = new DateTime(2023, 07, 01, 11, 20, 00);
        var case2Contact1Id = Guid.NewGuid();
        var case2FirstName = Faker.Name.First();
        var case2LastName = Faker.Name.Last();
        var case2StatedFirstName = Faker.Name.First();
        var case2StatedLastName = Faker.Name.Last();
        var case2SubjectId = Guid.NewGuid();
        var case2SubjectTitle = "Case 2 Subject";

        var incident1 = new Incident()
        {
            TicketNumber = case1TicketNumber,
            CreatedOn = case1CreatedOn
        };

        incident1.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.PrimaryIdAttribute}", new AliasedValue(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, case1Contact1Id));
        incident1.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.Fields.FirstName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.FirstName, case1FirstName));
        incident1.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.Fields.LastName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.LastName, case1LastName));
        incident1.Attributes.Add($"{nameof(Subject).ToLower()}.{Subject.PrimaryIdAttribute}", new AliasedValue(Subject.EntityLogicalName, Subject.PrimaryIdAttribute, case1SubjectId));
        incident1.Attributes.Add($"{nameof(Subject).ToLower()}.{Subject.Fields.Title}", new AliasedValue(Subject.EntityLogicalName, Subject.Fields.Title, case1SubjectTitle));

        var incident2 = new Incident()
        {
            TicketNumber = case2TicketNumber,
            CreatedOn = case2CreatedOn
        };

        incident2.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.PrimaryIdAttribute}", new AliasedValue(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, case2Contact1Id));
        incident2.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.Fields.FirstName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.FirstName, case2FirstName));
        incident2.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.Fields.LastName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.LastName, case2LastName));
        incident2.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.Fields.dfeta_StatedFirstName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.dfeta_StatedFirstName, case2StatedFirstName));
        incident2.Attributes.Add($"{nameof(Contact).ToLower()}.{Contact.Fields.dfeta_StatedLastName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.dfeta_StatedLastName, case2StatedLastName));
        incident2.Attributes.Add($"{nameof(Subject).ToLower()}.{Subject.PrimaryIdAttribute}", new AliasedValue(Subject.EntityLogicalName, Subject.PrimaryIdAttribute, case2SubjectId));
        incident2.Attributes.Add($"{nameof(Subject).ToLower()}.{Subject.Fields.Title}", new AliasedValue(Subject.EntityLogicalName, Subject.Fields.Title, case2SubjectTitle));

        var incidents = new[]
        {
            incident1,
            incident2
        };

        DataverseAdapterMock.Setup(d => d.GetActiveIncidents())
            .ReturnsAsync(incidents);

        var request = new HttpRequestMessage(HttpMethod.Get, "/cases");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var tableRow1 = doc.GetElementByTestId($"case-{case1TicketNumber}");
        Assert.NotNull(tableRow1);
        Assert.Equal(case1TicketNumber, tableRow1.GetElementByTestId($"case-reference-{case1TicketNumber}")!.TextContent);
        Assert.Equal($"{case1FirstName} {case1LastName}", tableRow1.GetElementByTestId($"name-{case1TicketNumber}")!.TextContent);
        Assert.Equal(case1SubjectTitle, tableRow1.GetElementByTestId($"case-type-{case1TicketNumber}")!.TextContent);
        Assert.Equal(case1CreatedOn.ToString("dd/MM/yyyy"), tableRow1.GetElementByTestId($"created-on-{case1TicketNumber}")!.TextContent);
        var tableRow2 = doc.GetElementByTestId($"case-{case2TicketNumber}");
        Assert.NotNull(tableRow2);
        Assert.Equal(case2TicketNumber, tableRow2.GetElementByTestId($"case-reference-{case2TicketNumber}")!.TextContent);
        Assert.Equal($"{case2StatedFirstName} {case2StatedLastName}", tableRow2.GetElementByTestId($"name-{case2TicketNumber}")!.TextContent);
        Assert.Equal(case2SubjectTitle, tableRow2.GetElementByTestId($"case-type-{case2TicketNumber}")!.TextContent);
        Assert.Equal(case2CreatedOn.ToString("dd/MM/yyyy"), tableRow2.GetElementByTestId($"created-on-{case2TicketNumber}")!.TextContent);
    }

    [Fact]
    public async Task Get_ValidRequestNoActiveCases_RendersExpectedContent()
    {
        // Arrange
        DataverseAdapterMock.Setup(d => d.GetActiveIncidents())
            .ReturnsAsync(new Incident[] { });

        var request = new HttpRequestMessage(HttpMethod.Get, "/cases");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var doc = await response.GetDocument();
        Assert.NotNull(doc.GetElementByTestId("no-cases"));
    }
}
