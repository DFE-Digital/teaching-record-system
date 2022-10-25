using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Responses
{
    public class GetIttProvidersResponse
    {
        [SwaggerSchema(Nullable = false)]
        public IEnumerable<IttProviderInfo> IttProviders { get; set; }
    }
}
