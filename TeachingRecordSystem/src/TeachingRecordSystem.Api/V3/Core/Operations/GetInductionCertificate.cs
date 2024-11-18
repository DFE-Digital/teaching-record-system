using System.Text;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetInductionCertificateCommand(string Trn);

public class GetInductionCertificateHandler
{
    private const string FullNameFormField = "Full Name";
    private const string TrnFormField = "TRN";
    private const string InductionDateFormField = "Induction Date";

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICertificateGenerator _certificateGenerator;

    public GetInductionCertificateHandler(
        IDataverseAdapter dataverseAdapter,
        ICertificateGenerator certificateGenerator)
    {
        _dataverseAdapter = dataverseAdapter;
        _certificateGenerator = certificateGenerator;
    }

    public async Task<FileDownloadInfo?> HandleAsync(GetInductionCertificateCommand command)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrnAsync(
            command.Trn,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN
            });

        if (teacher == null)
        {
            return null;
        }

        var (induction, _) = await _dataverseAdapter.GetInductionByTeacherAsync(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_induction.PrimaryIdAttribute,
                dfeta_induction.Fields.dfeta_StartDate,
                dfeta_induction.Fields.dfeta_CompletionDate,
                dfeta_induction.Fields.dfeta_InductionStatus
            },
            contactColumnNames: new[]
            {
                Contact.PrimaryIdAttribute,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName
            });

        if (induction?.dfeta_InductionStatus != dfeta_InductionStatus.Pass &&
            induction?.dfeta_InductionStatus != dfeta_InductionStatus.PassedinWales ||
            induction?.dfeta_CompletionDate == null)
        {
            return null;
        }

        var contact = induction.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute);

        var fullName = new StringBuilder();
        fullName.Append($"{contact.FirstName} ");
        if (!string.IsNullOrWhiteSpace(contact.MiddleName))
        {
            fullName.Append($"{contact.MiddleName} ");
        }

        fullName.Append(contact.LastName);

        var fieldValues = new Dictionary<string, string>()
        {
            { FullNameFormField, fullName.ToString() },
            { TrnFormField, command.Trn },
            {
                InductionDateFormField,
                induction.dfeta_CompletionDate.HasValue ?
                    induction.dfeta_CompletionDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("d MMMM yyyy") :
                    string.Empty
            }
        };

        var pdfStream = await _certificateGenerator.GenerateCertificateAsync("Induction certificate.pdf", fieldValues);

        return new FileDownloadInfo(pdfStream, $"InductionCertificate.pdf", "application/pdf");
    }
}
