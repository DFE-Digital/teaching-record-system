using System.Text.Json.Serialization;
using Microsoft.Xrm.Sdk;

namespace DqtApi.Responses
{
    public abstract class LinkedEntity<T> where T : Entity, new()
    {
        [JsonIgnore]
        public T Entity { get; set; }

        [JsonIgnore]
        public FormattedValueCollection FormattedValues { get; set; }   
    }
}
