using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QualifiedTeachersApi;

public class CamelCaseErrorKeysProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly ProblemDetailsFactory _innerFactory;

    public CamelCaseErrorKeysProblemDetailsFactory(ProblemDetailsFactory innerFactory)
    {
        _innerFactory = innerFactory;
    }

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string title = null,
        string type = null,
        string detail = null,
        string instance = null)
    {
        return _innerFactory.CreateProblemDetails(httpContext, statusCode, title, type, detail, instance);
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string title = null,
        string type = null,
        string detail = null,
        string instance = null)
    {
        var problemDetails = _innerFactory.CreateValidationProblemDetails(httpContext, modelStateDictionary, statusCode, title, type, detail, instance);

        // All our property names are camel cased; ensure error keys are camel cased too

        foreach (var errorKey in problemDetails.Errors.Keys.ToArray())
        {
            var errors = problemDetails.Errors[errorKey];
            problemDetails.Errors.Remove(errorKey);

            var camelCasedKey = string.Join(".", errorKey.Split(".").Select(System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName));

            problemDetails.Errors.Add(camelCasedKey, errors);
        }

        return problemDetails;
    }
}
