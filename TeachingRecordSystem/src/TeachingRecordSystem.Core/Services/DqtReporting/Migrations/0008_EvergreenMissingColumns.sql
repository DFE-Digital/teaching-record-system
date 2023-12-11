--adx_createdbyipaddress
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'adx_createdbyipaddress')
    alter table account add adx_createdbyipaddress nvarchar(100)

--adx_createdbyusername
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'adx_createdbyusername')
    alter table account add adx_createdbyusername nvarchar(100)

--adx_modifiedbyipaddress
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'adx_modifiedbyipaddress')
    alter table account add adx_modifiedbyipaddress nvarchar(100)

--adx_modifiedbyusername
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'adx_modifiedbyusername')
    alter table account add adx_modifiedbyusername nvarchar(100)

--msa_managingpartnerid
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'msa_managingpartnerid')
    alter table account add msa_managingpartnerid uniqueidentifier

--msa_managingpartnerid_entitytype
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'msa_managingpartnerid_entitytype')
    alter table account add msa_managingpartnerid_entitytype nvarchar(128)

--msa_managingpartneridname
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'msa_managingpartneridname')
    alter table account add msa_managingpartneridname nvarchar(160)

--msa_managingpartneridyominame
if not exists (select 1 from information_schema.columns where table_name = 'account' and column_name = 'msa_managingpartneridyominame')
    alter table account add msa_managingpartneridyominame nvarchar(160)
