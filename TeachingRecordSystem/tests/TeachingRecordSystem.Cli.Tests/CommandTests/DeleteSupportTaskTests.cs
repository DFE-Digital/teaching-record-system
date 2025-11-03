using System.CommandLine;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class DeleteSupportTaskTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task ValidInvocation_DeletesSupportTask()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);

        var command = GetCommand();

        var parseResult = command.Parse(["delete-support-task", "--ref", $"{supportTask.SupportTaskReference}", "--reason", "Testing"]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedTask = await dbContext.SupportTasks
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);

            Assert.NotNull(updatedTask?.DeletedOn);  // TODO Figure out how to pass TestableClock to Command
        });
    }

    [Fact]
    public async Task TaskDoesNotExist_ReturnsError()
    {
        // Arrange
        var supportTaskReference = SupportTask.GenerateSupportTaskReference();

        var command = GetCommand();

        var parseResult = command.Parse(["delete-support-task", "--ref", $"{supportTaskReference}", "--reason", "Testing"]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(1, result);
    }

    private Command GetCommand() => Commands.CreateDeleteSupportTaskCommand(Configuration);
}
