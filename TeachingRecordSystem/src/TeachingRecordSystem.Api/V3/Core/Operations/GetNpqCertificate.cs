using System.Text;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetNpqCertificateCommand(string Trn, Guid QualificationId);

public class GetNpqCertificateHandler(IDataverseAdapter dataverseAdapter, ICertificateGenerator certificateGenerator)
{
    private const string FullNameFormField = "Full Name";
    private const string PassDateFormField = "Pass Date";

    public async Task<FileDownloadInfo?> Handle(GetNpqCertificateCommand command)
    {
        var qualification = await dataverseAdapter.GetQualificationById(
            command.QualificationId,
            columnNames: new[]
            {
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.StateCode,
                dfeta_qualification.Fields.dfeta_PersonId
            },
            contactColumnNames: new[]
            {
                Contact.PrimaryIdAttribute,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_TRN
            });

        if (qualification == null ||
            !qualification.dfeta_CompletionorAwardDate.HasValue ||
            !qualification.dfeta_Type!.Value.IsNpq() ||
            qualification.StateCode != dfeta_qualificationState.Active)
        {
            return null;
        }

        var teacher = qualification.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute);
        if (teacher == null || teacher.dfeta_TRN != command.Trn)
        {
            return null;
        }

        var fullName = new StringBuilder();
        fullName.Append($"{teacher.FirstName} ");
        if (!string.IsNullOrWhiteSpace(teacher.MiddleName))
        {
            fullName.Append($"{teacher.MiddleName} ");
        }

        fullName.Append(teacher.LastName);

        var fieldValues = new Dictionary<string, string>()
        {
            { FullNameFormField, fullName.ToString() },
            { PassDateFormField, qualification.dfeta_CompletionorAwardDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("d MMMM yyyy") }
        };

        var pdfStream = await certificateGenerator.GenerateCertificate($"{qualification.dfeta_Type} Certificate.pdf", fieldValues);

        return new FileDownloadInfo(pdfStream, $"{qualification.dfeta_Type}Certificate.pdf", "application/pdf");
    }
}
