$ErrorActionPreference = "Stop"

$appDataDir = Join-Path -Path ([Environment]::GetFolderPath("ApplicationData")) "TeachingRecordSystem.Tests"
Get-ChildItem $appDataDir -Filter "*-dbversion.txt" | ForEach-Object { Remove-Item -Force $_ }
