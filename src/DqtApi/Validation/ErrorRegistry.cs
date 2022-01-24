using System.Collections.Generic;
using System.Linq;

namespace DqtApi.Validation
{
    public static class ErrorRegistry
    {
        private static readonly Dictionary<int, ErrorDescriptor> _all = new ErrorDescriptor[]
        {
        }.ToDictionary(d => d.ErrorCode, d => d);

        public static Error CreateError(int errorCode)
        {
            var descriptor = _all[errorCode];
            return new Error(descriptor);
        }
    }
}
