using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateGenerateWebhookSignatureCertificateCommand(IConfiguration configuration)
    {
        var command = new Command(
            "generate-webhook-signature-certificate",
            "Generates a new self-signed certificate and outputs its private key and certificate to key.pem and certificate.pem, respectively.");

        command.SetAction(
            _ =>
            {
                var key = ECDsa.Create(ECCurve.NamedCurves.nistP384);
                var certRequest = new CertificateRequest("CN=Teaching Record System", key, HashAlgorithmName.SHA384);
                var cert = certRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
                var certPem = cert.ExportCertificatePem();
                var keyPem = key.ExportECPrivateKeyPem();

                File.WriteAllText("key.pem", keyPem);
                File.WriteAllText("certificate.pem", certPem);
            });

        return command;
    }
}
