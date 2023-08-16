using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;
using TeachingRecordSystem.Core.Services.Certificates;

namespace TeachingRecordSystem.Core.Tests.Services.Certificates;

public class CertificateGeneratorTests
{
    [Fact]
    public async Task GenerateCertificate_GetsTemplateFromBlobStorageAndSetFieldValuesAsExpected()
    {
        // Arrange
        var blobServiceClient = Mock.Of<BlobServiceClient>();
        var blobContainerClient = Mock.Of<BlobContainerClient>();
        var blobClient = Mock.Of<BlobClient>();
        var templateName = "My Certificate Template.pdf";
        var field1Name = "Full Name";
        var field1Value = $"{Faker.Name.First()} {Faker.Name.Middle()} {Faker.Name.Last()}";
        var field2Name = "TRN";
        var field2Value = "1234567";
        var field3Name = "QTSDate";
        var field3Value = "12 November 2020";
        var fieldValues = new Dictionary<string, string>()
        {
            { field1Name, field1Value },
            { field2Name, field2Value },
            { field3Name, field3Value }
        };

        using var pdfStream = typeof(CertificateGeneratorTests).Assembly.GetManifestResourceStream($"{typeof(CertificateGeneratorTests).Namespace}.TestCertificate.pdf") ??
            throw new Exception("Failed to find TestCertificate.pdf.");

        Mock.Get(blobClient)
            .Setup(b => b.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfStream);
        Mock.Get(blobContainerClient)
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClient);
        Mock.Get(blobServiceClient)
            .Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClient);

        // Act
        var certificateGenerator = new CertificateGenerator(blobServiceClient);
        var stream = await certificateGenerator.GenerateCertificate(templateName, fieldValues);
        var pdfGenerated = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);

        // Assert
        Assert.NotNull(pdfGenerated);
        var field1 = pdfGenerated.AcroForm.Fields[field1Name];
        Assert.NotNull(field1);
        Assert.IsType<PdfTextField>(field1);
        Assert.Equal(field1Value, ((PdfTextField)field1).Text);
        var field2 = pdfGenerated.AcroForm.Fields[field2Name];
        Assert.NotNull(field2);
        Assert.IsType<PdfTextField>(field2);
        Assert.Equal(field2Value, ((PdfTextField)field2).Text);
        var field3 = pdfGenerated.AcroForm.Fields[field3Name];
        Assert.NotNull(field3);
        Assert.IsType<PdfTextField>(field3);
        Assert.Equal(field3Value, ((PdfTextField)field3).Text);
    }
}
