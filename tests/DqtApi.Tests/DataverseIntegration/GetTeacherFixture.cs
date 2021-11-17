using System;
using DqtApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetTeacherFixture : IDisposable
    {
        private readonly ServiceClient _service;

        public IOrganizationServiceAsync Service => _service;
        private readonly string _matchingNationalInsuranceNumber;
        private readonly string _nonmatchingNationalInsuranceNumber;
        private Guid _fixtureId;
        private readonly string _matchingTRN;
        private readonly string _nonmatchingTRN;

        private static DateTime MatchingBirthdate => new(2001, 1, 1);
        private static DateTime NonmatchingBirthdate => new(2002, 2, 2);

        private readonly IConfiguration _configuration;

        public GetTeacherFixture()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<GetTeacherTests>(optional: true)
                .AddEnvironmentVariables("IntegrationTests_");

            _configuration = builder.Build();

            _service = GetCrmServiceClient();

            var nationalInsuranceNumberGenerator = new NationalInsuranceNumberGenerator(_service);
            _matchingNationalInsuranceNumber = nationalInsuranceNumberGenerator.GetNextAvailable();
            _nonmatchingNationalInsuranceNumber = nationalInsuranceNumberGenerator.GetNextAvailable(_matchingNationalInsuranceNumber);

            _fixtureId = GetFixtureId(_matchingNationalInsuranceNumber);

            _matchingTRN = GetTRN();
            _nonmatchingTRN = (int.Parse(_matchingTRN) + 1).ToString();
        }

        private string GetTRN()
        {
            return _service.Retrieve(Contact.EntityLogicalName, _fixtureId, new ColumnSet(Contact.Fields.dfeta_TRN))
                .GetAttributeValue<string>(Contact.Fields.dfeta_TRN);
        }

        private ServiceClient GetCrmServiceClient() =>
            new(
                new Uri(_configuration["CrmUrl"]),
                _configuration["CrmClientId"],
                _configuration["CrmClientSecret"],
                useUniqueInstance: true);

        private Guid GetFixtureId(string nationalInsuranceNumber)
        {
            var fixtureId = _service.Create(new Contact
            {
                dfeta_NINumber = nationalInsuranceNumber,
                BirthDate = MatchingBirthdate
            });

            // updated TRN Required, so that TRN is generated
            _service.Update(new Contact
            {
                Id = fixtureId,
                dfeta_trnrequired = true
            });

            return fixtureId;
        }

        public GetTeacherRequest GetRequest(bool matchingTRN, bool matchingBirthdate, bool? matchingNationalInsuranceNumber = null) => new()
        {
            TRN = matchingTRN ? _matchingTRN : _nonmatchingTRN,
            BirthDate = matchingBirthdate ? MatchingBirthdate : NonmatchingBirthdate,
            NationalInsuranceNumber = (matchingNationalInsuranceNumber.HasValue && matchingNationalInsuranceNumber.Value) ?
                    _matchingNationalInsuranceNumber : _nonmatchingNationalInsuranceNumber
        };

        public void AssertMatchesFixtureId(Guid teacherId)
        {
            Assert.Equal(_fixtureId, teacherId);
        }

        public void Dispose()
        {
            // remove National Insurance Number so it can be re-used
            _service.Update(new Contact { Id = _fixtureId, dfeta_NINumber = string.Empty });

            _service.Dispose();
        }
    }
}
