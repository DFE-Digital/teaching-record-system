using System.CommandLine;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class AddTrnRangeTests(IServiceProvider services) : CommandTestBase(services), IAsyncLifetime
{
    async ValueTask IAsyncLifetime.InitializeAsync() =>
        await WithDbContextAsync(dbContext => dbContext.TrnRanges.ExecuteDeleteAsync());

    async ValueTask IAsyncDisposable.DisposeAsync() => await DbHelper.ClearDataAsync();  // Restore TRN ranges for other tests

    [Theory]
    [InlineData(999999)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000000)]
    public void InvalidFrom_ReturnsError(int from)
    {
        // Arrange
        var to = 9999999;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "--from must be between 1000000 and 9999999.");
    }

    [Theory]
    [InlineData(999999)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000000)]
    public void InvalidTo_ReturnsError(int to)
    {
        // Arrange
        var from = 1000000;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "--to must be between 1000000 and 9999999.");
    }

    [Fact]
    public void ToEqualToFrom_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = from;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "--to must be greater than --from.");
    }

    [Fact]
    public void ToLessThanFrom_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = 1999999;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "--to must be greater than --from.");
    }

    [Fact]
    public void NextLessThanFrom_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = 3000000;
        var next = 1999999;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}", "--next", $"{next}"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "--next must be between --from and --to.");
    }

    [Fact]
    public void NextGreaterThanTo_ReturnsError()
    {
        // Arrange
        var from = 2000000;
        var to = 3000000;
        var next = 3000001;

        var command = GetCommand();

        // Act
        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}", "--next", $"{next}"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "--next must be between --from and --to.");
    }

    [Fact]
    public async Task ValidInvocationWithoutNext_AddsTrnRangeToDb()
    {
        // Arrange
        var from = 1000000;
        var to = 9999999;

        var command = GetCommand();

        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}"]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        await WithDbContextAsync(async dbContext =>
        {
            var trnRange = await dbContext.TrnRanges.SingleOrDefaultAsync();
            Assert.NotNull(trnRange);
            Assert.Equal(from, trnRange.FromTrn);
            Assert.Equal(to, trnRange.ToTrn);
            Assert.Equal(from, trnRange.NextTrn);
            Assert.False(trnRange.IsExhausted);
        });
    }

    [Fact]
    public async Task ValidInvocationWithExplicitNext_AddsTrnRangeToDb()
    {
        // Arrange
        var from = 1000000;
        var to = 9999999;
        var next = 1000001;

        var command = GetCommand();

        var parseResult = command.Parse(["add-trn-range", "--from", $"{from}", "--to", $"{to}", "--next", $"{next}"]);

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        await WithDbContextAsync(async dbContext =>
        {
            var trnRange = await dbContext.TrnRanges.SingleOrDefaultAsync();
            Assert.NotNull(trnRange);
            Assert.Equal(from, trnRange.FromTrn);
            Assert.Equal(to, trnRange.ToTrn);
            Assert.Equal(next, trnRange.NextTrn);
            Assert.False(trnRange.IsExhausted);
        });
    }

    private Command GetCommand() => Commands.CreateAddTrnRangeCommand(Configuration);
}
