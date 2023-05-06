[CmdletBinding()]
param (
    [switch]
    $IncludeOptionSets = $false
)

$ErrorActionPreference = "Stop"

$userSecretsId = "QualifiedTeachersApi"

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

$coreToolsFolder = (Join-Path $PSScriptRoot .. tools coretools)

function Set-Configuration {
    $entitiesConfiguration = Get-Content (Join-Path $PSScriptRoot .. "crm_attributes.json") | ConvertFrom-Json -AsHashtable

    $entitiesWhitelist = ($entitiesConfiguration.Keys | Sort-Object) -join "|"

    $attributesWhitelist = ($entitiesConfiguration.Keys | Sort-Object | ForEach-Object {
        $attrs = ($entitiesConfiguration[$_] | Sort-Object) -join ","
        "$($_):$attrs"
    }) -join "|"

    $configPath = Join-Path $coreToolsFolder "CrmSvcUtil.exe.config"
    $config = [xml](Get-Content $configPath)

    $config.SelectSingleNode("/configuration/appSettings/add[@key='EntitiesWhitelist']").value = $entitiesWhitelist
    $config.SelectSingleNode("/configuration/appSettings/add[@key='AttributesWhitelist']").value = $attributesWhitelist

    $config.Save($configPath)
}

Set-Configuration

$namespace = "QualifiedTeachersApi.DataStore.Crm.Models"
$entitiesOutput = Join-Path -Path $PSScriptRoot -ChildPath ".." QualifiedTeachersApi src QualifiedTeachersApi DataStore Crm Models GeneratedCode.cs
$optionSetsOutput = Join-Path -Path $PSScriptRoot -ChildPath ".." QualifiedTeachersApi src QualifiedTeachersApi DataStore Crm Models GeneratedOptionSets.cs
mkdir (Split-Path $entitiesOutput) -Force | Out-Null

$crmSvcUtil = Join-Path -Path $coreToolsFolder -ChildPath "CrmSvcUtil.exe"

# entities
& $crmSvcUtil `
    /connectionstring:${connectionString} `
    /out:${entitiesOutput} `
    /namespace:${namespace} `
    /emitfieldsclasses `
    /SuppressGeneratedCodeAttribute `
    /serviceContextName:DqtCrmServiceContext `
    /codecustomization:"DLaB.CrmSvcUtilExtensions.Entity.CustomizeCodeDomService,DLaB.CrmSvcUtilExtensions" `
    /codegenerationservice:"DLaB.CrmSvcUtilExtensions.Entity.CustomCodeGenerationService,DLaB.CrmSvcUtilExtensions" `
    /codewriterfilter:"DLaB.CrmSvcUtilExtensions.Entity.CodeWriterFilterService,DLaB.CrmSvcUtilExtensions" `
    /namingservice:"DLaB.CrmSvcUtilExtensions.NamingService,DLaB.CrmSvcUtilExtensions" `
    /metadataproviderservice:"DLaB.CrmSvcUtilExtensions.Entity.MetadataProviderService,DLaB.CrmSvcUtilExtensions"

# option sets
if ($IncludeOptionSets -eq $true) {
    & $crmSvcUtil `
        /connectionstring:${connectionString} `
        /out:${optionSetsOutput} `
        /namespace:${namespace} `
        /SuppressGeneratedCodeAttribute `
        /codecustomization:"DLaB.CrmSvcUtilExtensions.OptionSet.CustomizeCodeDomService,DLaB.CrmSvcUtilExtensions" `
        /codegenerationservice:"DLaB.CrmSvcUtilExtensions.OptionSet.CustomCodeGenerationService,DLaB.CrmSvcUtilExtensions" `
        /codewriterfilter:"DLaB.CrmSvcUtilExtensions.OptionSet.CodeWriterFilterService,DLaB.CrmSvcUtilExtensions" `
        /namingservice:"DLaB.CrmSvcUtilExtensions.NamingService,DLaB.CrmSvcUtilExtensions" `
        /metadataproviderservice:"DLaB.CrmSvcUtilExtensions.BaseMetadataProviderService,DLaB.CrmSvcUtilExtensions"
}
