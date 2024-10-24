using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Controllers;

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

        var viewName = statusCode switch
        {
            403 => "Forbidden",
            404 => "NotFound",
            _ => "GenericError"
        };

        var result = View(viewName);
        result.StatusCode = statusCode;
        return result;
    }
}
