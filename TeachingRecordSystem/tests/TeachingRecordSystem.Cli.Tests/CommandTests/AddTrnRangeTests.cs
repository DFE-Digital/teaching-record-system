using System.CommandLine;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

[SharedDependenciesDataSource]
public class AddTrnRangeTests(IConfiguration configuration, DbFixture dbFixture)
{
    [Before(Test)]
    public Task ClearTrnRanges() =>
        dbFixture.WithDbContextAsync(dbContext => dbContext.TrnRanges.ExecuteDeleteAsync());

    [Test]
    [Arguments(999999)]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(10000000)]
    public async Task InvalidFrom_ReturnsError(int from)
    {
        // Arrange
        var to = 9999999;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse("add-trn-range", "--from", $"{from}", "--to", $"{to}");

        // Assert
        await Assert.That(parseResult.Errors).Contains(e => e.Message == "--from must be between 1000000 and 9999999.");
    }

    [Test]
    [Arguments(999999)]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(10000000)]
    public async Task InvalidTo_ReturnsError(int to)
    {
        // Arrange
        var from = 1000000;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse("add-trn-range", "--from", $"{from}", "--to", $"{to}");

        // Assert
        await Assert.That(parseResult.Errors).Contains(e => e.Message == "--to must be between 1000000 and 9999999.");
    }

    [Test]
    public async Task ToEqualToFrom_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = from;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse("add-trn-range", "--from", $"{from}", "--to", $"{to}");

        // Assert
        await Assert.That(parseResult.Errors).Contains(e => e.Message == "--to must be greater than --from.");
    }

    [Test]
    public async Task ToLessThanFrom_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = 1999999;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse("add-trn-range", "--from", $"{from}", "--to", $"{to}");

        // Assert
        await Assert.That(parseResult.Errors).Contains(e => e.Message == "--to must be greater than --from.");
    }

    [Test]
    public async Task NextLessThanFrom_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = 3000000;
        var next = 1999999;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse("add-trn-range", "--from", $"{from}", "--to", $"{to}", "--next", $"{next}");

        // Assert
        await Assert.That(parseResult.Errors).Contains(e => e.Message == "--next must be between --from and --to.");
    }

    [Test]
    public async Task NextGreaterThanTo_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = 3000000;
        var next = 3000001;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse("add-trn-range", "--from", $"{from}", "--to", $"{to}", "--next", $"{next}");

        // Assert
        await Assert.That(parseResult.Errors).Contains(e => e.Message == "--next must be between --from and --to.");
    }

    [Test]
    [NotInParallel]
    public async Task ValidInvocationWithoutNext_AddsTrnRangeToDb()
    {
        // Arrange
        var from = 1000000;
        var to = 9999999;

        var command = GetCommand();

        // Act
        var result = await command.InvokeAsync(["add-trn-range", "--from", $"{from}", "--to", $"{to}"]);

        // Assert
        await Assert.That(result).IsEqualTo(0);

        await dbFixture.WithDbContextAsync(async dbContext =>
        {
            var trnRange = await dbContext.TrnRanges.SingleOrDefaultAsync();
            await Assert.That(trnRange).IsNotNull();
            using var _ = Assert.Multiple();
            await Assert.That(trnRange!.FromTrn).IsEqualTo(from);
            await Assert.That(trnRange.ToTrn).IsEqualTo(to);
            await Assert.That(trnRange.NextTrn).IsEqualTo(from);
            await Assert.That(trnRange.IsExhausted).IsFalse();
        });
    }

    [Test]
    [NotInParallel]
    public async Task ValidInvocationWithExplicitNext_AddsTrnRangeToDb()
    {
        // Arrange
        var from = 1000000;
        var to = 9999999;
        var next = 1000001;

        var command = GetCommand();

        // Act
        var result = await command.InvokeAsync(["add-trn-range", "--from", $"{from}", "--to", $"{to}", "--next", $"{next}"]);

        // Assert
        await Assert.That(result).IsEqualTo(0);

        await dbFixture.WithDbContextAsync(async dbContext =>
        {
            var trnRange = await dbContext.TrnRanges.SingleOrDefaultAsync();
            await Assert.That(trnRange).IsNotNull();
            using var _ = Assert.Multiple();
            await Assert.That(trnRange!.FromTrn).IsEqualTo(from);
            await Assert.That(trnRange.ToTrn).IsEqualTo(to);
            await Assert.That(trnRange.NextTrn).IsEqualTo(next);
            await Assert.That(trnRange.IsExhausted).IsFalse();
        });
    }

    private Command GetCommand() => Commands.CreateAddTrnRangeCommand(configuration);
}
