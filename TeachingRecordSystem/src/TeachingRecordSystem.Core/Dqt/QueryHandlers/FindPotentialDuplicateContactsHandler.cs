using System.Globalization;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class FindPotentialDuplicateContactsHandler : ICrmQueryHandler<FindPotentialDuplicateContactsQuery, FindPotentialDuplicateContactsResult[]>
{
    public async Task<FindPotentialDuplicateContactsResult[]> Execute(FindPotentialDuplicateContactsQuery findQuery, IOrganizationServiceAsync organizationService)
    {
        // Find an existing active record with a TRN that matches on:
        // * at least 3 of FirstName, MiddleName, LastName and BirthDate *OR*
        // * email address *OR*
        // * NINO.

        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
        filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull);

        var childFilters = new FilterExpression(LogicalOperator.Or);

        if (TryGetAtLeastThreeMatchesFilter(out var matchCombinationsFilter))
        {
            childFilters.AddFilter(matchCombinationsFilter);
        }

        var emails = findQuery.EmailAddresses.ToArray();
        if (emails.Length > 0)
        {
            childFilters.AddCondition(Contact.Fields.EMailAddress1, ConditionOperator.In, emails);
        }

        if (!string.IsNullOrEmpty(findQuery.NationalInsuranceNumber))
        {
            childFilters.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Equal, findQuery.NationalInsuranceNumber);
        }

        if (findQuery.MatchedOnNationalInsuranceNumberContactIds.Length > 0)
        {
            childFilters.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.In, findQuery.MatchedOnNationalInsuranceNumberContactIds.Cast<object>().ToArray());
        }

        if (childFilters.Filters.Count == 0)
        {
            // Not enough data in the input to match on
            return [];
        }

        filter.AddFilter(childFilters);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new()
            {
                Columns =
                {
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_EYTSDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_PreviousLastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_HUSID,
                    Contact.Fields.dfeta_SlugId,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.dfeta_TRN
                }
            },
            Criteria = filter
        };

        var queryResult = await organizationService.RetrieveMultipleAsync(query);

        var results = queryResult.Entities
            .Select(entity => entity.ToEntity<Contact>())
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
                    ),
                    (
                        Attribute: Contact.Fields.dfeta_NINumber,
                        Matches: (!string.IsNullOrEmpty(findQuery.NationalInsuranceNumber) && findQuery.NationalInsuranceNumber == match.dfeta_NINumber) ||
                            findQuery.MatchedOnNationalInsuranceNumberContactIds.Contains(match.Id)
                    )
                };

                var matchedAttributeNames = attributeMatches.Where(m => m.Matches).Select(m => m.Attribute).ToArray();

                return new FindPotentialDuplicateContactsResult()
                {
                    ContactId = match.Id,
                    Trn = match.dfeta_TRN,
                    MatchedAttributes = matchedAttributeNames,
                    HasQtsDate = match.dfeta_QTSDate.HasValue,
                    HasEytsDate = match.dfeta_EYTSDate.HasValue,
                    FirstName = match.FirstName,
                    MiddleName = match.MiddleName ?? "",
                    LastName = match.LastName,
                    StatedFirstName = match.dfeta_StatedFirstName,
                    StatedMiddleName = match.dfeta_StatedMiddleName,
                    StatedLastName = match.dfeta_StatedLastName,
                    DateOfBirth = match.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                    NationalInsuranceNumber = !string.IsNullOrEmpty(match.dfeta_NINumber) ? match.dfeta_NINumber : null,
                    EmailAddress = match.EMailAddress1
                };
            })
            .ToArray();

        return results;

        bool TryGetAtLeastThreeMatchesFilter(out FilterExpression? filter)
        {
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
