using System.Security.Cryptography;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateGenerateKeyCommand(IConfiguration configuration)
    {
        var generateKeyCommand = new Command("generate-key", "Generates an RSA key.");

        generateKeyCommand.SetHandler(
            () =>
            {
                using var rsa = RSA.Create(keySizeInBits: 2048);
                Console.WriteLine(rsa.ToXmlString(includePrivateParameters: true));
            });

        return generateKeyCommand;
    }
}
