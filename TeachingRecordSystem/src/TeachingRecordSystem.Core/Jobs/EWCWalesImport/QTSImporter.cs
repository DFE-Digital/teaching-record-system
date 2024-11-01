using System.Globalization;
using CsvHelper;
using Microsoft.Xrm.Sdk.Query;
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
                var (personMatchStatus, personId) = await FindMatchingTeacherRecord(row);
                switch (personMatchStatus)
                {
                    case EWCWalesMatchStatus.NoAssociatedQTS:
                        break;
                    case EWCWalesMatchStatus.NoMatch:
                        throw new Exception(string.Format("Teacher with TRN {0} was not found.", row.QtsRefNo));
                    case EWCWalesMatchStatus.TRNandDOBMatchFailed:
                        throw new Exception(string.Format("For TRN {0} Date of Birth does not match with the existing record.", row.QtsRefNo));
                    case EWCWalesMatchStatus.MultipleTRNMatched:
                        throw new Exception(string.Format("TRN {0} was matched to more than one record in the system.", row.QtsRefNo));
                    case EWCWalesMatchStatus.TeacherInactive:
                        throw new Exception(string.Format("Teacher with TRN {0} is inactive.", row.QtsRefNo));
                    case EWCWalesMatchStatus.TeacherHasQTS:
                        throw new Exception(string.Format("Teacher with TRN {0} has QTS already.", row.QtsRefNo));
                }

                //Validate
                //Create ITT
                //Create Qualification
                //Create QTS
                //Add QTS Notiication message
                //Update IntegrationTransactionRecord
                //Update Integration Transaction Count
            }
        }
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
}
