using System.Net;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class SetQtlsDateRequestTests : TestBase
{
    public SetQtlsDateRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.AssignQtls });
    }

    [Theory, RoleNamesData(except: ApiRoles.AssignQtls)]
    public async Task Put_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { QTSDate = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("xxx")]
    public async Task Put_InvalidTrn_ReturnsErrror(string trn)
    {
        // Arrange
        var requestBody = CreateJsonContent(new { QTSDate = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
    }

    [Fact]
    public async Task Put_QTLSDateInFuture_ReturnsErrror()
    {
        // Arrange
        var futureDate = Clock.UtcNow.AddDays(1);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { QTSDate = futureDate.ToDateOnlyWithDqtBstFix(isLocalTime: true) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(SetQtlsRequest.QTSDate),
            expectedError: "QTLS Date cannot be in the future.");
    }

    [Fact]
    public async Task Put_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTrn = "1234567";
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true));

        var requestBody = CreateJsonContent(new { QTSDate = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{nonExistentTrn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //2
    [Fact]
    public async Task Put_ValidQTSDateWithNoExistingQTSDate_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { QTSDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                existingContact.Trn,
                QTSDate = qtlsDate

            },
            expectedStatusCode: 200);

        Assert.Null(task);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact!.dfeta_InductionStatus);
        Assert.Equal(qtlsDate, contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //3
    [Fact]
    public async Task Put_ClearExistingQTLSDateWithNoQTS_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQtlsDate(new DateOnly(2020, 01, 01)));

        var requestBody = CreateJsonContent(new { QTSDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                existingContact.Trn,
                QTSDate = qtlsDate

            },
            expectedStatusCode: 200);
        Assert.Null(contact!.dfeta_InductionStatus);
        Assert.Null(contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //6
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusIsRequiredToComplete_SetsInductionStatusRequiredToComplete()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var expectedInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var expectedHttpStatus = HttpStatusCode.OK;
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));


        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);

        // Assert
        Assert.Equal(expectedHttpStatus, response.StatusCode);
        Assert.Equal(expectedInductionStatus, contact!.dfeta_InductionStatus);
    }

    //7
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsRequiredToComplete_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var expectedInductionStatus = dfeta_InductionStatus.Exempt;
        var expectedHttpStatus = HttpStatusCode.OK;
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);

        // Assert
        Assert.Equal(expectedHttpStatus, response.StatusCode);
        Assert.Equal(expectedInductionStatus, contact!.dfeta_InductionStatus);
    }

    //8
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsInProgress_ReturnsStatusAcceptedWithTask()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.InProgress;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var expectedInductionStatus = dfeta_InductionStatus.InProgress;
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)); ;

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Equal(expectedInductionStatus, contact!.dfeta_InductionStatus);
        Assert.NotNull(task);
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Contains("Unable to set QTLSDate", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Contains($"Unable to set QTLSDate", task.Description);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    //9
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsNotYetCompleted_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var expectedInductionStatus = dfeta_InductionStatus.Exempt;
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedInductionStatus, contact!.dfeta_InductionStatus);
    }

    //10
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusIsNotYetCompleted_SetsInductionStatusNotYetCompleted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.NotYetCompleted, contact!.dfeta_InductionStatus);
    }

    //11 - verify about with ab
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsInductionExtendedWithoutAB_SetsInductionStatusExempt()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact!.dfeta_InductionStatus);
    }

    //12 
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusIsInductionExtendedWithAB_SetsInductionStatusBackToInductionExtended()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithQtlsDate(existingQtlsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, account.AccountId));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.InductionExtended, contact!.dfeta_InductionStatus);
    }

    //13 - with AB
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusIsInductionExtendedWithAB_SetsInductionStatusInductionExtended()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("04/04/2011");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, account.AccountId));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.InductionExtended, contact!.dfeta_InductionStatus);
    }

    //14
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsExempt_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Exempt;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact!.dfeta_InductionStatus);
        Assert.Equal(incomingqtlsDate, contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(incomingqtlsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //15
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusIsExempt_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Exempt;
        var existingQtlsDate = DateOnly.Parse("04/04/2014");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact!.dfeta_InductionStatus);
        Assert.Null(contact!.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //16
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsPass_SetsInductionStatusPass()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Pass;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Pass, contact!.dfeta_InductionStatus);
        Assert.Equal(incomingqtlsDate, contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(incomingqtlsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //17
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusPass_SetsInductionStatusPass()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Pass;
        var existingQtlsDate = DateOnly.Parse("04/04/2014");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Pass, contact!.dfeta_InductionStatus);
        Assert.Null(contact!.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //18
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusFail_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Fail;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/01/1992");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Fail, contact!.dfeta_InductionStatus);
        Assert.Null(contact!.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Contains("Unable to set QTLSDate", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Contains($"Unable to set QTLSDate", task.Description);
    }

    //19
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusFail_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Fail;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var existingQtlsDate = DateOnly.Parse("04/04/1991");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Fail, contact!.dfeta_InductionStatus);
        Assert.Equal(existingQtlsDate, contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(existingQtlsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Contains("Unable to set QTLSDate", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Contains($"Unable to set QTLSDate", task.Description);
    }

    //20
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusIsPassedInWales_SetsInductionStatusPassedInWales()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.PassedinWales;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.PassedinWales, contact!.dfeta_InductionStatus);
        Assert.Equal(incomingqtlsDate, contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(incomingqtlsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //21
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusPassedInWales_SetsInductionStatusPassedInWales()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.PassedinWales;
        var existingQtlsDate = DateOnly.Parse("04/04/2014");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.PassedinWales, contact!.dfeta_InductionStatus);
        Assert.Null(contact!.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    //22
    [Fact]
    public async Task Put_SetQTLSWhenInductionStatusFailedInWales_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.FailedinWales;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/01/1992");
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.FailedinWales, contact!.dfeta_InductionStatus);
        Assert.Null(contact!.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Contains("Unable to set QTLSDate", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Contains($"Unable to set QTLSDate", task.Description);
    }

    //23
    [Fact]
    public async Task Put_ClearQTLSWhenInductionStatusFailedInWales_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.FailedinWales;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var existingQtlsDate = DateOnly.Parse("04/04/1991");
        var incomingqtlsDate = default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == existingContact.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact!.dfeta_InductionStatus);
        Assert.Equal(existingQtlsDate, contact!.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(existingQtlsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Contains("Unable to set QTLSDate", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Contains($"Unable to set QTLSDate", task.Description);
    }

    [Fact]
    public async Task Put_ValidQTLSWithNoQTS_SetsQTSDate()
    {
        // Arrange
        var qtlsDate = new DateOnly(2010, 01, 01);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQtlsDate(qtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId == existingContact.ContactId.ToEntityReference(Contact.EntityLogicalName) && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(qtlsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task Put_RemoveQTLSDate_RevertsQTSDate()
    {
        // Arrange
        var qtlsDate = new DateOnly(2010, 01, 01);
        var qtsDate = new DateOnly(2008, 01, 01);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate)
            .WithQtlsDate(qtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = default(DateOnly?) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId == existingContact.ContactId.ToEntityReference(Contact.EntityLogicalName) && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(qtsDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task Put_QTLSDateAfterAllQTSDates_SetsQTSDate()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var qtsDate1 = new DateOnly(2010, 01, 01);
        var earliestQTSDate = new DateOnly(2008, 01, 01);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate1)
            .WithQts(earliestQTSDate));

        var requestBody = CreateJsonContent(new { QTSDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(earliestQTSDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task Put_QTLSDateBeforeAllQTSDates_SetsQTSDate()
    {
        // Arrange
        var earliestQTLSDate = new DateOnly(2001, 01, 01);
        var qtsDate1 = new DateOnly(2010, 01, 01);
        var qtsDate2 = new DateOnly(2008, 01, 01);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate1)
            .WithQts(qtsDate2));

        var requestBody = CreateJsonContent(new { QTSDate = earliestQTLSDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId == existingContact.ContactId.ToEntityReference(Contact.EntityLogicalName) && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(earliestQTLSDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Theory]
    [InlineData("01/01/1999", "02/02/2023", "02/02/2022", "02/02/2022")] //QTS After QTLS
    [InlineData("01/01/2024", "02/02/2001", "02/02/2004", "02/02/2001")] //QTLS After QTS
    public async Task Put_RemoveQTLS_SetsQTSDateToEarliestQTSRegistrationDate(string qtls, string qts1, string qts2, string expectedQTS)
    {
        // Arrange
        var qtlsDate = DateOnly.Parse(qtls);
        var qtsDate1 = DateOnly.Parse(qts1);
        var qtsDate2 = DateOnly.Parse(qts2);
        var expectedQTSDate = DateOnly.Parse(expectedQTS);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate1)
            .WithQts(qtsDate2)
            .WithQtlsDate(qtlsDate));

        var requestBody = CreateJsonContent(new { QTSDate = default(DateOnly?) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedQTSDate, contact!.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.Exempt, dfeta_InductionExemptionReason.Exempt, "01/01/2021", dfeta_InductionStatus.Exempt, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.Exempt, dfeta_InductionExemptionReason.Exempt, null, dfeta_InductionStatus.Exempt, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.Fail, null, "01/01/2021", dfeta_InductionStatus.Fail, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.Fail, null, null, dfeta_InductionStatus.Fail, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.InProgress, null, "01/01/2021", dfeta_InductionStatus.Exempt, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.InProgress, null, null, dfeta_InductionStatus.InProgress, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null, "01/01/2021", dfeta_InductionStatus.Exempt, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null, null, dfeta_InductionStatus.NotYetCompleted, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.InductionExtended, null, "01/01/2021", dfeta_InductionStatus.Exempt, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.InductionExtended, null, null, dfeta_InductionStatus.InductionExtended, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.Pass, null, "01/01/2021", dfeta_InductionStatus.Pass, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.Pass, null, null, dfeta_InductionStatus.Pass, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.PassedinWales, null, "01/01/2021", dfeta_InductionStatus.PassedinWales, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.PassedinWales, null, null, dfeta_InductionStatus.PassedinWales, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null, "01/01/2021", dfeta_InductionStatus.Exempt, HttpStatusCode.OK)]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null, null, dfeta_InductionStatus.RequiredtoComplete, HttpStatusCode.OK)]
    public async Task Put_ValidQTLSDateWithNoQTS_SetsInductionStatus(dfeta_InductionStatus existingInductionStatus, dfeta_InductionExemptionReason? existingInductionExemptionReason, string? incomingQtls, dfeta_InductionStatus expectetInductionStatus, HttpStatusCode expectedHttpStatus)
    {
        // Arrange
        var qtlsDate = !string.IsNullOrEmpty(incomingQtls) ? DateOnly.Parse(incomingQtls) : default(DateOnly?);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: existingInductionExemptionReason, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { QTSDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == existingContact.PersonId);

        // Assert
        Assert.Equal(expectedHttpStatus, response.StatusCode);
        Assert.Equal(expectetInductionStatus, contact!.dfeta_InductionStatus);
    }
}
