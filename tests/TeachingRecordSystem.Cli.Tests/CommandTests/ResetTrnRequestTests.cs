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
    public async Task ResetTrnRequest_ValidInvocation_ResetsRequestAndCreatesSupportTask()
    {
        // Arrange
        var (applicationUserId, trnRequest, closedSupportTaskReference) =
            await CreateResolvedTrnRequestAsync(SupportTaskStatus.Closed);
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

    // Builds a request that resolves to one of two people who match on name and date of birth, so matching returns
    // potential matches rather than a definite match.
    private async Task<(Guid ApplicationUserId, TrnRequestMetadata TrnRequest, string SupportTaskReference)> CreateResolvedTrnRequestAsync(
        SupportTaskStatus supportTaskStatus)
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();

        var people = await Task.WhenAll(
            Enumerable.Range(0, 2).Select(_ => TestData.CreatePersonAsync(p => p
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth))));

        var createResult = await TestData.CreateTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithFirstName(firstName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithMatchedPersons(people.Select(p => p.PersonId).ToArray())
                .WithResolvedPersonId(people[0].PersonId)
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
