using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.AuthorizeAccess.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    public IActionResult Error(int? code)
    {
        // If there is no error, return a 404
        // (prevents browsing to this page directly)
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        if (exceptionHandlerPathFeature == null && statusCodeReExecuteFeature == null)
        {
            return NotFound();
        }

        var statusCode = code ?? 500;

        // Treat Forbidden as NotFound so we don't give away our internal URLs
        if (code == 403)
        {
            statusCode = 404;
        }

        var viewName = statusCode == 404 ? "NotFound" : "GenericError";
        var result = View(viewName);
        result.StatusCode = statusCode;
        return result;
    }
}
