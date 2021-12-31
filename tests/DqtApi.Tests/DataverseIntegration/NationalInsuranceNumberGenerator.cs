using System.Linq;
using DqtApi.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DqtApi.Tests.DataverseIntegration
{
    public class NationalInsuranceNumberGenerator
    {
        private readonly IOrganizationService _service;

        private readonly string _first = "AA111111A";

        public NationalInsuranceNumberGenerator(IOrganizationService service)
        {
            _service = service;
        }

        public string GetNextAvailable(string previous = null)
        {
            var needle = (previous == null) ? _first : new NationalInsuranceNumber(previous).Increment();

            var query = new QueryByAttribute(Contact.EntityLogicalName);
            query.AddAttributeValue(Contact.Fields.dfeta_NINumber, needle);

            var entities = _service.RetrieveMultiple(query).Entities;

            if (!entities.Any())
            {
                return needle;
            }

            return GetNextAvailable(needle);
        }

        private struct NationalInsuranceNumber
        {
            private readonly string _text;

            public NationalInsuranceNumber(string text)
            {
                _text = text;
            }

            public string Prefix => _text.Substring(0, 2);
            public string Suffix => _text.Substring(8, 1);
            public int Number => int.Parse(_text.Substring(2, 6));

            // This assumes that we can safely increment just the numeric part of the NIN
            // in order to get a unique value within the environment
            public string Increment()
            {
                return $"{Prefix}{Number + 1}{Suffix}";
            }
        }
    }
}
