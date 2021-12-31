using System;
using System.Text.Json.Serialization;
using DqtApi.DAL;
using DqtApi.Models;

namespace DqtApi.V1.Responses
{
    public class Qualification
    {
        private readonly dfeta_qualification _qualification;

        public Qualification(dfeta_qualification qualification)
        {
            _qualification = qualification;
        }

        [JsonPropertyName("name")]
        public string Name => _qualification.FormattedValues.ValueOrNull(dfeta_qualification.Fields.dfeta_Type);

        [JsonPropertyName("date_awarded")]
        public DateTime? DateAwarded => _qualification.dfeta_CompletionorAwardDate;
    }
}
