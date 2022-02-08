using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Xrm.Sdk.Query;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.DataStore.Crm.Models
{
    public class GetTeacherRequest
    {
        [FromRoute(Name = "trn")]
        public string TRN { get; set; }

        [FromQuery(Name = "birthdate"), SwaggerParameter(Required = true), SwaggerSchema(Format = "date"), ModelBinder(typeof(ModelBinding.DateModelBinder))]
        public DateTime? BirthDate { get; set; }

        [FromQuery(Name = "nino")]
        public string NationalInsuranceNumber { get; set; }

        public QueryExpression GenerateQuery()
        {
            var filter = GenerateFilter();

            return new(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    Contact.Fields.FullName,
                    Contact.Fields.StateCode,
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_ActiveSanctions
                ),
                Criteria = filter
            };
        }

        private FilterExpression GenerateFilter()
        {
            var filter = new FilterExpression(LogicalOperator.And);

            if (string.IsNullOrEmpty(NationalInsuranceNumber))
            {
                filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, TRN);
            }
            else
            {
                var childFilter = new FilterExpression(LogicalOperator.Or);

                childFilter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, TRN);
                childFilter.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Equal, NationalInsuranceNumber);

                filter.AddFilter(childFilter);
            }

            filter.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, BirthDate);

            return filter;
        }

        public Contact SelectMatch(IEnumerable<Contact> matches)
        {
            return
                matches.FirstOrDefault(match => match.dfeta_TRN == TRN) ??
                matches.FirstOrDefault(match => match.dfeta_NINumber == NationalInsuranceNumber);
        }
    }
}
