using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.UiCommon.FormFlow;

public delegate IActionResult MissingInstanceHandler(
    JourneyDescriptor journeyDescriptor,
    HttpContext httpContext,
    int? statusCode);
