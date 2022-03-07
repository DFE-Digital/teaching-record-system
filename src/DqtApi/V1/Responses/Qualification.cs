using System;
using System.Text.Json.Serialization;

namespace DqtApi.V1.Responses
{
    public class Qualification
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("date_awarded")]
        public DateTime? DateAwarded { get; set; }
    }
}
