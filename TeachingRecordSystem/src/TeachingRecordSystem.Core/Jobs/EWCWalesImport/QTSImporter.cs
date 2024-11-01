using System.Diagnostics.Metrics;
using System.Globalization;
using CsvHelper;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;
public class QTSImporter(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task Import(StreamReader csvReaderStream, Guid IntegrationTransactionId)
    {
        using (var csv = new CsvReader(csvReaderStream, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<EWCWalesQTSFileImportData>().ToList();
            var validationMessages = new List<EWCWalesImportValidationMessage>();

            foreach (var row in records)
            {
                var validationFailures = await Validate(row);


                //Create ITT
                var ittQuery = new CreateInitialTeacherTrainingQuery()
                {
                    PersonId = Guid.NewGuid(),
                    ITTQualificationId = Guid.NewGuid(),
                    CountryId = Guid.NewGuid(),
                    Result = dfeta_ITTResult.Pass
                };
                var ittId = await crmQueryDispatcher.ExecuteQuery(ittQuery);

                //Create Qualification
                var qualificationQuery = new CreateQualificationQuery()
                {
                    PersonId = Guid.NewGuid(),
                    HECountryId = Guid.NewGuid(),
                    HECourseLength = "",
                    HEEstablishmentId = Guid.NewGuid(),
                    PqClassCode = "",
                    HEQualificationId = Guid.NewGuid(),
                    HEClassDivision = Guid.NewGuid(),
                    HESubject1id = Guid.NewGuid(),
                    HESubject2id = Guid.NewGuid(),
                    HESubject3id = Guid.NewGuid(),
                    Type = 0,
                };
                var qualificationId = await crmQueryDispatcher.ExecuteQuery(qualificationQuery);

                //QTS
                var qtsQuery = new CreateQTSQuery()
                {
                    PersonId = Guid.NewGuid(),
                    TeacherStatusId = Guid.NewGuid(),
                    QTSDate = null
                };
                var createQtsId = await crmQueryDispatcher.ExecuteQuery(qtsQuery);


                //Add QTS Notiication message
                //Update IntegrationTransactionRecord
                //Update Integration Transaction Count
            }
        }
    }

    public async Task<QTSImportLookupData> GetLookupData(EWCWalesQTSFileImportData row)
    {
        //contact
        var (personMatchStatus, personId) = await FindMatchingTeacherRecord(row);

        var lookupData = new QTSImportLookupData
        {
            PersonId = personId,
            PersonMatchStatus = personMatchStatus,
            OrganisationId = null,
            OrganisationMatchStatus = null,
            IttQualificationId = null,
            IttQualificationMatchStatus = null,
            IttSubject1Id = null,
            IttSubject1MatchStatus = null,
            IttSubject2Id = null,
            IttSubject2MatchStatus = null,
            PqEstablishmentId = null,
            PqEstablishmentMatchStatus = null,
            PQCountryId = null,
            PQCountryMatchStatus = null,
            PQHEQualificationId = null,
            PQHEQualificationMatchStatus = null,
            PQSubject1Id = null,
            PQSubject1MatchStatus = null,
            PQSubject2Id = null,
            PQSubject2MatchStatus = null,
            PQSubject3Id = null,
            PQSubject3MatchStatus = null,
            TeacherStatusId = null,
            TeacherStatusMatchStatus = null
        };

        return lookupData;
    }

    public async Task<List<string>> Validate(EWCWalesQTSFileImportData row)
    {
        var validationFailures = new List<string>();
        var lookups = await GetLookupData(row);

        switch (lookups.PersonMatchStatus)
        {
            case EWCWalesMatchStatus.NoAssociatedQTS:
                break;
            case EWCWalesMatchStatus.NoMatch:
                validationFailures.Add(string.Format("Teacher with TRN {0} was not found.", row.QtsRefNo));
                break;
            case EWCWalesMatchStatus.TRNandDOBMatchFailed:
                validationFailures.Add(string.Format("For TRN {0} Date of Birth does not match with the existing record.", row.QtsRefNo));
                break;
            case EWCWalesMatchStatus.MultipleTRNMatched:
                validationFailures.Add(string.Format("TRN {0} was matched to more than one record in the system.", row.QtsRefNo));
                break;
            case EWCWalesMatchStatus.TeacherInactive:
                validationFailures.Add(string.Format("Teacher with TRN {0} is inactive.", row.QtsRefNo));
                break;
            case EWCWalesMatchStatus.TeacherHasQTS:
                validationFailures.Add(string.Format("Teacher with TRN {0} has QTS already.", row.QtsRefNo));
                break;
        }

        //QTS REF
        if (String.IsNullOrEmpty(row.QtsRefNo))
            validationFailures.Add("QTS Ref Number");


        //Date Of birth
        if (String.IsNullOrEmpty(row.DateOfBirth))
        {
            validationFailures.Add("Date of Birth");
        }
        else
        {
            DateTime dateOfBirth;
            if (!DateTime.TryParse(row.DateOfBirth, out dateOfBirth))
                validationFailures.Add("Validation Failed: Invalid Date of Birth");
        }

        //QTS Date
        if (String.IsNullOrEmpty(row.QtsDate))
        {
            validationFailures.Add("QTS Date");
        }
        else
        {
            DateTime date;
            if (false == DateTime.TryParse(row.QtsDate, out date))
                validationFailures.Add("Validation Failed: Invalid QTS Date");
        }

        //IttEstabCode
        if (!string.IsNullOrEmpty(row.IttEstabCode))
        {
            if (lookups.OrganisationMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Organisation with ITT Establishment Code {row.IttEstabCode} was not found.");
            else if (lookups.OrganisationMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple organisations with ITT Establishment Code {row.IttEstabCode} found.");
        }

        // ITT Qualification
        if (!string.IsNullOrEmpty(row.IttQualCode))
        {
            if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"ITT qualification with code {row.IttQualCode} was not found.");
            else if (lookups.OrganisationMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple ITT qualifications with code {row.IttQualCode} found.");
        }

        // IIT Subject 1
        if (!string.IsNullOrEmpty(row.IttSubjectCode1))
        {
            if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"ITT subject with code {row.IttSubjectCode1} was not found.");
            else if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple ITT subjects with code {row.IttSubjectCode1} found.");
        }

        // IIT Subject 2
        if (!string.IsNullOrEmpty(row.IttSubjectCode2))
        {
            if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"ITT subject with code {row.IttSubjectCode2} was not found.");
            else if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple ITT subjects with code {row.IttSubjectCode2} found.");
        }

        // PQ Establishment
        if (!string.IsNullOrEmpty(row.PqEstabCode))
        {
            if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Organisation with PQ Establishment Code {row.PqEstabCode} was not found.");
            else if (lookups.IttQualificationMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple organisations with PQ Establishment Code {row.PqEstabCode} found.");
        }

        // PQ Country
        if (!string.IsNullOrEmpty(row.Country))
        {
            if (lookups.PQCountryMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Country with PQ Country Code {row.Country} was not found.");
            else if (lookups.PQCountryMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple countries with PQ Country Code {row.Country} found.");
        }

        // PQ HE Qualification
        if (!string.IsNullOrEmpty(row.PqQualCode))
        {
            if (lookups.PQHEQualificationMatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Qualification with PQ Qualification Code {row.PqQualCode} was not found.");
            else if (lookups.PQHEQualificationMatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple qualifications with PQ Qualification Code {row.PqQualCode} found.");
        }

        // PQ Subject 1
        if (!string.IsNullOrEmpty(row.PqSubjectCode1))
        {
            if (lookups.PQSubject1MatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Subject with PQ Subject Code {row.PqSubjectCode1} was not found.");
            else if (lookups.PQSubject1MatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple subjects with PQ Subject Code {row.PqSubjectCode1} found.");
        }

        // PQ Subject 2
        if (!string.IsNullOrEmpty(row.PqSubjectCode2))
        {
            if (lookups.PQSubject2MatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Subject with PQ Subject Code {row.PqSubjectCode2} was not found.");
            else if (lookups.PQSubject2MatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple subjects with PQ Subject Code {row.PqSubjectCode2} found.");
        }

        // PQ Subject 3
        if (!string.IsNullOrEmpty(row.PqSubjectCode3))
        {
            if (lookups.PQSubject3MatchStatus == EWCWalesMatchStatus.NoMatch)
                validationFailures.Add($"Subject with PQ Subject Code {row.PqSubjectCode3} was not found.");
            else if (lookups.PQSubject3MatchStatus == EWCWalesMatchStatus.MultipleMatchesFound)
                validationFailures.Add($"Multiple subjects with PQ Subject Code {row.PqSubjectCode3} found.");
        }

        return validationFailures;
    }


    public async Task<(EWCWalesMatchStatus, Guid? PersonId)> FindMatchingTeacherRecord(EWCWalesQTSFileImportData item)
    {
        var contacts = await crmQueryDispatcher.ExecuteQuery(
                    new GetActiveContactsByTrnsQuery(
                        new[] { item.QtsRefNo },
                        new ColumnSet(
                            Contact.Fields.dfeta_TRN,
                            Contact.Fields.BirthDate,
                            Contact.Fields.dfeta_QTSDate,
                            Contact.Fields.dfeta_qtlsdate)));

        if (contacts.Count == 0)
            return (EWCWalesMatchStatus.NoMatch, null);

        if (contacts.Count > 1)
            return (EWCWalesMatchStatus.MultipleTRNMatched, null);

        var contact = contacts.First().Value!;
        if (!contact.BirthDate.HasValue)
            return (EWCWalesMatchStatus.TRNandDOBMatchFailed, null);

        var qtsRegistrations = await crmQueryDispatcher.ExecuteQuery(
            new GetActiveQtsRegistrationsByContactIdsQuery(
                new[] { contact.ContactId!.Value },
                new ColumnSet(
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId)
                )
            );

        if (qtsRegistrations.Count > 0)
            return (EWCWalesMatchStatus.TeacherHasQTS, contact.ContactId!);
        else
            return (EWCWalesMatchStatus.NoAssociatedQTS, contact.ContactId!);
    }

    public class QTSImportLookupData
    {
        public required Guid? PersonId { get; set; }
        public required EWCWalesMatchStatus? PersonMatchStatus { get; set; }
        public required Guid? OrganisationId { get; set; }
        public required EWCWalesMatchStatus? OrganisationMatchStatus { get; set; }
        public required Guid? IttQualificationId { get; set; }
        public required EWCWalesMatchStatus? IttQualificationMatchStatus { get; set; }
        public required Guid? IttSubject1Id { get; set; }
        public required EWCWalesMatchStatus? IttSubject1MatchStatus { get; set; }
        public required Guid? IttSubject2Id { get; set; }
        public required EWCWalesMatchStatus? IttSubject2MatchStatus { get; set; }
        public required Guid? PqEstablishmentId { get; set; }
        public required EWCWalesMatchStatus? PqEstablishmentMatchStatus { get; set; }
        public required Guid? PQCountryId { get; set; }
        public required EWCWalesMatchStatus? PQCountryMatchStatus { get; set; }
        public required Guid? PQHEQualificationId { get; set; }
        public required EWCWalesMatchStatus? PQHEQualificationMatchStatus { get; set; }
        public required Guid? PQSubject1Id { get; set; }
        public required EWCWalesMatchStatus? PQSubject1MatchStatus { get; set; }
        public required Guid? PQSubject2Id { get; set; }
        public required EWCWalesMatchStatus? PQSubject2MatchStatus { get; set; }
        public required Guid? PQSubject3Id { get; set; }
        public required EWCWalesMatchStatus? PQSubject3MatchStatus { get; set; }
        public required Guid? TeacherStatusId { get; set; }
        public required EWCWalesMatchStatus? TeacherStatusMatchStatus { get; set; }
    }
}
