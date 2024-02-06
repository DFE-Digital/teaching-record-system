using System.Text;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetQtsCertificateCommand(string Trn);

public class GetQtsCertificateHandler(IDataverseAdapter dataverseAdapter, ICertificateGenerator certificateGenerator)
{
    private const string QtsFormNameField = "Full Name";
    private const string QtsFormTrnField = "TRN";
    private const string QtsFormDateField = "QTSDate";

    private const string QtsAwardedInWalesTeacherStatusValue = "213";

    public async Task<FileDownloadInfo?> Handle(GetQtsCertificateCommand command)
    {
        var teacher = await dataverseAdapter.GetTeacherByTrn(
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

        var qtsAwardedInWalesStatus = await dataverseAdapter.GetTeacherStatus(QtsAwardedInWalesTeacherStatusValue, null);
        var qtsRegistrations = await dataverseAdapter.GetQtsRegistrationsByTeacher(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_qtsregistration.Fields.dfeta_QTSDate,
                dfeta_qtsregistration.Fields.dfeta_TeacherStatusId
            });

        var qtsRegistration = qtsRegistrations.SingleOrDefault(qts => qts.dfeta_QTSDate is not null);
        if (qtsRegistration is null || qtsRegistration.dfeta_TeacherStatusId.Id == qtsAwardedInWalesStatus.Id)
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

        var pdfStream = await certificateGenerator.GenerateCertificate(certificateName, fieldValues);

        return new FileDownloadInfo(pdfStream, "QTSCertificate.pdf", "application/pdf");
    }
}
