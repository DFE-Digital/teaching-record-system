using System.Text;
using MediatR;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetInductionCertificateHandler : IRequestHandler<GetInductionCertificateRequest, GetCertificateResponse?>
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

    public async Task<GetCertificateResponse?> Handle(GetInductionCertificateRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN
            });

        if (teacher == null)
        {
            return null;
        }

        var (induction, _) = await _dataverseAdapter.GetInductionByTeacher(
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

        if ((induction?.dfeta_InductionStatus != dfeta_InductionStatus.Pass
            && induction?.dfeta_InductionStatus != dfeta_InductionStatus.PassedinWales)
            || induction?.dfeta_CompletionDate == null)
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
            { TrnFormField, request.Trn },
            {
                InductionDateFormField,
                induction.dfeta_CompletionDate.HasValue ?
                    induction.dfeta_CompletionDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true).ToString("d MMMM yyyy") :
                    string.Empty
            }
        };

        var pdfStream = await _certificateGenerator.GenerateCertificate("Induction certificate.pdf", fieldValues);
        using var output = new MemoryStream();
        pdfStream.CopyTo(output);

        return new GetCertificateResponse()
        {
            FileDownloadName = $"InductionCertificate.pdf",
            FileContents = output.ToArray()
        };
    }
}
