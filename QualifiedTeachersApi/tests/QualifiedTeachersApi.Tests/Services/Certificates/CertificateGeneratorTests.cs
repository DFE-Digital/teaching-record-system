#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;
using QualifiedTeachersApi.Services.Certificates;
using Xunit;

namespace QualifiedTeachersApi.Tests.Services.Certificates;

public class CertificateGeneratorTests : ApiTestBase
{
    public CertificateGeneratorTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task GenerateCertificate_WhenCalled_GetsTemplateFromBlobStorageAndSetFieldValuesAsExpected()
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

        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        var pdfPath = Path.Combine(projectDirectory!, "Resources", "TestCertificate.pdf");
        using var pdfStream = File.OpenRead(pdfPath);

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
