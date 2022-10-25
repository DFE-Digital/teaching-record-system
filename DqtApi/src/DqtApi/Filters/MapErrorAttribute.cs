﻿using DqtApi.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DqtApi.Filters
{
    public class MapErrorAttribute : ExceptionFilterAttribute
    {
        public MapErrorAttribute(int errorCode, int statusCode = StatusCodes.Status400BadRequest)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; }

        public int StatusCode { get; }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ErrorException ex && ErrorCode == ex.Error.ErrorCode)
            {
                context.Result = ex.ToResult(StatusCode);
                context.ExceptionHandled = true;
            }
        }
    }
}
