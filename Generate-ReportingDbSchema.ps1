$file = Join-Path $PSScriptRoot QualifiedTeachersApi src QualifiedTeachersApi Services DqtReporting Migrations 0001_Initial.sql
$qtCli = Join-Path $PSScriptRoot QualifiedTeachersApi src QtCli

$entityTypes = @(
    "account",
    "accountleads",
    "annotation",
    "campaign",
    "contact",
    "contactleads",
    "dfeta_autonumber",
    "dfeta_businesseventaudit",
    "dfeta_confirmationletter",
    "dfeta_country",
    "dfeta_countrystate",
    "dfeta_disability",
    "dfeta_document",
    "dfeta_earlyyearsstatus",
    "dfeta_employment",
    "dfeta_ethnicity",
    "dfeta_hequalification",
    "dfeta_hesaukprnmapping",
    "dfeta_hesubject",
    "dfeta_induction",
    "dfeta_inductionperiod",
    "dfeta_initialteachertraining",
    "dfeta_initiativecode",
    "dfeta_integrationtransaction",
    "dfeta_integrationtransactionrecord",
    "dfeta_ittqualification",
    "dfeta_ittsubject",
    "dfeta_legacyaudit",
    "dfeta_mqestablishment",
    "dfeta_mrapplication",
    "dfeta_mrcourse",
    "dfeta_mremployment",
    "dfeta_mrtraining",
    "dfeta_optionsetmapping",
    "dfeta_organisationcategory",
    "dfeta_prdeclinestatusreason",
    "dfeta_previousname",
    "dfeta_profileamendrequest",
    "dfeta_qtsregistration",
    "dfeta_qualification",
    "dfeta_sanction",
    "dfeta_sanctioncode",
    "dfeta_schooldirectinitiative",
    "dfeta_serviceannouncement",
    "dfeta_specialism",
    "dfeta_teacherstatus",
    "dfeta_temporaryqueries",
    "dfeta_tsstcohorts",
    "dfeta_tsstsubjects",
    "dfeta_webactivitylog",
    "incident",
    "incidentresolution",
    "lead",
    "subject",
    "systemuser",
    "transactioncurrency")

dotnet build $qtCli

Write-Output "" | Out-File $file

foreach ($entityType in $entityTypes) {
    dotnet "$qtCli/bin/Debug/net7.0/QtCli.dll" generate-reporting-db-table --entity-type $entityType | Out-File $file -Append
    Write-Output "" | Out-File $file -Append
}
