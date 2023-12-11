IF EXISTS (select 1 from information_schema.tables where table_name = 'GlobalOptionSetMetadata')
BEGIN
    INSERT INTO [dbo].[GlobalOptionSetMetadata](OptionSetName,[Option],IsUserLocalizedLabel,LocalizedLabelLanguageCode,LocalizedLabel) values('dfeta_programmetype',389040024,0,1033,'high potential ITT');
END
