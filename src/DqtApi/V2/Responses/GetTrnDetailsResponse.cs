using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Responses
{
    public class GetTrnDetailsResponse
    {
        [SwaggerSchema(Nullable = false)]
        public IEnumerable<TrnDetails> Details { get; set; }
    }
}
