using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V3.Requests;

namespace QualifiedTeachersApi.V3.Handlers;

public class GetQtsCertificateHandler : IRequestHandler<GetQtsCertificateRequest, FileResult>
{
    private const string QtsFormNameField = "Full Name";
    private const string QtsFormTrnField = "TRN";
    private const string QtsFormDateField = "QTSDate";

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly BlobServiceClient _blobServiceClient;

    public GetQtsCertificateHandler(
        IDataverseAdapter dataverseAdapter,
        BlobServiceClient blobServiceClient)
    {
        _dataverseAdapter = dataverseAdapter;
        _blobServiceClient = blobServiceClient;
    }

    private FileResult GetPdfStream(PdfDocument pdf)
    {
        byte[] pdfData;
        using (var stream = new MemoryStream())
        {
            pdf.Save(stream, false);
            pdfData = stream.ToArray();
        }

        return new FileContentResult(pdfData, "application/pdf")
        {
            FileDownloadName = "QTS Certificate.pdf"
        };
    }

    public async Task<FileResult> Handle(GetQtsCertificateRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_QTSDate
            });

        if (teacher?.dfeta_QTSDate is null)
        {
            return null;
        }

        var qtsPdf = await GenerateQtsPdf(teacher);
        return GetPdfStream(qtsPdf);
    }

    private async Task<PdfDocument> GenerateQtsPdf(Contact teacher)
    {
        var pdf = await DownloadPdfTemplate();

        SetFormContent(pdf.AcroForm, teacher);
        SetFormAppearance(pdf.AcroForm);
        SetDocumentPermissions(pdf);

        return pdf;
    }

    private async Task<PdfDocument> DownloadPdfTemplate()
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient("certificates").GetBlobClient("QTS certificate.pdf");

        MemoryStream stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);

        stream.Position = 0;

        return PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
    }

    private void SetFormContent(PdfAcroForm form, Contact teacher)
    {
        form.Fields[QtsFormNameField].Value = new PdfString($"{teacher.FirstName} {teacher.LastName}");
        form.Fields[QtsFormNameField].ReadOnly = true;

        form.Fields[QtsFormTrnField].Value = new PdfString(teacher.dfeta_TRN);
        form.Fields[QtsFormTrnField].ReadOnly = true;

        form.Fields[QtsFormDateField].Value = new PdfString(teacher.dfeta_QTSDate!.Value.ToLongDateString());
        form.Fields[QtsFormDateField].ReadOnly = true;
    }

    private void SetFormAppearance(PdfAcroForm form)
    {
        if (form.Elements.ContainsKey("/NeedAppearances"))
        {
            form.Elements["/NeedAppearances"] = new PdfBoolean(true);
        }
        else
        {
            form.Elements.Add("/NeedAppearances", new PdfBoolean(true));
        }
    }

    private void SetDocumentPermissions(PdfDocument pdf)
    {
        pdf.SecuritySettings.OwnerPassword = Guid.NewGuid().ToString();

        pdf.SecuritySettings.PermitPrint = true;
        pdf.SecuritySettings.PermitFullQualityPrint = true;

        pdf.SecuritySettings.PermitAnnotations = false;
        pdf.SecuritySettings.PermitAssembleDocument = false;
        pdf.SecuritySettings.PermitFormsFill = false;
        pdf.SecuritySettings.PermitExtractContent = false;
        pdf.SecuritySettings.PermitAccessibilityExtractContent = false;
        pdf.SecuritySettings.PermitModifyDocument = false;
    }
}
