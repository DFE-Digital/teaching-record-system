using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateContactHandler : ICrmQueryHandler<UpdateContactQuery, bool>
{
    public async Task<bool> ExecuteAsync(UpdateContactQuery query, IOrganizationServiceAsync organizationService)
    {
        var contact = new Contact() { Id = query.ContactId };

        void SetAttributeIfSpecified<T>(Option<T> value, string attributeName, Func<T, object>? mapValue = null)
        {
            if (value.HasValue)
            {
                object? attributeValue = value.ValueOrFailure();
                if (mapValue is not null)
                {
                    attributeValue = mapValue((T)attributeValue!);
                }

                contact.Attributes.Add(attributeName, attributeValue);
            }
        }

        SetAttributeIfSpecified(query.FirstName, Contact.Fields.FirstName);
        SetAttributeIfSpecified(query.MiddleName, Contact.Fields.MiddleName);
        SetAttributeIfSpecified(query.LastName, Contact.Fields.LastName);
        SetAttributeIfSpecified(query.StatedFirstName, Contact.Fields.dfeta_StatedFirstName);
        SetAttributeIfSpecified(query.StatedMiddleName, Contact.Fields.dfeta_StatedMiddleName);
        SetAttributeIfSpecified(query.StatedLastName, Contact.Fields.dfeta_StatedLastName);
        SetAttributeIfSpecified(query.DateOfBirth, Contact.Fields.BirthDate, dateOnly => dateOnly.ToDateTimeWithDqtBstFix(isLocalTime: false));
        SetAttributeIfSpecified(query.EmailAddress, Contact.Fields.EMailAddress1);
        SetAttributeIfSpecified(query.NationalInsuranceNumber, Contact.Fields.dfeta_NINumber);

        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = contact
        });

        return true;
    }
}
