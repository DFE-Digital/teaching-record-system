using System.Text;
using MediatR;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetNpqCertificateHandler : IRequestHandler<GetNpqCertificateRequest, GetCertificateResponse?>
{
    private const string FullNameFormField = "Full Name";
    private const string PassDateFormField = "Pass Date";
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICertificateGenerator _certificateGenerator;

    public GetNpqCertificateHandler(
        IDataverseAdapter dataverseAdapter,
        ICertificateGenerator certificateGenerator)
    {
        _dataverseAdapter = dataverseAdapter;
        _certificateGenerator = certificateGenerator;
    }

    public async Task<GetCertificateResponse?> Handle(GetNpqCertificateRequest request, CancellationToken cancellationToken)
    {
        var qualification = await _dataverseAdapter.GetQualificationById(
            request.QualificationId,
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

        if (qualification == null
            || !qualification.dfeta_CompletionorAwardDate.HasValue
            || !qualification.dfeta_Type!.Value.IsNpq()
            || qualification.StateCode != dfeta_qualificationState.Active)
        {
            return null;
        }

        var teacher = qualification.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute);
        if (teacher == null
            || teacher.dfeta_TRN != request.Trn)
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

        var pdfStream = await _certificateGenerator.GenerateCertificate($"{qualification.dfeta_Type} Certificate.pdf", fieldValues);

        return new GetCertificateResponse()
        {
            FileDownloadName = $"{qualification.dfeta_Type}Certificate.pdf",
            FileContents = pdfStream
        };
    }
}
