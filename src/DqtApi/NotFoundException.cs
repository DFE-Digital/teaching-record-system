using System;

namespace DqtApi
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string resourceName, object resourceId)
            : base(GetMessage(resourceName, resourceId))
        {
        }

        private static string GetMessage(string resourceName, object resourceId) => $"The {resourceName} with ID {resourceId} was not found.";
    }
}
