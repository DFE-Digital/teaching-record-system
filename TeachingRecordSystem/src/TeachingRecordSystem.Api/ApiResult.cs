using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api;

public sealed class ApiResult<TResult> where TResult : notnull
{
    private readonly TResult? _result;
    private readonly ApiError? _error;

    public ApiResult(TResult result) : this(result, null)
    {
    }

    public ApiResult(ApiError error) : this(default, error)
    {
    }

    private ApiResult(TResult? result, ApiError? error)
    {
        Debug.Assert(result is not null ^ error is not null);
        _result = result;
        _error = error;
    }

    public bool IsError => _error is not null;

    public static implicit operator ApiResult<TResult>(ApiError error) => new(error);

    public static implicit operator ApiResult<TResult>(TResult result) => new(result);

    public ApiError GetError() =>
        _error ?? throw new InvalidOperationException("ApiResult is not in an error state.");

    public TResult GetSuccess() =>
        _result ?? throw new InvalidOperationException("ApiResult is not in a success state.");

    public ApiResultActionResultBuilder<TResult> ToActionResult(Func<TResult, IActionResult> mapSuccess) => new(this, mapSuccess);
}

public class ApiResultActionResultBuilder<TResult>(ApiResult<TResult> result, Func<TResult, IActionResult> mapSuccess) : IActionResult
    where TResult : notnull
{
    private readonly Dictionary<int, int> _errorCodeStatusCodeMappings = [];
    private readonly Dictionary<int, Func<ApiError, IActionResult>> _errorCodeResultMappings = [];

    public Task ExecuteResultAsync(ActionContext context)
    {
        if (result.IsError)
        {
            var error = result.GetError();

            var actionResult = _errorCodeResultMappings.TryGetValue(error.ErrorCode, out var createErrorResult)
                ? createErrorResult(error)
                : (error.ToActionResult(_errorCodeStatusCodeMappings.TryGetValue(error.ErrorCode, out var sc) ? sc : StatusCodes.Status400BadRequest));

            return actionResult.ExecuteResultAsync(context);
        }

        return mapSuccess(result.GetSuccess()).ExecuteResultAsync(context);
    }

    public ApiResultActionResultBuilder<TResult> MapErrorCode(int errorCode, int statusCode)
    {
        _errorCodeStatusCodeMappings[errorCode] = statusCode;
        return this;
    }

    public ApiResultActionResultBuilder<TResult> MapErrorCode(int errorCode, Func<ApiError, IActionResult> mapError)
    {
        _errorCodeResultMappings[errorCode] = mapError;
        return this;
    }
}

public sealed class Unit
{
    private Unit() { }

    public static Unit Instance { get; } = new();
}
