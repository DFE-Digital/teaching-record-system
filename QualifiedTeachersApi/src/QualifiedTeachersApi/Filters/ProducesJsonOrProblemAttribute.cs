#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QualifiedTeachersApi.Filters;

public sealed class ProducesJsonOrProblemAttribute : ProducesAttribute
{
    public ProducesJsonOrProblemAttribute() : base("application/json", "application/problem+json")
    {
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is ProblemDetails)
        {
            // Don't override Content-Type; ProblemDetails will get application/problem+json
            return;
        }

        base.OnResultExecuting(context);
    }
}
