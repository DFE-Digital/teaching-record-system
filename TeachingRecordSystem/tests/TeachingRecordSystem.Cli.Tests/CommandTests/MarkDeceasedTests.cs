using System.CommandLine;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class MarkDeceasedTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task MissingTrnOption_ReturnsError()
    {
        // Arrange
        var command = GetCommand();

        var parseResult = command.Parse([
            "mark-deceased"
        ]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--trn' is required.");
    }

    [Fact]
    public async Task MissingDateOfDeathOption_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var command = GetCommand();

        var parseResult = command.Parse([
            "mark-deceased", "--trn", person.Trn
        ]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--date-of-death' is required.");
    }

    [Fact]
    public async Task ValidInvocation_DeactivatesPerson()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dateOfDeath = Clock.Today.AddDays(-1);

        var command = GetCommand();

        var parseResult = command.Parse([
            "mark-deceased", "--trn", person.Trn,"--date-of-death", dateOfDeath.ToString()
        ]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);
        await WithDbContextAsync(async dbContext =>
        {
            var updatePerson = await dbContext.Persons
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.Trn == person.Trn);

            Assert.NotNull(updatePerson?.DateOfDeath);
            Assert.Equal(dateOfDeath, updatePerson.DateOfDeath);
        });
    }

    private Command GetCommand() => Commands.MarkDeceasedCommand(Configuration);
}

