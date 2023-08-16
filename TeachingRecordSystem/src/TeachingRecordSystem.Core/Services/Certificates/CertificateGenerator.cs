using Azure.Storage.Blobs;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace TeachingRecordSystem.Core.Services.Certificates;

public class CertificateGenerator : ICertificateGenerator
{
    private const string CertificatesContainerName = "certificates";
    private readonly BlobServiceClient _blobServiceClient;

    public CertificateGenerator(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Stream> GenerateCertificate(string templateName, IReadOnlyDictionary<string, string> fieldValues)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(CertificatesContainerName);
        var blobClient = blobContainerClient.GetBlobClient(templateName);

        using var stream = await blobClient.OpenReadAsync();
        using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
        ApplyContentToTemplate(pdfDocument, fieldValues);
        var output = new MemoryStream();
        pdfDocument.Save(output);
        return output;
    }

    private void ApplyContentToTemplate(PdfDocument pdfDocument, IReadOnlyDictionary<string, string> fieldValues)
    {
        // Remove any template specific metadata (some of which is misleading in current templates)
        pdfDocument.Info.Author = string.Empty;
        pdfDocument.Info.Title = string.Empty;
        pdfDocument.Info.Subject = string.Empty;
        pdfDocument.Info.Creator = string.Empty;
        pdfDocument.Info.CreationDate = pdfDocument.Info.ModificationDate;

        foreach (var kvp in fieldValues)
        {
            pdfDocument.AcroForm.Fields[kvp.Key].Value = new PdfString(kvp.Value);
            pdfDocument.AcroForm.Fields[kvp.Key].ReadOnly = true;
        }

        if (pdfDocument.AcroForm.Elements.ContainsKey("/NeedAppearances"))
        {
            pdfDocument.AcroForm.Elements["/NeedAppearances"] = new PdfBoolean(true);
        }
        else
        {
            pdfDocument.AcroForm.Elements.Add("/NeedAppearances", new PdfBoolean(true));
        }

        pdfDocument.SecuritySettings.OwnerPassword = Guid.NewGuid().ToString();
        pdfDocument.SecuritySettings.PermitPrint = true;
        pdfDocument.SecuritySettings.PermitFullQualityPrint = true;
        pdfDocument.SecuritySettings.PermitAnnotations = false;
        pdfDocument.SecuritySettings.PermitAssembleDocument = false;
        pdfDocument.SecuritySettings.PermitFormsFill = false;
        pdfDocument.SecuritySettings.PermitExtractContent = false;
        pdfDocument.SecuritySettings.PermitAccessibilityExtractContent = false;
        pdfDocument.SecuritySettings.PermitModifyDocument = false;
    }
}
