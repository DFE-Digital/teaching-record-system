using System.Globalization;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class FindPotentialDuplicateContactsHandler : ICrmQueryHandler<FindPotentialDuplicateContactsQuery, FindPotentialDuplicateContactsResult[]>
{
    public async Task<FindPotentialDuplicateContactsResult[]> Execute(FindPotentialDuplicateContactsQuery findQuery, IOrganizationServiceAsync organizationService)
    {
        var emails = findQuery.EmailAddresses.ToArray();

        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

        var childFilters = new FilterExpression(LogicalOperator.Or);

        if (TryGetMatchCombinationsFilter(out var matchCombinationsFilter))
        {
            childFilters.AddFilter(matchCombinationsFilter);
        }

        if (emails.Length > 0)
        {
            childFilters.AddCondition(Contact.Fields.EMailAddress1, ConditionOperator.In, emails);
        }

        if (childFilters.Filters.Count == 0)
        {
            // Not enough data in the input to match on
            return Array.Empty<FindPotentialDuplicateContactsResult>();
        }

        filter.AddFilter(childFilters);

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
                    Contact.Fields.dfeta_PreviousLastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_HUSID,
                    Contact.Fields.dfeta_SlugId,
                    Contact.Fields.EMailAddress1,
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
                        Matches: NamesAreMatched(findQuery.FirstNames, match.FirstName)
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
                        Attribute: Contact.Fields.dfeta_PreviousLastName,
                        Matches: NamesAreEqual(findQuery.LastName, match.dfeta_PreviousLastName)
                    ),
                    (
                        Attribute: Contact.Fields.BirthDate,
                        Matches: findQuery.DateOfBirth.ToDateTime().Equals(match.BirthDate)
                    ),
                    (
                        Attribute: Contact.Fields.EMailAddress1,
                        Matches: findQuery.EmailAddresses.Contains(match.EMailAddress1, StringComparer.OrdinalIgnoreCase)
                    )
                };

                var matchedAttributeNames = attributeMatches.Where(m => m.Matches).Select(m => m.Attribute).ToArray();

                return new FindPotentialDuplicateContactsResult()
                {
                    ContactId = match.Id,
                    MatchedAttributes = matchedAttributeNames,
                    HasActiveSanctions = match.dfeta_ActiveSanctions == true,
                    HasQtsDate = match.dfeta_QTSDate.HasValue,
                    HasEytsDate = match.dfeta_EYTSDate.HasValue,
                    FirstName = match.FirstName,
                    MiddleName = match.MiddleName ?? "",
                    LastName = match.LastName,
                    DateOfBirth = match.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                    EmailAddress = match.EMailAddress1
                };
            })
            .ToArray();

        return results;

        bool TryGetMatchCombinationsFilter(out FilterExpression? filter)
        {
            // Find an existing active record that matches on at least 3 of FirstName, MiddleName, LastName & BirthDate

            var fields = new[]
            {
                (FieldName: Contact.Fields.FirstName, Value: findQuery.FirstNames),
                (FieldName: Contact.Fields.MiddleName, Value: findQuery.MiddleName),
                (FieldName: Contact.Fields.LastName, Value: findQuery.LastName),
                (FieldName: Contact.Fields.dfeta_PreviousLastName, Value: findQuery.LastName),
                (FieldName: Contact.Fields.BirthDate, Value: (object)findQuery.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false)),
            }.ToList();

            // Don't try to match with any fields that are empty
            fields.RemoveAll(f =>
                f.Value == null ||
                (f.Value is string stringValue && string.IsNullOrEmpty(stringValue)) ||
                (f.Value is IEnumerable<string> stringEnumerable && !stringEnumerable.Any()));

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
                    if (value is IEnumerable<string> collectionValue)
                    {
                        innerFilter.AddCondition(fieldName, ConditionOperator.In, collectionValue.ToArray());
                    }
                    else
                    {
                        innerFilter.AddCondition(fieldName, ConditionOperator.Equal, value);
                    }
                }

                combinationsFilter.AddFilter(innerFilter);
            }

            filter = combinationsFilter;
            return true;
        }

        static bool NamesAreEqual(string a, string b) =>
            string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;

        static bool NamesAreMatched(IEnumerable<string> inputs, string matched) =>
            inputs.Any(i => NamesAreEqual(i, matched));
    }
}
