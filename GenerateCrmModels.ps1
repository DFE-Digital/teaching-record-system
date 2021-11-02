$ErrorActionPreference = "Stop"

$userSecretsId = "DqtApi"

function Get-UserSecret {
    [CmdletBinding()]
    param (
        [string]$Key
    )

    $secrets = dotnet user-secrets --id $userSecretsId list

    $value = $secrets -match "^${Key}" -split " = " | Select-Object -Skip 1

    if ($null -eq $value) {
        Write-Error "No secret found with key ${Key}"
    }

    return $value
}

$crmUrl = Get-UserSecret "CrmUrl"
$crmClientId = Get-UserSecret "CrmClientId"
$crmClientSecret = Get-UserSecret "CrmClientSecret"

$connectionString = "AuthType=ClientSecret;url=${crmUrl};ClientId=${crmClientId};ClientSecret=${crmClientSecret}"

$coreToolsFolder = (Join-Path $PSScriptRoot tools coretools)

Copy-Item (Join-Path $PSScriptRoot tools DqtApi.CrmSvcUtilFilter filter.xml) (Join-Path $coreToolsFolder filter.xml)

$namespace = "DqtApi.Models"
$output = Join-Path -Path $PSScriptRoot -ChildPath src DqtApi Models GeneratedCode.cs
mkdir (Split-Path $output) -Force | Out-Null

$crmSvcUtil = Join-Path -Path $coreToolsFolder -ChildPath "CrmSvcUtil.exe"
& $crmSvcUtil `
    /connectionstring:${connectionString} `
    /out:${output} `
    /namespace:${namespace} `
    /emitfieldsclasses `
    /servicecontextname:DqtServiceContext `
    /codewriterfilter:DqtApi.CrmSvcUtilFilter.CodeWriterFilter,DqtApi.CrmSvcUtilFilter
