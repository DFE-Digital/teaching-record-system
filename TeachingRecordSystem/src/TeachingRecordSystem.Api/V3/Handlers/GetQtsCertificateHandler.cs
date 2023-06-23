using System.Text;
using MediatR;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using TeachingRecordSystem.Api.Services.Certificates;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetQtsCertificateHandler : IRequestHandler<GetQtsCertificateRequest, GetCertificateResponse?>
{
    private const string QtsFormNameField = "Full Name";
    private const string QtsFormTrnField = "TRN";
    private const string QtsFormDateField = "QTSDate";

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
                Contact.Fields.dfeta_QTSDate
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
            { QtsFormDateField, teacher.dfeta_QTSDate!.Value.ToString("dd MMMM yyyy") }
        };

        var pdfStream = await _certificateGenerator.GenerateCertificate("QTS certificate.pdf", fieldValues);
        using var output = new MemoryStream();
        pdfStream.CopyTo(output);

        return new GetCertificateResponse()
        {
            FileDownloadName = $"QTSCertificate.pdf",
            FileContents = output.ToArray()
        };
    }
}
