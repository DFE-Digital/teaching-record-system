using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.Services.Certificates;
using QualifiedTeachersApi.V3.Requests;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Handlers;

public class GetNpqCertificateHandler : IRequestHandler<GetNpqCertificateRequest, GetCertificateResponse>
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

    public async Task<GetCertificateResponse> Handle(GetNpqCertificateRequest request, CancellationToken cancellationToken)
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
                Contact.Fields.LastName
            });

        if (qualification == null
            || !qualification.dfeta_CompletionorAwardDate.HasValue
            || !qualification.dfeta_Type.Value.IsNpq()
            || qualification.StateCode != dfeta_qualificationState.Active)
        {
            return null;
        }

        var teacher = qualification.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute);
        if (teacher == null)
        {
            return null;
        }

        var fieldValues = new Dictionary<string, string>()
        {
            { FullNameFormField, $"{teacher.FirstName} {teacher.MiddleName} {teacher.LastName}" },
            { PassDateFormField, qualification.dfeta_CompletionorAwardDate.Value.ToLongDateString() }
        };

        var pdfStream = await _certificateGenerator.GenerateCertificate($"{qualification.dfeta_Type} Certificate.pdf", fieldValues);
        using var output = new MemoryStream();
        pdfStream.CopyTo(output);

        return new GetCertificateResponse()
        {
            FileDownloadName = $"{qualification.dfeta_Type}Certificate.pdf",
            FileContents = output.ToArray()
        };
    }
}
