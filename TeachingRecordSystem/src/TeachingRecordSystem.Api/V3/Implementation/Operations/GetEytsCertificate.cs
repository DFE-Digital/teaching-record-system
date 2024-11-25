using System.Text;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetEytsCertificateCommand(string Trn);

public class GetEytsCertificateHandler(IDataverseAdapter dataverseAdapter, ICertificateGenerator certificateGenerator)
{
    private const string FullNameFormField = "Full Name";
    private const string TrnFormField = "TRN";
    private const string EytsDateFormField = "EYTSDate";

    public async Task<FileDownloadInfo?> HandleAsync(GetEytsCertificateCommand command)
    {
        var teacher = await dataverseAdapter.GetTeacherByTrnAsync(
            command.Trn,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_EYTSDate
            });

        if (teacher?.dfeta_EYTSDate is null)
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
            { TrnFormField, teacher.dfeta_TRN },
            { EytsDateFormField, teacher.dfeta_EYTSDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("d MMMM yyyy") }
        };

        var pdfStream = await certificateGenerator.GenerateCertificateAsync("EYTS certificate.pdf", fieldValues);

        return new FileDownloadInfo(pdfStream, $"EYTSCertificate.pdf", "application/pdf");
    }
}
