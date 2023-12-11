IF EXISTS (select 1 from information_schema.tables where table_name = 'GlobalOptionSetMetadata')
BEGIN
    update [dbo].[GlobalOptionSetMetadata] set LocalizedLabel = 'High potential ITT' where [Option] = 389040024 AND OptionSetName = 'dfeta_programmetype';
END
