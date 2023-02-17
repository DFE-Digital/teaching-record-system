$ErrorActionPreference = "Stop"

$userSecretsId = "DqtApiApimIntegrationTests"

function Get-UserSecrets {
    $secrets = dotnet user-secrets --id $userSecretsId list

    $result = @{}

    foreach ($line in ($secrets -Split [Environment]::NewLine)) {
        $parts = $line -split " = "
        $key = $parts[0]
        $value = $parts[1]

        $result[$key] = $value
    }

    return $result
}

$secrets = Get-UserSecrets

$apimKey = $secrets["ApimKey"]
$crmHost = $secrets["CrmHost"]
$crmClientId = $secrets["CrmClientId"]
$crmClientSecret = $secrets["CrmClientSecret"]
$adTenantId = $secrets["AdTenantId"]

function GetCrmToken {
    $params = @{
        client_id = $crmClientId;
        client_secret = $crmClientSecret;
        grant_type = "client_credentials";
        scope = "https://${CrmHost}/.default"
    }

    return (Invoke-RestMethod `
        -Uri "https://login.microsoftonline.com/${adTenantId}/oauth2/v2.0/token" `
        -Method POST `
        -Body $params).access_token
}

$crmToken = GetCrmToken

$settingsFile = "apim.runsettings"

function CreateSettingsFile {
    $additionalEnvVars = ""

    foreach ($secret in $secrets.Keys) {
        $key = $secret -replace ":", "__"
        $additionalEnvVars += "      <${key}>$($secrets[$secret])</${key}>" + [Environment]::NewLine
    }

    $additionalEnvVars = $additionalEnvVars.Substring(0, $additionalEnvVars.Length - [Environment]::NewLine.Length)

@"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <AdditionalHeadersJson>{ &quot;Authorization&quot;: &quot;Bearer ${crmToken}&quot;, &quot;Ocp-Apim-Subscription-Key&quot;: &quot;${apimKey}&quot; }</AdditionalHeadersJson>
${additionalEnvVars}
    </EnvironmentVariables>
  </RunConfiguration>
</RunSettings>
"@ | Out-File $settingsFile
}

CreateSettingsFile

dotnet build

$vstest = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
& $vstest bin\Debug\net5.0\DqtApi.FunctionalTests.dll `
    --Settings:$settingsFile `
    --TestCaseFilter:"FullyQualifiedName~GetTeacher"
