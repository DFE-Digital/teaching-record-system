
using Microsoft.Extensions.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class TrnRequestMetadataBackfillJob(IDbContextFactory<TrsDbContext> dbContextFactory, ICrmServiceClientProvider crmServiceClientProvider, IConfiguration config)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var writeDbContext = await dbContextFactory.CreateDbContextAsync();
        var serviceClient = crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);


        var teacherIds = new Dictionary<Guid, string>();

        //fetch trn requests from trs
        var trnRequests = writeDbContext.TrnRequests.ToArray();
        var teacherDictionary = trnRequests
            .GroupBy(x => x.TeacherId)
            .ToDictionary(
                g => g.Key,
                g => (g.First().TrnRequestId, g.First().TrnToken)
            );

        var contactsQuery = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Contact.Fields.CreatedOn,
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
                teacherIds.Add(record.Id, record.GetAttributeValue<string>(Contact.Fields.dfeta_TrnRequestID));
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
            var batchSize = 25;
            var totalBatches = (int)Math.Ceiling((double)teacherIds.Count() / batchSize);
            for (int batch = 0; batch < totalBatches; batch++)
            {
                var contactsToSync = teacherIds.Skip(batch * batchSize).Take(batchSize).ToList();
                var recordsToInsert = new List<TrnRequestMetadata>();

                foreach (var contact in contactsToSync)
                {
                    //fetch audit records
                    var query = new QueryExpression(Audit.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(true),
                        Orders =
                    {
                        new OrderExpression(Audit.Fields.CreatedOn, OrderType.Ascending)
                    },
                        PageInfo = new()
                        {
                            PageNumber = 1,
                            Count = 500
                        }
                    };
                    query.Criteria.AddCondition(Audit.Fields.ObjectId, ConditionOperator.Equal, contact.Key);
                    query.Criteria.AddCondition(Audit.Fields.ObjectTypeCode, ConditionOperator.Equal, "contact");
                    query.Criteria.AddCondition(Audit.Fields.Operation, ConditionOperator.Equal, 1); //create
                    var auditRecords = await serviceClient.RetrieveMultipleAsync(query);
                    var audit = auditRecords.Entities.FirstOrDefault();

                    //audit is stored in json
                    if (audit != null)
                    {
                        var json = audit["changedata"].ToString();
                        JObject outerData = JObject.Parse(json);

                        if (outerData.ContainsKey("changedAttributes"))
                        {
                            JToken changedAttributesToken = outerData["changedAttributes"];

                            if (changedAttributesToken is not null && changedAttributesToken.Type == JTokenType.Array)
                            {
                                JArray changedAttributesArray = (JArray)changedAttributesToken;

                                var firstNameObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "firstname", StringComparison.OrdinalIgnoreCase));
                                var lastNameObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "lastname", StringComparison.OrdinalIgnoreCase));
                                var dobObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "birthdate", StringComparison.OrdinalIgnoreCase));

                                var middleNameObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "middlename", StringComparison.OrdinalIgnoreCase));

                                var genderObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "gendercode", StringComparison.OrdinalIgnoreCase));

                                var addressline1Obj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "address_line1", StringComparison.OrdinalIgnoreCase));

                                var addressline2Obj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "address_line2", StringComparison.OrdinalIgnoreCase));

                                var addressline3Obj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "address_line3", StringComparison.OrdinalIgnoreCase));

                                var emailAddressObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "emailaddress1", StringComparison.OrdinalIgnoreCase));

                                var postalcodeObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "address1_postalcode", StringComparison.OrdinalIgnoreCase));

                                var fullNameObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "fullname", StringComparison.OrdinalIgnoreCase));

                                var ninoObj = changedAttributesArray
                                    .Children<JObject>()
                                    .FirstOrDefault(jObj => string.Equals(jObj.Value<string>("logicalName"), "dfeta_ninumber", StringComparison.OrdinalIgnoreCase));

                                var applicationUser = audit.GetAttributeValue<EntityReference>("userid").Id;
                                var createdOn = audit.GetAttributeValue<DateTime>("createdon");
                                var firstName = firstNameObj?.Value<string>("newValue");
                                var middleName = middleNameObj?.Value<string>("newValue");
                                var lastName = lastNameObj?.Value<string>("newValue");
                                var dob = dobObj?.Value<DateTime?>("newValue");
                                var gender = genderObj?.Value<int?>("newValue");
                                //var addressline1 = addressline1Obj.Value<string>("newValue");   //not in audit for creates
                                //var addressline2 = addressline2Obj.Value<string>("newValue");   //not in audit for creates
                                //var addressline3 = addressline3Obj.Value<string>("newValue");   //not in audit for creates
                                //city                                                            //not in audit for creates
                                //country                                                         //not in audit for creates
                                var email = emailAddressObj?.Value<string>("newValue");
                                var postcode = postalcodeObj?.Value<string>("newValue");
                                var fullname = fullNameObj?.Value<string>("newValue");
                                var nino = ninoObj?.Value<string>("newValue");

                                //applicationUser & trn requestid
                                var requestId = TrnRequestHelper.GetCrmTrnRequestId(applicationUser, contact.Value);

                                recordsToInsert.Add(new TrnRequestMetadata
                                {
                                    ApplicationUserId = audit.GetAttributeValue<EntityReference>("userid").Id,
                                    RequestId = requestId,
                                    CreatedOn = audit.GetAttributeValue<DateTime>("createdon"),
                                    DateOfBirth = dob.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                                    EmailAddress = email ?? string.Empty,
                                    IdentityVerified = false,
                                    Name = new string[] { firstName, middleName, lastName }.Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                                    Gender = gender,
                                    Postcode = postcode ?? string.Empty,
                                    PotentialDuplicate = false,
                                    OneLoginUserSubject = null,
                                    NationalInsuranceNumber = nino
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
                }
            }
        }
        catch (Exception e)
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
                    created_on = EXCLUDED.created_on,
                    date_of_birth = EXCLUDED.date_of_birth,
                    email_address = EXCLUDED.email_address,
                    identity_verified = EXCLUDED.identity_verified,
                    name = EXCLUDED.name,
                    gender = EXCLUDED.gender,
                    postcode = EXCLUDED.postcode,
                    potential_duplicate = EXCLUDED.potential_duplicate,
                    address_line1 = EXCLUDED.address_line1,
                    address_line2 = EXCLUDED.address_line2,
                    address_line3 = EXCLUDED.address_line3,
                    city = EXCLUDED.city,
                    country = EXCLUDED.country,
                    national_insurance_number = EXCLUDED.national_insurance_number;
            ", conn, transaction))
        {
            await command.ExecuteNonQueryAsync();
        }
    }

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
            await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);  // address_line1
            await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);  // address_line2
            await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);  // address_line3
            await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);  // city
            await writer.WriteAsync(DBNull.Value, NpgsqlTypes.NpgsqlDbType.Text);  // country
            await writer.WriteAsync(record.NationalInsuranceNumber, NpgsqlTypes.NpgsqlDbType.Text);  // national_insurance_number
        }

        await writer.CompleteAsync();
    }
}
