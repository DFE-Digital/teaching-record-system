﻿using System;

namespace DqtApi.Validation
{
    public class ErrorException : Exception
    {
        public ErrorException(Error error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public Error Error { get; }

        public override string Message => Error.Title;
    }
}
