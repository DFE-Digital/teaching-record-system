using System.Net;
using TeachingRecordSystem.Api.V3.V20240912.Requests;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3.V20240912;

[Collection(nameof(DisableParallelization))]
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
        var person = await TestData.CreatePerson(p => p.WithTrn());
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
    public async Task Put_QtlsDateInFuture_ReturnsErrror()
    {
        // Arrange
        var futureDate = Clock.UtcNow.AddDays(1);
        var person = await TestData.CreatePerson(p => p.WithTrn());
        var requestBody = CreateJsonContent(new { qtsDate = futureDate.ToDateOnlyWithDqtBstFix(isLocalTime: true) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(SetQtlsRequest.QtsDate),
            expectedError: "Date cannot be in the future.");
    }

    [Fact]
    public async Task Put_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTrn = "1234567";
        var requestBody = CreateJsonContent(new { qtsDate = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{nonExistentTrn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ValidQtsDateWithNoExistingQtsDate_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(p => p.WithTrn());
        var requestBody = CreateJsonContent(new { qtsDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().SingleOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                person.Trn,
                qtsDate = qtlsDate

            },
            expectedStatusCode: 200);

        Assert.Null(task);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
        Assert.Equal(qtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_ClearExistingQtlsDateWithNoQts_UpdatesContactAndReturnsOk()
    {
        // Arrange
        var qtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQtlsDate(new DateOnly(2020, 01, 01)));

        var requestBody = CreateJsonContent(new { qtsDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                person.Trn,
                qtsDate = qtlsDate

            },
            expectedStatusCode: 200);
        Assert.Null(contact.dfeta_InductionStatus);
        Assert.Null(contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusIsRequiredToComplete_SetsInductionStatusRequiredToComplete()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var expectedInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var expectedHttpStatus = HttpStatusCode.OK;
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));


        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);

        // Assert
        Assert.Equal(expectedHttpStatus, response.StatusCode);
        Assert.Equal(expectedInductionStatus, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusIsRequiredToComplete_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var expectedInductionStatus = dfeta_InductionStatus.Exempt;
        var expectedHttpStatus = HttpStatusCode.OK;
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);

        // Assert
        Assert.Equal(expectedHttpStatus, response.StatusCode);
        Assert.Equal(expectedInductionStatus, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusIsInProgress_ReturnsStatusAcceptedWithTask()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.InProgress;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var expectedInductionStatus = dfeta_InductionStatus.InProgress;
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)); ;

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Equal(expectedInductionStatus, contact.dfeta_InductionStatus);
        Assert.NotNull(task);
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Equal($"Unable to set QTLSDate {incomingqtlsDate}, teacher induction currently set to 'In Progress'", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject); ;
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusIsNotYetCompleted_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var expectedInductionStatus = dfeta_InductionStatus.Exempt;
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedInductionStatus, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_ClearQtlsWithActiveAlert_ClearsQtlsAndCreatesReviewTask()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate)
            .WithSanction("G1"));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Category == "QTLS date set/removed for record with an active alert");

        // Assert
        Assert.NotNull(task);
        Assert.Equal("QTLS date set/removed for record with an active alert", task.Category);
        Assert.Equal($"QTLSDate removed for a record with active alert", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.NotYetCompleted, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_SetQtlsSWithSameQtlsDate_ReturnsAcceptedAndCreatesReviewTask()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("04/04/2002");
        var existingqtlsDate = DateOnly.Parse("04/04/2002");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingqtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Category == "Unable to set QTLSDate");

        // Assert
        Assert.NotNull(task);
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Equal($"Unable to set QTLSDate {incomingqtlsDate}, this matches existing QTLS date on teacher record", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_SetQtlsWithActiveAlert_SetsQtlsAndCreatesReviewTask()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("04/04/2001");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithSanction("G1"));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Category == "QTLS date set/removed for record with an active alert");

        // Assert
        Assert.NotNull(task);
        Assert.Equal("QTLS date set/removed for record with an active alert", task.Category);
        Assert.Equal($"QTLSDate {incomingqtlsDate} set for a record with active alert", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusIsNotYetCompleted_SetsInductionStatusNotYetCompleted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.NotYetCompleted;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.NotYetCompleted, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_InductionExtendedWithoutActiveInductionPeriod_SetsInductionStatusExempt()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_InductionExtendedForAppropriateBodyWithoutActiveInductionPeriod_SetsInductionStatusExempt()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, Clock.Today.AddMonths(-1), Clock.Today.AddDays(-1), account.AccountId));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusIsInductionExtendedWithAppropriateBody_SetsInductionStatusBackToInductionExtended()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtlsDate = DateOnly.Parse("04/04/2019");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithQtlsDate(existingQtlsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, account.AccountId));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.InductionExtended, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusIsInductionExtendedWithAB_SetsInductionStatusInductionExtended()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("someaccount"));
        var existingInductionStatus = dfeta_InductionStatus.InductionExtended;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("04/04/2011");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, account.AccountId));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal($"Unable to set QTLSDate {incomingqtlsDate}, teacher induction currently set to 'Induction Extended' claimed with an AB", task.Description);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.InductionExtended, contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusIsExempt_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Exempt;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact!.dfeta_InductionStatus);
        Assert.Equal(incomingqtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(incomingqtlsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusIsExempt_SetsInductionStatusExempt()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Exempt;
        var existingQtlsDate = DateOnly.Parse("04/04/2014");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
        Assert.Null(contact.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusIsPass_SetsInductionStatusPass()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Pass;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Pass, contact.dfeta_InductionStatus);
        Assert.Equal(incomingqtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(incomingqtlsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusPass_SetsInductionStatusPass()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Pass;
        var existingQtlsDate = DateOnly.Parse("04/04/2014");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Pass, contact.dfeta_InductionStatus);
        Assert.Null(contact.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusFail_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Fail;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/01/1992");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Fail, contact.dfeta_InductionStatus);
        Assert.Null(contact.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Equal($"Unable to set QTLSDate {incomingqtlsDate}, teacher induction currently set to 'Fail'", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusFail_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.Fail;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var existingQtlsDate = DateOnly.Parse("04/04/1991");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Fail, contact.dfeta_InductionStatus);
        Assert.Equal(existingQtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(existingQtlsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Equal($"Unable to set QTLSDate {incomingqtlsDate}, teacher induction currently set to 'Fail'", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusIsPassedInWales_SetsInductionStatusPassedInWales()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.PassedinWales;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/07/1997");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.PassedinWales, contact.dfeta_InductionStatus);
        Assert.Equal(incomingqtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(incomingqtlsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusPassedInWales_SetsInductionStatusPassedInWales()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.PassedinWales;
        var existingQtlsDate = DateOnly.Parse("04/04/2014");
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: dfeta_InductionExemptionReason.Exempt, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.PassedinWales, contact.dfeta_InductionStatus);
        Assert.Null(contact.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }

    [Fact]
    public async Task Put_SetQtlsWhenInductionStatusFailedInWales_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.FailedinWales;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var incomingqtlsDate = DateOnly.Parse("01/01/1992");
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.FailedinWales, contact.dfeta_InductionStatus);
        Assert.Null(contact.dfeta_qtlsdate);
        Assert.Equal(existingQtsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Equal($"Unable to set QTLSDate {incomingqtlsDate}, teacher induction currently set to 'Failed in Wales'", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
    }

    [Fact]
    public async Task Put_ClearQtlsWhenInductionStatusFailedInWales_ReturnsAccepted()
    {
        // Arrange
        var existingInductionStatus = dfeta_InductionStatus.FailedinWales;
        var existingQtsDate = DateOnly.Parse("04/04/2016");
        var existingQtlsDate = DateOnly.Parse("04/04/1991");
        var incomingqtlsDate = default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(existingQtsDate)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: null, null, null, null, null, null)
            .WithQtlsDate(existingQtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incomingqtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId.Id == person.PersonId && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.NotNull(task);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(dfeta_InductionStatus.Exempt, contact.dfeta_InductionStatus);
        Assert.Equal(existingQtlsDate, contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(existingQtlsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal("Unable to set QTLSDate", task.Category);
        Assert.Equal($"Unable to remove QTLSDate, teacher induction currently set to 'Failed in Wales'", task.Description);
        Assert.Equal("Notification for SET QTLS data collections team", task.Subject);
    }

    [Fact]
    public async Task Put_ValidQtlsWithNoQts_SetsQtsDate()
    {
        // Arrange
        var qtlsDate = new DateOnly(2010, 01, 01);
        var incommingQtlsDate = new DateOnly(2009, 01, 01);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQtlsDate(qtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = incommingQtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId == person.ContactId.ToEntityReference(Contact.EntityLogicalName) && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(incommingQtlsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task Put_RemoveQtlsDate_RevertsQtsDate()
    {
        // Arrange
        var qtlsDate = new DateOnly(2010, 01, 01);
        var qtsDate = new DateOnly(2008, 01, 01);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate)
            .WithQtlsDate(qtlsDate));

        var requestBody = CreateJsonContent(new { qtsDate = default(DateOnly?) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId == person.ContactId.ToEntityReference(Contact.EntityLogicalName) && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(qtsDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task Put_QtlsDateAfterAllQtsDates_SetsQtsDate()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var qtsDate1 = new DateOnly(2010, 01, 01);
        var earliestQTSDate = new DateOnly(2008, 01, 01);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate1)
            .WithQts(earliestQTSDate));

        var requestBody = CreateJsonContent(new { qtsDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(earliestQTSDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task Put_QtlsDateBeforeAllQtsDates_SetsQtsDate()
    {
        // Arrange
        var earliestQTLSDate = new DateOnly(2001, 01, 01);
        var qtsDate1 = new DateOnly(2010, 01, 01);
        var qtsDate2 = new DateOnly(2008, 01, 01);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate1)
            .WithQts(qtsDate2));

        var requestBody = CreateJsonContent(new { qtsDate = earliestQTLSDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);
        var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().FirstOrDefault(x => x.RegardingObjectId == person.ContactId.ToEntityReference(Contact.EntityLogicalName) && x.Subject == "Notification for SET QTLS data collections team");

        // Assert
        Assert.Null(task);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(earliestQTLSDate, contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
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
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQts(qtsDate1)
            .WithQts(qtsDate2)
            .WithQtlsDate(qtlsDate));

        var requestBody = CreateJsonContent(new { QtsDate = default(DateOnly?) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().FirstOrDefault(x => x.ContactId == person.PersonId);

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
    public async Task Put_ValidQtlsDateWithNoQts_SetsInductionStatus(dfeta_InductionStatus existingInductionStatus, dfeta_InductionExemptionReason? existingInductionExemptionReason, string? incomingQtls, dfeta_InductionStatus expectetInductionStatus, HttpStatusCode expectedHttpStatus)
    {
        // Arrange
        var qtlsDate = !string.IsNullOrEmpty(incomingQtls) ? DateOnly.Parse(incomingQtls) : default(DateOnly?);
        var person = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithInduction(inductionStatus: existingInductionStatus, inductionExemptionReason: existingInductionExemptionReason, null, null, null, null, null));

        var requestBody = CreateJsonContent(new { qtsDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{person.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Single(x => x.ContactId == person.PersonId);

        // Assert
        Assert.Equal(expectedHttpStatus, response.StatusCode);
        Assert.Equal(expectetInductionStatus, contact.dfeta_InductionStatus);
    }
}
