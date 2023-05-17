if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msdyn_decisioninfluencetag')
    alter table contact add msdyn_decisioninfluencetag int

if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msdyn_isassistantinorgchart')
    alter table contact add msdyn_isassistantinorgchart bit
