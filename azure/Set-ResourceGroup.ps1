[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [String]$ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]$EnvironmentName,
    [Parameter(Mandatory = $true)]
    [String]$Subscription,
    [Parameter(Mandatory = $true)]
    [String]$ParametersFile,
    [Parameter(Mandatory = $true)]
    [String]$ServicePrincipalName
)

$ErrorActionPreference = "Stop"

$parentBusiness = "Teacher Training and Qualifications"
$portfolio = "Early Years and Schools Group"
$product = "Database of Qualified Teachers"
$service = "Teacher Training and Qualifications"
$serviceLine = "Teaching Workforce"
$serviceOffering = "Database of Qualified Teachers"

& az account set --subscription $Subscription

$spId = (& az ad sp list --display-name $ServicePrincipalName | ConvertFrom-Json).objectId

if ($null -eq $spId) {
    Write-Error "Cannot find service principal named: '$ServicePrincipalName'."
}

$rgExists = & az group exists --name $ResourceGroupName

if ($rgExists -ne "true") {
    & az group create `
        --location "West Europe" `
        --name $ResourceGroupName `
        --tags "Environment=${EnvironmentName}" "Parent Business=${parentBusiness}" "Portfolio=${portfolio}" "Product=${product}" "Service=${service}" "Service Line=${serviceLine}" "Service Offering=${serviceOffering}"
}

& az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file (Join-Path $PSScriptRoot "azuredeploy.json") `
    --parameters "@${ParametersFile}" `
    --parameters "keyVaultReaderServicePrincipalId=${spId}"
