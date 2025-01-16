using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillTrnRequestMetadataJob(
    TrsDbContext dbContext,
    IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper syncHelper,
    IAuditRepository auditRepository)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var serviceContext = new DqtCrmServiceContext(organizationService);

        var columns = new ColumnSet(
            Contact.Fields.dfeta_TrnRequestID,
            Contact.Fields.CreatedOn,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.dfeta_StatedFirstName,
            Contact.Fields.dfeta_StatedMiddleName,
            Contact.Fields.dfeta_StatedLastName,
            Contact.Fields.BirthDate,
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.GenderCode,
            Contact.Fields.Address1_Line1,
            Contact.Fields.Address1_Line2,
            Contact.Fields.Address1_Line3,
            Contact.Fields.Address1_City,
            Contact.Fields.Address1_PostalCode,
            Contact.Fields.Address1_Country,
            Contact.Fields.EMailAddress1
        );

        const int pageSize = 1000;

        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.dfeta_TrnRequestID, ConditionOperator.NotNull);
        filter.AddCondition(Contact.Fields.dfeta_TrnRequestID, ConditionOperator.NotEqual, "");

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = columns,
            Criteria = filter,
            Orders =
            {
                new OrderExpression(Contact.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            }
        };

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EntityCollection result;
            try
            {
                result = await organizationService.RetrieveMultipleAsync(query);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            foreach (var contact in result.Entities.Select(e => e.ToEntity<Contact>()))
            {
                if (string.IsNullOrEmpty(contact.dfeta_TrnRequestID))
                {
                    continue;
                }

                try
                {
                    var originalContact = await GetOriginalContactVersionAsync(contact);

                    var applicationUserId = Guid.Parse(contact.dfeta_TrnRequestID[..36]);
                    var requestId = contact.dfeta_TrnRequestID[38..];

                    var md = await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(md =>
                        md.ApplicationUserId == applicationUserId && md.RequestId == requestId);

                    if (md is null)
                    {
                        string[] name =
                            (!string.IsNullOrEmpty(originalContact.dfeta_StatedFirstName) &&
                             !string.IsNullOrEmpty(originalContact.dfeta_StatedLastName)
                                ?
                                new[]
                                {
                                    originalContact.dfeta_StatedFirstName, originalContact.dfeta_StatedMiddleName,
                                    originalContact.dfeta_StatedLastName
                                }
                                : [originalContact.FirstName, originalContact.MiddleName, originalContact.LastName])
                            .Where(n => !string.IsNullOrEmpty(n)).ToArray();

                        md = new TrnRequestMetadata
                        {
                            ApplicationUserId = applicationUserId,
                            RequestId = requestId,
                            CreatedOn = contact.CreatedOn!.Value,
                            IdentityVerified = null,
                            EmailAddress = originalContact.EMailAddress1,
                            OneLoginUserSubject = null,
                            Name = name,
                            DateOfBirth = originalContact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false)
                        };
                        dbContext.Add(md);
                    }

                    var potentialDuplicate = serviceContext.TaskSet
                        .Where(t => t.RegardingObjectId.Id == contact.Id && t.Description.StartsWith("Potential duplicate"))
                        .ToArray()
                        .Count() > 0;

                    md.PotentialDuplicate = potentialDuplicate;
                    md.NationalInsuranceNumber = originalContact.dfeta_NINumber;
                    md.Gender =
                        originalContact.GenderCode?.TryConvertToEnumByValue<Contact_GenderCode, Gender>(out var gender) == true
                            ? (int?)gender
                            : null;
                    md.AddressLine1 = originalContact.Address1_Line1;
                    md.AddressLine2 = originalContact.Address1_Line2;
                    md.AddressLine3 = originalContact.Address1_Line3;
                    md.City = originalContact.Address1_City;
                    md.Postcode = originalContact.Address1_PostalCode;
                    md.Country = originalContact.Address1_Country;

                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed for contact ID '{contact.Id}'.", e);
                }
            }

            if (result.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;
            }
            else
            {
                break;
            }
        }

        async Task<Contact> GetOriginalContactVersionAsync(Contact contact)
        {
            var audit = await GetAuditAsync();

            if (audit is null)
            {
                await syncHelper.SyncAuditAsync(Contact.EntityLogicalName, [contact.Id], skipIfExists: true);
                audit = (await GetAuditAsync())!;
            }

            return GetInitialEntityVersion(contact, audit.AuditDetails, columns.Columns.ToArray());

            Task<AuditDetailCollection?> GetAuditAsync() =>
                auditRepository.GetAuditDetailAsync(
                    Contact.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    contact.Id);
        }
    }

    private TEntity GetInitialEntityVersion<TEntity>(TEntity latest, IEnumerable<AuditDetail> auditDetails, string[] attributeNames)
        where TEntity : Entity
    {
        if (!latest.TryGetAttributeValue<DateTime?>("createdon", out var createdOn) || !createdOn.HasValue)
        {
            throw new ArgumentException($"Expected {latest.LogicalName} entity with ID {latest.Id} to have a non-null 'createdon' attribute value.", nameof(latest));
        }

        var ordered = auditDetails
            .OfType<AttributeAuditDetail>()
            .Select(a => (AuditDetail: a, AuditRecord: a.AuditRecord.ToEntity<Audit>()))
            .OrderBy(a => a.AuditRecord.CreatedOn)
            .ThenBy(a => a.AuditRecord.Action == Audit_Action.Create ? 0 : 1)
            .ToArray();

        if (ordered.Length == 0)
        {
            return latest;
        }

        return GetInitialVersion();

        TEntity GetInitialVersion()
        {
            TEntity? initial;
            if (ordered[0] is { AuditRecord: { Action: Audit_Action.Create } } createAction)
            {
                initial = createAction.AuditDetail.NewValue.ToEntity<TEntity>();
                initial.Id = latest.Id;
            }
            else
            {
                // Starting with `latest`, go through each event in reverse and undo the changes it applied.
                // When we're done we end up with the initial version of the record.
                initial = latest.ShallowClone();

                foreach (var a in ordered.Reverse())
                {
                    // Check that new attributes align with what we have in `initial`;
                    // if they don't, then we've got an incomplete history
                    foreach (var attr in a.AuditDetail.NewValue.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
                    {
                        if (!AttributeValuesEqual(attr.Value, initial.Attributes.TryGetValue(attr.Key, out var initialAttr) ? initialAttr : null))
                        {
                            throw new Exception($"Non-contiguous audit records for {initial.LogicalName} '{initial.Id}':\n" +
                                $"Expected '{attr.Key}' to be '{attr.Value ?? "<null>"}' but was '{initialAttr ?? "<null>"}'.");
                        }

                        if (!a.AuditDetail.OldValue.Attributes.Contains(attr.Key))
                        {
                            initial.Attributes.Remove(attr.Key);
                        }
                    }

                    foreach (var attr in a.AuditDetail.OldValue.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
                    {
                        initial.Attributes[attr.Key] = attr.Value;
                    }
                }
            }

            return initial.SparseClone(attributeNames);
        }

        static bool AttributeValuesEqual(object? first, object? second)
        {
            if (first is null && second is null)
            {
                return true;
            }

            if (first is null || second is null)
            {
                return false;
            }

            if (first.GetType() != second.GetType())
            {
                return false;
            }

            return first is EntityReference firstRef && second is EntityReference secondRef ?
                firstRef.Name == secondRef.Name && firstRef.Id == secondRef.Id :
                first.Equals(second);
        }
    }
}

file static class Extensions
{
    public static TEntity ShallowClone<TEntity>(this TEntity entity) where TEntity : Entity
    {
        // N.B. This only clones Attributes

        var cloned = new Entity(entity.LogicalName, entity.Id);

        foreach (var attr in entity.Attributes)
        {
            cloned.Attributes.Add(attr.Key, attr.Value);
        }

        return cloned.ToEntity<TEntity>();
    }

    public static TEntity SparseClone<TEntity>(this TEntity entity, string[] attributeNames) where TEntity : Entity
    {
        // N.B. This only clones Attributes in the whitelist
        var cloned = new Entity(entity.LogicalName, entity.Id);

        foreach (var attr in entity.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
        {
            cloned.Attributes.Add(attr.Key, attr.Value);
        }

        return cloned.ToEntity<TEntity>();
    }

    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
