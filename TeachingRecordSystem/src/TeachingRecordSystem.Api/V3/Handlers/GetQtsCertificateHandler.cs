using System.Text;
using MediatR;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Dqt;
using TeachingRecordSystem.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetQtsCertificateHandler : IRequestHandler<GetQtsCertificateRequest, GetCertificateResponse?>
{
    private const string QtsFormNameField = "Full Name";
    private const string QtsFormTrnField = "TRN";
    private const string QtsFormDateField = "QTSDate";

    private const string QtsAwardedInWalesTeacherStatusValue = "213";

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICertificateGenerator _certificateGenerator;

    public GetQtsCertificateHandler(
        IDataverseAdapter dataverseAdapter,
        ICertificateGenerator certificateGenerator)
    {
        _dataverseAdapter = dataverseAdapter;
        _certificateGenerator = certificateGenerator;
    }

    public async Task<GetCertificateResponse?> Handle(GetQtsCertificateRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
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

        var qtsAwardedInWalesStatus = await _dataverseAdapter.GetTeacherStatus(QtsAwardedInWalesTeacherStatusValue, null);
        var qtsRegistrations = await _dataverseAdapter.GetQtsRegistrationsByTeacher(
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

        var pdfStream = await _certificateGenerator.GenerateCertificate(certificateName, fieldValues);
        using var output = new MemoryStream();
        pdfStream.CopyTo(output);

        return new GetCertificateResponse()
        {
            FileDownloadName = $"QTSCertificate.pdf",
            FileContents = output.ToArray()
        };
    }
}
