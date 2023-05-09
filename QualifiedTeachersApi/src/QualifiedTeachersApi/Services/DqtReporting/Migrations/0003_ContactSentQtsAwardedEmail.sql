if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'dfeta_sentqtsawardemail')
	alter table contact add dfeta_sentqtsawardemail datetime