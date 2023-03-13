﻿using System.Collections.Generic;
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

public class GetQtsCertificateHandler : IRequestHandler<GetQtsCertificateRequest, GetCertificateResponse>
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

    public async Task<GetCertificateResponse> Handle(GetQtsCertificateRequest request, CancellationToken cancellationToken)
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

        var fieldValues = new Dictionary<string, string>()
        {
            { QtsFormNameField, $"{teacher.FirstName} {teacher.MiddleName} {teacher.LastName}" },
            { QtsFormTrnField, teacher.dfeta_TRN },
            { QtsFormDateField, teacher.dfeta_QTSDate!.Value.ToLongDateString() }
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
