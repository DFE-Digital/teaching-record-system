if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'dfeta_QtlsDateHasBeenSet')
    alter table contact add dfeta_QtlsDateHasBeenSet bit;
