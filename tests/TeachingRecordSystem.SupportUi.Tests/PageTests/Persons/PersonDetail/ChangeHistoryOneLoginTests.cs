using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public partial class ChangeHistoryTests
{
    [Fact]
    public async Task Get_PersonOneLoginUserConnecting_RendersExpectedEntry()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(matchedPerson);
        var user = await TestData.CreateUserAsync();
        var process = new ProcessContext(ProcessType.PersonOneLoginUserConnecting, TimeProvider.UtcNow, user.UserId);

        await OneLoginService.SetUserMatchedAsync(
            new SetUserMatchedOptions
            {
                OneLoginUserSubject = oneLoginUser.Subject!,
                MatchedPersonId = matchedPerson.PersonId,
                MatchRoute = OneLoginUserMatchRoute.SupportUi,
                MatchedAttributes = [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd"))
                ]
            },
            process);


        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{matchedPerson.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "OneLogin Connected",
            user.Name,
            process.Now,
            [
                ("Event Type", "Manual Connection")
            ]);
    }

    [Fact]
    public async Task Get_PersonOneLoginUserDisconnecting_RendersExpectedEntry()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(matchedPerson);
        var user = await TestData.CreateUserAsync();
        var reason = new ChangeReasonWithDetailsAndEvidence()
        {
            Details = "these are details",
            EvidenceFile = null,
            Reason = "this is the reason",
            AdditionalInformation = null
        };
        var process = new ProcessContext(ProcessType.PersonOneLoginUserDisconnecting, TimeProvider.UtcNow, user.UserId, reason);

        await OneLoginService.SetUserUnmatchedAsync(oneLoginUser.Subject!, process);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{matchedPerson.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "OneLogin Disconnected",
            user.Name,
            process.Now,
            [
                ("Event Type", "OneLogin Disconnected"),
                ("Reason", reason.Details)
            ]);
    }

    [Fact]
    public async Task Get_OneLoginUserRecordMatchingSupportTaskCompleting_RendersExpectedEntry()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true, email: Option.Some(matchedPerson.EmailAddress));
        var user = await TestData.CreateUserAsync();

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var process = new ProcessContext(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, TimeProvider.UtcNow, user.UserId);
        await OneLoginSupportTaskService.ResolveRecordMatchingSupportTaskAsync(
            new ConnectedOutcomeOptions
            {
                MatchedPersonId = matchedPerson.PersonId,
                MatchedAttributes =
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd"))
                ],
                SupportTask = supportTask,
                Trn = matchedPerson.Trn
            },
            process);


        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{matchedPerson.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "OneLogin Connected",
            user.Name,
            process.Now,
            [
                ("Event Type", "Task Connection")
            ]);
    }

    [Fact]
    public async Task Get_OneLoginUserVerificationTaskCompleting_RendersExpectedEntry()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var options = new VerifiedAndConnectedOutcomeOptions
        {
            SupportTask = supportTask,
            MatchedPersonId = matchedPerson.PersonId,
            MatchedAttributes =
            [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
            ]
        };

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, TimeProvider.UtcNow, SystemUser.SystemUserId);

        await OneLoginSupportTaskService.ResolveVerificationSupportTaskAsync(options, processContext);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{matchedPerson.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            processContext.ProcessId,
            "OneLogin Connected",
            SystemUser.SystemUserName,
            processContext.Now,
            [
                ("Event Type", "Task Connection")
            ]);
    }
}
