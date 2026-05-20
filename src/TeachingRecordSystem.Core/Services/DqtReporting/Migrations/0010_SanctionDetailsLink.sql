if not exists (select 1 from information_schema.columns where table_name = 'dfeta_sanction' and column_name = 'dfeta_detailslink')
    alter table dfeta_sanction add dfeta_detailslink nvarchar(1000)
