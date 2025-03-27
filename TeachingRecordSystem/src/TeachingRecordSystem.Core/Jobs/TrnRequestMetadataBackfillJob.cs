
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class TrnRequestMetadataBackfillJob(IDbContextFactory<TrsDbContext> dbContextFactory, ICrmServiceClientProvider crmServiceClientProvider, IConfiguration config,
    ILogger<TrsDataSyncHelper> logger,
    IOrganizationServiceAsync2 organizationService)
{
    private const int BATCH_SIZE = 25;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var writeDbContext = await dbContextFactory.CreateDbContextAsync();
        var serviceClient = crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);

        //teacherid & trnrequestid dictionary
        var teacherIds = new Dictionary<Guid, string>();

        //fetch trn requests from trs
        var trnRequests = writeDbContext.TrnRequests.ToArray();
        var teacherDictionary = trnRequests
            .GroupBy(x => x.TeacherId)
            .ToDictionary(
                g => g.Key,
                g => (g.First().TrnToken)
            );

        foreach (var kvp in teacherDictionary)
        {
            if (!teacherIds.ContainsKey(kvp.Key))
            {
                teacherIds.Add(kvp.Key, kvp.Value!);
            }
        }

        var contactsQuery = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
            Contact.Fields.CreatedOn,
            Contact.Fields.CreatedBy,
            Contact.Fields.dfeta_TrnRequestID
            ),
            PageInfo = new()
            {
                PageNumber = 1,
                Count = 500
            }
        };
        contactsQuery.Criteria.AddCondition(Contact.Fields.dfeta_TrnRequestID, ConditionOperator.NotNull);
        EntityCollection result;

        //fetch contacts from crm that have a trnrequestid
        do
        {
            result = await serviceClient.RetrieveMultipleAsync(contactsQuery);
            foreach (var record in result.Entities)
            {
                var createdByUserId = record.GetAttributeValue<EntityReference>(Contact.Fields.CreatedBy).Id;
                var trnRequestId = record.GetAttributeValue<string>(Contact.Fields.dfeta_TrnRequestID);
                string request = TrnRequestHelper.GetCrmTrnRequestId(createdByUserId, trnRequestId);
                teacherIds.Add(record.Id, trnRequestId);
            }
            contactsQuery.PageInfo.PageNumber++;
            contactsQuery.PageInfo.PagingCookie = result.PagingCookie;
        } while (result.MoreRecords);

        var connstring = config.GetPostgresConnectionString();
        using var conn = new NpgsqlConnection(connstring);
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        await CreateTempTableAsync(conn, transaction);
        try
        {
            foreach (var chunk in teacherIds.Chunk(BATCH_SIZE))
            {
                var recordsToInsert = new List<TrnRequestMetadata>();

                foreach (var contact in chunk)
                {
                    var potentialDuplicate = false;

                    //fetch contact
                    var contactColumnSet = new ColumnSet(
                        Contact.Fields.Address1_Line1,
                        Contact.Fields.Address1_Line2,
                        Contact.Fields.Address1_Line3,
                        Contact.Fields.Address1_City,
                        Contact.Fields.Address1_Country,
                        Contact.Fields.Address1_PostalCode
                    );
                    var crmContact = serviceClient.Retrieve(Contact.EntityLogicalName, contact.Key, contactColumnSet);


                    //fetch any tasks for contact that were created if flagged as potential duplicate
                    var contactDuplicateTaskQuery = new QueryExpression(CrmTask.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(true),
                        PageInfo = new()
                        {
                            PageNumber = 1,
                            Count = 500
                        }
                    };
                    contactDuplicateTaskQuery.Criteria.AddCondition(CrmTask.Fields.Subject, ConditionOperator.Equal, "Notification for QTS Unit Team");
                    contactDuplicateTaskQuery.Criteria.AddCondition(CrmTask.Fields.RegardingObjectId, ConditionOperator.Equal, contact.Key);
                    contactDuplicateTaskQuery.Criteria.AddCondition(CrmTask.Fields.dfeta_potentialduplicateid, ConditionOperator.NotNull);
                    var duplicateTasks = await serviceClient.RetrieveMultipleAsync(contactDuplicateTaskQuery);
                    if (duplicateTasks.Entities.Any())
                    {
                        potentialDuplicate = true;
                    }


                    // fetch audits
                    IEnumerable<Guid> ids = new[] { contact.Key };
                    var audit = await GetAuditRecordsAsync(Contact.EntityLogicalName, ids, cancellationToken);

                    var auditDetails = audit[contact.Key];

                    var createAuditDetails = auditDetails.AuditDetails
                        .OfType<AttributeAuditDetail>()
                        .Where(a => a.AuditRecord.ToEntity<Audit>().Action == Audit_Action.Create);

                    var attributeNames = new List<string>();
                    if (createAuditDetails.Count() > 0)
                    {
                        foreach (var detail in createAuditDetails)
                        {
                            // Assuming the audit detail holds the new values under the "NewValue" key.
                            if (detail.NewValue is not null)
                            {
                                string? firstName = null;
                                string? middleName = null;
                                string? lastName = null;
                                DateTime? dob = null;
                                int? gender = null;
                                string? email = null;
                                var addressline1 = default(string?);
                                var addressline2 = default(string?);
                                var addressline3 = default(string?);
                                var city = default(string?);
                                var country = default(string?);
                                var nino = default(string?);
                                var postcode = default(string?);
                                Guid userId = detail.AuditRecord.GetAttributeValue<EntityReference>(Audit.Fields.UserId).Id;
                                DateTime createdOn = detail.AuditRecord.GetAttributeValue<DateTime>(Audit.Fields.CreatedOn);

                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.FirstName, out string firstNameOut))
                                    firstName = firstNameOut;
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.MiddleName, out string middleNameOut))
                                    middleName = middleNameOut;
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.LastName, out string lastNameOut))
                                    lastName = lastNameOut;
                                if (detail.NewValue.TryGetAttributeValue<DateTime?>(Contact.Fields.BirthDate, out DateTime? dobOut))
                                    dob = dobOut;
                                if (detail.NewValue.TryGetAttributeValue<OptionSetValue?>(Contact.Fields.GenderCode, out OptionSetValue? genderOut))
                                    gender = genderOut?.Value;
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.EMailAddress1, out string emailOut))
                                    email = emailOut;
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.Address1_Line1, out string addressline1Out))
                                    addressline1 = addressline1Out;
                                else
                                {
                                    addressline1 = crmContact.GetAttributeValue<string>(Contact.Fields.Address1_Line1);
                                }
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.Address1_Line2, out string addressline2Out))
                                    addressline2 = addressline2Out;
                                else
                                {
                                    addressline2 = crmContact.GetAttributeValue<string>(Contact.Fields.Address1_Line2);
                                }
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.Address1_Line3, out string addressline3Out))
                                    addressline3 = addressline3Out;
                                else
                                {
                                    addressline3 = crmContact.GetAttributeValue<string>(Contact.Fields.Address1_Line3);
                                }
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.Address1_City, out string cityOut))
                                    city = cityOut;
                                else
                                {
                                    city = crmContact.GetAttributeValue<string>(Contact.Fields.Address1_City);
                                }
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.Address1_Country, out string countryOut))
                                    country = countryOut;
                                else
                                {
                                    country = crmContact.GetAttributeValue<string>(Contact.Fields.Address1_Country);
                                }
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.Address1_PostalCode, out string postcodeOut))
                                    postcode = postcodeOut;
                                else
                                {
                                    addressline1 = crmContact.GetAttributeValue<string>(Contact.Fields.Address1_PostalCode);
                                }
                                if (detail.NewValue.TryGetAttributeValue<string>(Contact.Fields.dfeta_NINumber, out string ninoOut))
                                    nino = ninoOut;


                                //applicationUser & contact.dfeta_TrnRequestID
                                var requestId = contact.Value;

                                recordsToInsert.Add(new TrnRequestMetadata
                                {
                                    ApplicationUserId = userId,
                                    RequestId = requestId,
                                    CreatedOn = createdOn,
                                    DateOfBirth = dob.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                                    EmailAddress = email ?? string.Empty,
                                    IdentityVerified = false,
                                    Name = new string[] { firstName, middleName, lastName }.Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                                    Gender = gender,
                                    Postcode = postcode,
                                    PotentialDuplicate = potentialDuplicate,
                                    OneLoginUserSubject = null,
                                    NationalInsuranceNumber = nino,
                                    AddressLine1 = addressline1,
                                    AddressLine2 = addressline2,
                                    AddressLine3 = addressline3,
                                    City = city,
                                    Country = country,
                                });

                            }
                        }
                    }
                }

                if (recordsToInsert.Any())
                {
                    //batch insert temp table
                    await BulkInsertAsync(conn, transaction, recordsToInsert);

                    //upsert into trn_request_metadata
                    await UpsertAsync(conn, transaction);

                    //clear temp table for next iteration
                    await TruncateTempImportTrnRequestMetadataAsync(conn, transaction);
                }
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        await transaction.CommitAsync();

    }

    private async Task CreateTempTableAsync(NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        using (var command = new NpgsqlCommand(@"
            CREATE TEMP TABLE temp_import_trn_request_metadata (
                application_user_id uuid NOT NULL,
                request_id character varying(255) NOT NULL,
                created_on timestamp with time zone NOT NULL,
                date_of_birth date NOT NULL,
                email_address text,
                identity_verified boolean,
                name text[] NOT NULL DEFAULT ARRAY[]::text[],
                gender integer,
                postcode text,
                potential_duplicate boolean,
                address_line1 text,
                address_line2 text,
                address_line3 text,
                city text,
                country text,
                national_insurance_number text
            ) ON COMMIT DROP;
        ", conn, transaction))
        {
            await command.ExecuteNonQueryAsync();
        }
    }



    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecordsAsync(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        // Throttle the amount of concurrent requests
        using var requestThrottle = new SemaphoreSlim(20, 20);

        // Keep track of the last seen 'retry-after' value
        var retryDelayUpdateLock = new object();
        var retryDelay = Task.Delay(0, cancellationToken);

        void UpdateRetryDelay(TimeSpan ts)
        {
            lock (retryDelayUpdateLock)
            {
                retryDelay = Task.Delay(ts, cancellationToken);
            }
        }

        return (await Task.WhenAll(ids
            .Chunk(10)
            .Select(async chunk =>
            {
                var request = new ExecuteMultipleRequest()
                {
                    Requests = new(),
                    Settings = new()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    }
                };

                // The following is not supported by FakeXrmEasy hence the check above to allow more test coverage
                request.Requests.AddRange(chunk.Select(e => new RetrieveRecordChangeHistoryRequest() { Target = e.ToEntityReference(entityLogicalName) }));

                ExecuteMultipleResponse response;
                while (true)
                {
                    await retryDelay;
                    await requestThrottle.WaitAsync(cancellationToken);
                    try
                    {
                        response = (ExecuteMultipleResponse)await organizationService.ExecuteAsync(request, cancellationToken);
                    }
                    catch (FaultException fex) when (fex.IsCrmRateLimitException(out var retryAfter))
                    {
                        logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; Fault exception. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                        UpdateRetryDelay(retryAfter);
                        continue;
                    }
                    finally
                    {
                        requestThrottle.Release();
                    }

                    if (response.IsFaulted)
                    {
                        var firstFault = response.Responses.First(r => r.Fault is not null).Fault;

                        if (firstFault.IsCrmRateLimitFault(out var retryAfter))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; CRM rate limit fault. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                            UpdateRetryDelay(retryAfter);
                            continue;
                        }
                        else if (firstFault.Message.Contains("The HTTP status code of the response was not expected (429)"))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; 429 too many requests", entityLogicalName);
                            UpdateRetryDelay(TimeSpan.FromMinutes(2));
                            continue;
                        }

                        throw new FaultException<OrganizationServiceFault>(firstFault, new FaultReason(firstFault.Message));
                    }

                    break;
                }

                return response.Responses.Zip(
                    chunk,
                    (r, e) => (Id: e, ((RetrieveRecordChangeHistoryResponse)r.Response).AuditDetailCollection));
            })))
            .SelectMany(b => b)
            .ToDictionary(t => t.Id, t => t.AuditDetailCollection);
    }


    public async Task TruncateTempImportTrnRequestMetadataAsync(NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        var sql = "TRUNCATE TABLE temp_import_trn_request_metadata;";
        using (var command = new NpgsqlCommand(sql, conn, transaction))
        {
            await command.ExecuteNonQueryAsync();
        }
    }

    // Insert if row with requestid & application user id does not exist
    // Update row from audit history, but do not null out the fields if a field is null
    // from the audit history.
    private async Task UpsertAsync(NpgsqlConnection conn, NpgsqlTransaction transaction)
    {
        using (var command = new NpgsqlCommand(@"
            INSERT INTO trn_request_metadata (
                application_user_id,
                request_id,
                created_on,
                date_of_birth,
                email_address,
                identity_verified,
                name,
                gender,
                postcode,
                potential_duplicate,
                address_line1,
                address_line2,
                address_line3,
                city,
                country,
                national_insurance_number
            )
            SELECT 
                application_user_id,
                request_id,
                created_on,
                date_of_birth,
                email_address,
                identity_verified,
                name,
                gender,
                postcode,
                potential_duplicate,
                address_line1,
                address_line2,
                address_line3,
                city,
                country,
                national_insurance_number
            FROM temp_import_trn_request_metadata
            ON CONFLICT (application_user_id, request_id)
            DO UPDATE SET 
                created_on = COALESCE(EXCLUDED.created_on, trn_request_metadata.created_on),
                date_of_birth = COALESCE(EXCLUDED.date_of_birth, trn_request_metadata.date_of_birth),
                email_address = COALESCE(EXCLUDED.email_address, trn_request_metadata.email_address),
                identity_verified = COALESCE(EXCLUDED.identity_verified, trn_request_metadata.identity_verified),
                name = COALESCE(EXCLUDED.name, trn_request_metadata.name),
                gender = COALESCE(EXCLUDED.gender, trn_request_metadata.gender),
                postcode = COALESCE(EXCLUDED.postcode, trn_request_metadata.postcode),
                potential_duplicate = COALESCE(EXCLUDED.potential_duplicate, trn_request_metadata.potential_duplicate),
                address_line1 = COALESCE(EXCLUDED.address_line1, trn_request_metadata.address_line1),
                address_line2 = COALESCE(EXCLUDED.address_line2, trn_request_metadata.address_line2),
                address_line3 = COALESCE(EXCLUDED.address_line3, trn_request_metadata.address_line3),
                city = COALESCE(EXCLUDED.city, trn_request_metadata.city),
                country = COALESCE(EXCLUDED.country, trn_request_metadata.country),
                national_insurance_number = COALESCE(EXCLUDED.national_insurance_number, trn_request_metadata.national_insurance_number);
        ", conn, transaction))
        {
            await command.ExecuteNonQueryAsync();
        }
    }


    /// <summary>
    /// inserts into temporary table (in batches)
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="transaction"></param>
    /// <param name="records"></param>
    /// <returns></returns>
    private async Task BulkInsertAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, List<TrnRequestMetadata> records)
    {
        using var writer = await conn.BeginBinaryImportAsync(@"
            COPY temp_import_trn_request_metadata (
                application_user_id,
                request_id,
                created_on,
                date_of_birth, 
                email_address,
                identity_verified,
                name,
                gender,
                postcode,
                potential_duplicate,
                address_line1,
                address_line2,
                address_line3,
                city,
                country,
                national_insurance_number
            ) FROM STDIN (FORMAT BINARY)");

        foreach (var record in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(record.ApplicationUserId, NpgsqlTypes.NpgsqlDbType.Uuid);
            await writer.WriteAsync(record.RequestId, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.CreatedOn, NpgsqlTypes.NpgsqlDbType.TimestampTz);
            await writer.WriteAsync(record.DateOfBirth.ToDateTime(TimeOnly.MinValue), NpgsqlTypes.NpgsqlDbType.Date);
            await writer.WriteAsync(record.EmailAddress, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.IdentityVerified, NpgsqlTypes.NpgsqlDbType.Boolean);
            await writer.WriteAsync(record.Name ?? Array.Empty<string>(), NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.Gender ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(record.Postcode, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.PotentialDuplicate, NpgsqlTypes.NpgsqlDbType.Boolean);
            await writer.WriteAsync(record.AddressLine1, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.AddressLine2, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.AddressLine3, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.City, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.Country, NpgsqlTypes.NpgsqlDbType.Text);
            await writer.WriteAsync(record.NationalInsuranceNumber, NpgsqlTypes.NpgsqlDbType.Text);  // national_insurance_number
        }

        await writer.CompleteAsync();
    }

    public class TrnRequestBackfillItem
    {
        public Guid ContactId { get; set; }
        public string? addressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? city { get; set; }
    }
}
