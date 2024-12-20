using System.Text;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetQtsCertificateCommand(string Trn);

public class GetQtsCertificateHandler(IDataverseAdapter dataverseAdapter, ICertificateGenerator certificateGenerator)
{
    private const string QtsFormNameField = "Full Name";
    private const string QtsFormTrnField = "TRN";
    private const string QtsFormDateField = "QTSDate";

    public async Task<FileDownloadInfo?> HandleAsync(GetQtsCertificateCommand command)
    {
        var teacher = await dataverseAdapter.GetTeacherByTrnAsync(
            command.Trn,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_InductionStatus
            });

        if (teacher?.dfeta_QTSDate is null)
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
            { QtsFormNameField, fullName.ToString() },
            { QtsFormTrnField, teacher.dfeta_TRN },
            { QtsFormDateField, teacher.dfeta_QTSDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("d MMMM yyyy") }
        };

        var certificateName = "QTS certificate.pdf";
        if (teacher.dfeta_InductionStatus == dfeta_InductionStatus.Exempt)
        {
            certificateName = "Exempt QTS certificate.pdf";
        }

        var pdfStream = await certificateGenerator.GenerateCertificateAsync(certificateName, fieldValues);

        return new FileDownloadInfo(pdfStream, "QTSCertificate.pdf", "application/pdf");
    }
}
