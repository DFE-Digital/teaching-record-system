using FastEndpoints;

namespace TeachingRecordSystem.Api.Infrastructure.FastEndpoints;

public static class Parsers
{
    public static ParseResult DateOnlyParser(object? input)
    {
        var success = DateOnly.TryParseExact(input?.ToString(), "yyyy-MM-dd", out var result);
        return new ParseResult(success, result);
    }

    public static ParseResult NullableDateOnlyParser(object? input)
    {
        if (string.IsNullOrEmpty(input?.ToString()))
        {
            return new ParseResult(isSuccess: true, (DateOnly?)null);
        }

        var success = DateOnly.TryParseExact(input.ToString(), "yyyy-MM-dd", out var result);
        return new ParseResult(success, result);
    }
}
