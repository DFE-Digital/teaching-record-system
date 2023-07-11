if not exists (select 1 from information_schema.columns where table_name = 'incident' and column_name = 'msdyn_copilotengaged')
    alter table incident add msdyn_copilotengaged bit
