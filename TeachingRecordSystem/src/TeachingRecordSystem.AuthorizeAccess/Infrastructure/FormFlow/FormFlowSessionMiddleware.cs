using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;

public class FormFlowSessionMiddleware(RequestDelegate next, IDataProtectionProvider dataProtectionProvider)
{
    private const string CookieName = "ffsessid";

    private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector(nameof(FormFlowSessionMiddleware));

    private readonly CookieOptions _cookieOptions = new()
    {
        HttpOnly = true,
        IsEssential = true,
        Secure = true,
    };

    public async Task Invoke(HttpContext context)
    {
        if (!TryExtractSessionIdFromRequest(context, out var sessionId))
        {
            sessionId = AddNewSessionIdToResponse(context);
        }

        context.Features.Set(new FormFlowSessionIdFeature(sessionId));

        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set<FormFlowSessionIdFeature>(null);
        }
    }

    private string AddNewSessionIdToResponse(HttpContext context)
    {
        var sessionId = CreateSessionId();
        var cookieValue = _dataProtector.Protect(sessionId);

        context.Response.OnStarting(
            state =>
            {
                var (context, cookieValue) = ((HttpContext, string))state;
                context.Response.Cookies.Append(CookieName, cookieValue, _cookieOptions);
                return Task.CompletedTask;
            },
            (context, cookieValue));

        return sessionId.ToString();

        static string CreateSessionId()
        {
            Span<byte> guidBytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(guidBytes);
            return new Guid(guidBytes).ToString();
        }
    }

    private bool TryExtractSessionIdFromRequest(HttpContext context, [NotNullWhen(true)] out string? sessionId)
    {
        if (context.Request.Cookies.TryGetValue(CookieName, out var cookieValue))
        {
            try
            {
                var bytes = _dataProtector.Unprotect(cookieValue);
                sessionId = new Guid(bytes).ToString();
                return true;
            }
            catch (CryptographicException)
            {
            }
        }

        sessionId = null;
        return false;
    }
}

public class FormFlowSessionMiddlewareStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseMiddleware<FormFlowSessionMiddleware>();
        next(app);
    };
}
