using Microsoft.PowerPlatform.Dataverse.Client;
using DqtApi.Models;
using Microsoft.Xrm.Sdk.Query;
using System.Threading.Tasks;

namespace DqtApi.DAL
{
    public class DataverseAdaptor : IDataverseAdaptor
    {
        private readonly IOrganizationServiceAsync _service;
        public DataverseAdaptor(IOrganizationServiceAsync service)
        {
            _service = service;
        }

        public async Task<Teacher> GetTeacherByTRN(string trn)
        {
            var query = new QueryExpression()
            {
                EntityName = "contact",
                ColumnSet = new ColumnSet(
                    "dfeta_trn",
                    "dfeta_ninumber",
                    "fullname",
                    "birthdate",
                    "dfeta_activesanctions",
                    "statecode"),
                Criteria = new FilterExpression()
                {
                    Conditions =
                    {
                        new ConditionExpression()
                        {
                            AttributeName = "dfeta_trn",
                            Operator = ConditionOperator.Equal,
                            Values = { trn }
                        }
                    }
                }
            };

            var results = await _service.RetrieveMultipleAsync(query);

            if (results.Entities.Count == 0)
            {
                return null;
            }

            if (results.Entities.Count > 1)
            {
                throw new MoreThanOneMatchingTeacherException();
            }

            return new Teacher(results.Entities[0]);
        }
    }
}