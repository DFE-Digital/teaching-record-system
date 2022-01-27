[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [String]$EnvironmentName,
    [Parameter(Mandatory = $true)]
    [String]$AzureSubscription,
    [Parameter(Mandatory = $true)]
    [String]$ConfigKey,
    [Parameter(Mandatory = $true)]
    $ConfigValue,
    [Parameter]
    [Switch]
    $Commit = $false
)

$ErrorActionPreference = "Stop"

function Invoke-NativeCommand() {
    $command = $args[0]
    $commandArgs = @()
    if ($args.Count -gt 1) {
        $commandArgs = $args[1..($args.Count - 1)]
    }

    & $command $commandArgs
    $result = $LASTEXITCODE

    if ($result -ne 0) {
        throw "$command $commandArgs exited with code $result."
    }
}

function Merge-Config($json, $path, $value) {
    $config = $json | ConvertFrom-Json -AsHashTable

    $pathComponents = $path -split '\.'
    $location = $config

    foreach ($component in $pathComponents) {
        if (!($location.ContainsKey($component))) {
            $location.Add($component, (ConvertFrom-Json '{}' -AsHashtable))
        }

        if ($component -eq $pathComponents[$pathComponents.Length - 1]) {
            $location."$component" = $value
        } else {
            $location = $location."$component"
        }
    }

    $config | ConvertTo-Json -Depth 100
}

$envConfigFile = Join-Path $PSScriptRoot "terraform" "${EnvironmentName}.tfvars.json"

if (!(Test-Path $envConfigFile)) {
    throw "Cannot find environment config file at '$envConfigFile'."
}

$envConfig = Get-Content $envConfigFile | ConvertFrom-Json
$keyVaultName = $envConfig.key_vault_name

$appConfig = Invoke-Command az keyvault secret show `
    --name "APP-CONFIG" `
    --vault-name $keyVaultName `
    --subscription $AzureSubscription `
    --query 'value' `
    --output tsv

$newConfig = Merge-Config $appConfig $ConfigKey $ConfigValue
$newConfig

if ($commit -eq $true) {
    $tempFile = New-TemporaryFile
    $newConfig | Out-File $tempFile

    try {
        Invoke-Command az keyvault secret set `
            --name "APP-CONFIG" `
            --file $tempFile `
            --vault-name $keyVaultName `
            --subscription $AzureSubscription `
    }
    finally {
        Remove-Item -Force $tempFile
    }

} else {
    Write-Information "Run again with the -Commit flag to update the Key Vault secret."
}
