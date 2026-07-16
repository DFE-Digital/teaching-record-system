#!/usr/bin/env -S dotnet --

var schemaVersionFilePath = Path.Join(Environment.CurrentDirectory, ".tests-schema-version.txt");

if (File.Exists(schemaVersionFilePath))
{
    File.Delete(schemaVersionFilePath);
}
