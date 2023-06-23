using System.Text;
using MediatR;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using TeachingRecordSystem.Api.Services.Certificates;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetEytsCertificateHandler : IRequestHandler<GetEytsCertificateRequest, GetCertificateResponse?>
{
    private const string FullNameFormField = "Full Name";
    private const string TrnFormField = "TRN";
    private const string EytsDateFormField = "EYTSDate";

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICertificateGenerator _certificateGenerator;

    public GetEytsCertificateHandler(
        IDataverseAdapter dataverseAdapter,
        ICertificateGenerator certificateGenerator)
    {
        _dataverseAdapter = dataverseAdapter;
        _certificateGenerator = certificateGenerator;
    }

    public async Task<GetCertificateResponse?> Handle(GetEytsCertificateRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
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

        var pdfStream = await _certificateGenerator.GenerateCertificate("EYTS certificate.pdf", fieldValues);
        using var output = new MemoryStream();
        pdfStream.CopyTo(output);

        return new GetCertificateResponse()
        {
            FileDownloadName = $"EYTSCertificate.pdf",
            FileContents = output.ToArray()
        };
    }
}
