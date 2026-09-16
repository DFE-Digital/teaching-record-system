using System.CommandLine;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class ResetTrnRequestTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task ResetTrnRequest_TrnRequestDoesNotExist_ReturnsError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(applicationUser.UserId, Guid.NewGuid().ToString(), error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("TRN request was not found", error.ToString());
    }

    [Fact]
    public async Task ResetTrnRequest_TrnRequestHasAnOpenSupportTask_ReturnsError()
    {
        // Arrange
        var (applicationUserId, trnRequest, _) = await CreateResolvedTrnRequestAsync(SupportTaskStatus.Open);
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(applicationUserId, trnRequest.RequestId, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("TRN request already has open support tasks", error.ToString());
    }

    [Fact]
    public Task ResetTrnRequest_RequestHasPotentialMatches_ResetsRequestAndCreatesSupportTask() =>
        AssertResetsRequestAndCreatesSupportTaskAsync(MatchScenario.PotentialMatches);

    [Fact]
    public Task ResetTrnRequest_RequestHasADefiniteMatch_ResetsRequestAndCreatesSupportTask() =>
        AssertResetsRequestAndCreatesSupportTaskAsync(MatchScenario.DefiniteMatch);

    [Fact]
    public Task ResetTrnRequest_RequestHasNoMatches_ResetsRequestAndCreatesSupportTask() =>
        AssertResetsRequestAndCreatesSupportTaskAsync(MatchScenario.NoMatches);

    private async Task AssertResetsRequestAndCreatesSupportTaskAsync(MatchScenario matchScenario)
    {
        // Arrange
        var (applicationUserId, trnRequest, closedSupportTaskReference) =
            await CreateResolvedTrnRequestAsync(SupportTaskStatus.Closed, matchScenario);
        var output = new StringWriter();

        // Act
        var result = await InvokeAsync(applicationUserId, trnRequest.RequestId, output: output);

        // Assert
        Assert.Equal(0, result);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedRequest = await dbContext.TrnRequestMetadata
                .SingleAsync(r => r.ApplicationUserId == applicationUserId && r.RequestId == trnRequest.RequestId);

            Assert.Equal(TrnRequestStatus.Pending, updatedRequest.Status);
            Assert.Null(updatedRequest.ResolvedPersonId);

            // Matching runs again as part of the reset and only flags a potential duplicate when it finds
            // more than one candidate, which also confirms each scenario set up the outcome it intended.
            Assert.Equal(matchScenario is MatchScenario.PotentialMatches, updatedRequest.PotentialDuplicate);

            var newSupportTask = await dbContext.SupportTasks
                .SingleAsync(t =>
                    t.TrnRequestApplicationUserId == applicationUserId &&
                    t.TrnRequestId == trnRequest.RequestId &&
                    t.SupportTaskReference != closedSupportTaskReference);

            Assert.Equal(SupportTaskType.TrnRequest, newSupportTask.SupportTaskType);
            Assert.Equal(SupportTaskStatus.Open, newSupportTask.Status);
            Assert.Contains($"Created new {SupportTaskType.TrnRequest} support task: {newSupportTask.SupportTaskReference}", output.ToString());
        });
    }

    private enum MatchScenario
    {
        NoMatches,
        PotentialMatches,
        DefiniteMatch
    }

    private async Task<(Guid ApplicationUserId, TrnRequestMetadata TrnRequest, string SupportTaskReference)> CreateResolvedTrnRequestAsync(
        SupportTaskStatus supportTaskStatus,
        MatchScenario matchScenario = MatchScenario.PotentialMatches)
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        // How many people share the request's details decides the outcome: two leaves every candidate a potential
        // match, one that also shares the national insurance number is a definite match, and none leaves matching
        // with nothing to find.
        var matchingPeopleCount = matchScenario switch
        {
            MatchScenario.NoMatches => 0,
            MatchScenario.DefiniteMatch => 1,
            _ => 2
        };

        var matchingPeople = await Task.WhenAll(
            Enumerable.Range(0, matchingPeopleCount).Select(_ => TestData.CreatePersonAsync(p =>
            {
                p.WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth);

                if (matchScenario is MatchScenario.DefiniteMatch)
                {
                    p.WithNationalInsuranceNumber(nationalInsuranceNumber);
                }
            })));

        // The request still has to resolve to someone; with no matching people that's an unrelated record.
        var resolvedPerson = matchingPeople.FirstOrDefault() ?? await TestData.CreatePersonAsync();

        var createResult = await TestData.CreateTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithMatchedPersons(matchingPeople.Select(p => p.PersonId).ToArray())
                .WithResolvedPersonId(resolvedPerson.PersonId)
                .WithStatus(supportTaskStatus));

        return (applicationUser.UserId, createResult.TrnRequest, createResult.SupportTask.SupportTaskReference);
    }

    private Task<int> InvokeAsync(
        Guid sourceApplicationUserId,
        string trnRequestId,
        TextWriter? output = null,
        TextWriter? error = null)
    {
        var command = Commands.CreateResetTrnRequestCommand(Configuration);

        var parseResult = command.Parse([
            "--trn-request-id", trnRequestId,
            "--source-application-user-id", sourceApplicationUserId.ToString(),
            "--reason", "Testing"
        ]);

        return parseResult.InvokeAsync(
            new InvocationConfiguration
            {
                Output = output ?? TextWriter.Null,
                Error = error ?? TextWriter.Null
            });
    }
}
