using System.Globalization;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class FindPotentialDuplicateContactsHandler : ICrmQueryHandler<FindPotentialDuplicateContactsQuery, FindPotentialDuplicateContactsResult[]>
{
    public async Task<FindPotentialDuplicateContactsResult[]> Execute(FindPotentialDuplicateContactsQuery findQuery, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

        if (TryGetMatchCombinationsFilter(out var matchCombinationsFilter))
        {
            filter.AddFilter(matchCombinationsFilter);
        }
        else
        {
            // Not enough data in the input to match on
            return Array.Empty<FindPotentialDuplicateContactsResult>();
        }

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new()
            {
                Columns =
                {
                    Contact.Fields.dfeta_ActiveSanctions,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_EYTSDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_HUSID,
                    Contact.Fields.dfeta_SlugId
                }
            },
            Criteria = filter
        };

        var queryResult = await organizationService.RetrieveMultipleAsync(query);

        var results = queryResult.Entities.Select(entity => entity.ToEntity<Contact>())
            .Select(match =>
            {
                var attributeMatches = new[]
                {
                    (
                        Attribute: Contact.Fields.FirstName,
                        Matches: NamesAreEqual(findQuery.FirstName, match.FirstName)
                    ),
                    (
                        Attribute: Contact.Fields.MiddleName,
                        Matches: NamesAreEqual(findQuery.MiddleName, match.MiddleName ?? "")
                    ),
                    (
                        Attribute: Contact.Fields.LastName,
                        Matches: NamesAreEqual(findQuery.LastName, match.LastName)
                    ),
                    (
                        Attribute: Contact.Fields.BirthDate,
                        Matches: findQuery.DateOfBirth.ToDateTime().Equals(match.BirthDate)
                    )
                };

                var matchedAttributeNames = attributeMatches.Where(m => m.Matches).Select(m => m.Attribute).ToArray();

                return new FindPotentialDuplicateContactsResult()
                {
                    TeacherId = match.Id,
                    MatchedAttributes = matchedAttributeNames,
                    HasActiveSanctions = match.dfeta_ActiveSanctions == true,
                    HasQtsDate = match.dfeta_QTSDate.HasValue,
                    HasEytsDate = match.dfeta_EYTSDate.HasValue,
                };
            })
            .ToArray();

        return results;

        bool TryGetMatchCombinationsFilter(out FilterExpression? filter)
        {
            // Find an existing active record that matches on at least 3 of FirstName, MiddleName, LastName & BirthDate

            var fields = new[]
            {
                (FieldName: Contact.Fields.FirstName, Value: findQuery.FirstName),
                (FieldName: Contact.Fields.MiddleName, Value: findQuery.MiddleName),
                (FieldName: Contact.Fields.LastName, Value: findQuery.LastName),
                (FieldName: Contact.Fields.BirthDate, Value: (object)findQuery.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false))
            }.ToList();

            // If fields are null in the input then don't try to match them (typically MiddleName)
            fields.RemoveAll(f => f.Value == null || (f.Value is string stringValue && string.IsNullOrEmpty(stringValue)));

            var combinations = fields.GetCombinations(length: 3).ToArray();

            if (combinations.Length == 0)
            {
                filter = default;
                return false;
            }

            var combinationsFilter = new FilterExpression(LogicalOperator.Or);

            foreach (var combination in combinations)
            {
                var innerFilter = new FilterExpression(LogicalOperator.And);

                foreach (var (fieldName, value) in combination)
                {
                    innerFilter.AddCondition(fieldName, ConditionOperator.Equal, value);
                }

                combinationsFilter.AddFilter(innerFilter);
            }

            filter = combinationsFilter;
            return true;
        }

        static bool NamesAreEqual(string a, string b) =>
            string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }
}
