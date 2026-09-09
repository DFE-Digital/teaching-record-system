using System.CommandLine;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class PersonTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task Unmerge_ReactivatesPersonAndPublishesPersonUnmergedEventInAnUnmergingProcess()
    {
        // Arrange
        var (personToRetain, personToUnmerge) = await CreateMergedPersonsAsync();

        var command = GetSubcommand("unmerge");
        var parseResult = command.Parse($"--trn {personToUnmerge.Trn}");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons
                .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                .SingleAsync(p => p.PersonId == personToUnmerge.PersonId);

            Assert.Equal(PersonStatus.Active, updatedPerson.Status);
            Assert.Null(updatedPerson.MergedWithPersonId);
        });

        var (process, processEvent) = await GetProcessAndEventAsync(personToUnmerge.PersonId);

        Assert.Equal(ProcessType.PersonUnmerging, process.ProcessType);
        Assert.Equal(SystemUser.SystemUserId, process.UserId);

        var unmergedEvent = Assert.IsType<PersonUnmergedEvent>(processEvent.Payload);
        Assert.Equal(personToUnmerge.PersonId, unmergedEvent.PersonId);
        Assert.Equal(personToRetain.PersonId, unmergedEvent.UnmergedFromPersonId);
    }

    [Fact]
    public async Task Unmerge_WritesWarningThatOneLoginUsersHaveNotBeenMovedBack()
    {
        // Arrange
        var (personToRetain, personToUnmerge) = await CreateMergedPersonsAsync();

        var command = GetSubcommand("unmerge");
        var parseResult = command.Parse($"--trn {personToUnmerge.Trn}");

        var output = new StringWriter();

        // Act
        var result = await parseResult.InvokeAsync(new InvocationConfiguration { Output = output });

        // Assert
        Assert.Equal(0, result);
        Assert.Contains($"Un-merged TRN {personToUnmerge.Trn} from TRN {personToRetain.Trn}.", output.ToString());
        Assert.Contains(
            $"WARNING: any One Login users that were linked to TRN {personToUnmerge.Trn} before the merge remain linked to TRN {personToRetain.Trn} and have not been moved back.",
            output.ToString());
    }

    [Fact]
    public async Task Unmerge_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = GetSubcommand("unmerge");
        var parseResult = command.Parse("--trn 1234567");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Unmerge_PersonHasNotBeenMerged_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var command = GetSubcommand("unmerge");
        var parseResult = command.Parse($"--trn {person.Trn}");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(1, result);
    }

    private Command GetSubcommand(string name) =>
        Commands.CreatePersonCommand(Configuration).Subcommands.Single(c => c.Name == name);

    private async Task<(Person PersonToRetain, Person PersonToUnmerge)> CreateMergedPersonsAsync()
    {
        var personToRetain = await TestData.CreatePersonAsync();
        var personToUnmerge = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToUnmerge);
            personToUnmerge.Status = PersonStatus.Deactivated;
            personToUnmerge.MergedWithPersonId = personToRetain.PersonId;
            await dbContext.SaveChangesAsync();
        });

        return (personToRetain, personToUnmerge);
    }

    // Finds the single process event raised for the given person, along with the process it belongs to.
    private Task<(Core.DataStore.Postgres.Models.Process Process, ProcessEvent ProcessEvent)> GetProcessAndEventAsync(Guid personId) =>
        WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(PersonUnmergedEvent) && pe.PersonIds.Contains(personId))
                .ToListAsync();

            var processEvent = Assert.Single(processEvents);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            return (process, processEvent);
        });
}
